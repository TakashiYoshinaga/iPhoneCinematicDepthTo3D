using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace LookingGlass.Editor {
    [Serializable]
    public class HoloplayPreviewWindow : ScriptableObject {
        private const string WindowNamePrefix = "HoloPlay Game View ";

        public static HoloplayPreviewWindow Create(Holoplay holoplay) =>
            Create(holoplay, null);

        public static HoloplayPreviewWindow Create(Holoplay holoplay, EditorWindow gameView) {
            if (holoplay == null)
                throw new ArgumentNullException(nameof(holoplay));

            HoloplayPreviewWindow preview = ScriptableObject.CreateInstance<HoloplayPreviewWindow>();

            //Accounts for the case that the HoloplayPreviewWindow destroys itself during Awake or OnEnable
            if (preview == null) {
                Debug.LogWarning("The preview window destroyed itself before being able to be used!");
                return null;
            }

            holoplay.RenderBlack = true;
            if (gameView == null) {
                gameView = (EditorWindow) EditorWindow.CreateInstance(GameViewExtensions.GameViewType);
#if UNITY_EDITOR_WIN
                gameView.Show();
#else
                //NOTE: On MacOS Big Sur (Intel) with Unity 2019.4, there was a 25px bottom-bar of unknown origin messing up the preview window.
                //This weird bottom bar did NOT occur on Unity 2018.4.
                //Either way, ShowUtility() made this issue go away.
                gameView.ShowUtility();
#endif
            }
            preview.gameView = gameView;

            string name = WindowNamePrefix + "";
            gameView.titleContent = new GUIContent(name);
            preview.name = name;

            if (all == null)
                all = new List<HoloplayPreviewWindow>();

            all.Add(preview);
            if (all.Count == 1) {
                EditorApplication.wantsToQuit += CloseAllAndAcceptQuit;
            }

            try {
                preview.InitializeWithHoloplay(holoplay);

                //WARNING: Do NOT call this here -- it's too early. The GameView's GetZoomAreaSize() may return small values like 320x533 for some odd reason.
                //preview.UpdateFromResolutionIfNeeded();

                gameView.SetGameViewZoom();
            } catch (Exception e) {
                Debug.LogException(e);
                ScriptableObject.DestroyImmediate(preview);
                return null;
            }

            EditorApplication.update += preview.OnUpdate;
            return preview;
        }

        private static List<HoloplayPreviewWindow> all;
        private static List<WindowsOSMonitor> monitors;
        private static bool supportsExperimentalDisplayDPIScaling =
#if UNITY_EDITOR_WIN
            true
#else
            false
#endif
            ;

        public static int Count => all?.Count ?? 0;
        public static IEnumerable<HoloplayPreviewWindow> All {
            get {
                if (all == null)
                    yield break;
                foreach (HoloplayPreviewWindow preview in all)
                    yield return preview;
            }
        }

        [SerializeField] private EditorWindow gameView;
        [SerializeField] private Holoplay holoplay;
        [SerializeField] private WindowsOSMonitor matchingMonitor;

        private bool setCustomRenderSize = false;
        private Rect lastPrintedPosition;
        private int frameCount = 0;
        private bool usedManualPreview = false;

        public Holoplay Holoplay => holoplay;
        public EditorWindow GameView => gameView;

        #region Unity Messages
        //NOTE: For some reason, Unity auto-destroys this ScriptableObject when loading a new scene..
        private void OnDestroy() {
            EditorApplication.update -= OnUpdate;

            if (gameView != null) {
                gameView.Close();
                EditorWindow.DestroyImmediate(gameView);
            }

            if (holoplay != null) {
                holoplay.ClearCustomRenderingResolution();
                holoplay.RenderBlack = false;
            }

            if (all != null) {
                all.Remove(this);
                if (all.Count == 0) {
                    EditorApplication.wantsToQuit -= CloseAllAndAcceptQuit;
                }
            }
        }
        #endregion

        private void OnUpdate() {
            if (holoplay == null) {
                Debug.LogWarning("The target " + nameof(Holoplay) + " component was destroyed. Closing its preview window.");
                DestroyImmediate(this);
                return;
            }

            if (gameView == null) {
                Debug.LogWarning("The editor preview window was closed.");
                DestroyImmediate(this);
                return;
            }

            if (frameCount < 2) {
                Rect position = gameView.position;
                if (position != lastPrintedPosition) {
                    lastPrintedPosition = position;
                    try {
                        InitializeWithHoloplay(holoplay);
                        UpdateFromResolutionIfNeeded();
                    } catch (Exception e) {
                        Debug.LogError("An error occurred while updating the preview window! It will be closed.");
                        Debug.LogException(e);
                        DestroyImmediate(this);
                        return;
                    }
                }
            }
            if (frameCount == 5) {
                holoplay.RenderBlack = false;
                if (holoplay.Preview2D)
                    holoplay.RenderPreview2D();
                else
                    holoplay.RenderQuilt();
                EditorApplication.QueuePlayerLoopUpdate();
                gameView.Repaint();
            }

            frameCount++;
        }

        private static bool CloseAllAndAcceptQuit() {
            CloseAll();
            return true;
        }

        private static void CloseAll() {
            if (all == null)
                return;

            for (int i = all.Count - 1; i >= 0; i--) {
                HoloplayPreviewWindow preview = all[i];
                preview.gameView.Close();
                ScriptableObject.DestroyImmediate(preview);
            }
        }

        private void InitializeWithHoloplay(Holoplay holoplay) {
            Assert.IsNotNull(holoplay);
            this.holoplay = holoplay;
            holoplay.ClearCustomRenderingResolution();
            setCustomRenderSize = false;

            Calibration cal = holoplay.Calibration;
            Vector2 position;
            Vector2 size;

            RectInt calibratedRect = new RectInt(cal.xpos, cal.ypos, cal.screenWidth, cal.screenHeight);
            int indexInList = -1;

            if (supportsExperimentalDisplayDPIScaling) {
                if (monitors == null)
                    monitors = new List<WindowsOSMonitor>();
                else
                    monitors.Clear();
                monitors.AddRange(WindowsOSMonitor.GetAll());
                indexInList = monitors.FindIndex((WindowsOSMonitor monitor) => monitor.NonScaledRect.Equals(calibratedRect));
            }

            if (indexInList >= 0) {
                matchingMonitor = monitors[indexInList];
                position = matchingMonitor.ScaledRect.position;
                size = matchingMonitor.ScaledRect.size;
            } else {
                if (supportsExperimentalDisplayDPIScaling)
                    Debug.LogWarning("Unable to find a monitor matching the unscaled rect of " + calibratedRect + " from HoPS calibration data. " +
                        "The preview window might not handle DPI screen scaling properly.");

                position = new Vector2(cal.xpos, cal.ypos);
                size = new Vector2(cal.screenWidth, cal.screenHeight);
            }

            //NOTE: When testing different resolutions, we must currently anchor the preview window to the bottom-left of the LKG device's screen.
            //This keeps the center visually consistent.
            //We've never tried to recalculate center values in the calibration data, though it might be possible.

            if (Preview.UseManualPreview) {
                ManualPreviewSettings settings = Preview.ManualPreviewSettings;
                position = settings.position;
                size = settings.resolution;
                usedManualPreview = true;
            }

            //NOTE: Do we need this logic?
//#if UNITY_EDITOR_WIN
//            // Account for display scaling, but it is not working in 2020.3
//            position.x *= 96 / Mathf.RoundToInt(Screen.dpi);
//            position.y *= 96 / Mathf.RoundToInt(Screen.dpi);
//#endif

            //After a few frames, we need to re-check to see what Unity allowed our position rect to be!
            //It will automatically resize to avoid going outside the screen, or overlapping the Windows taskbar.
            Rect idealRect = new Rect(position, size);

            //The default maxSize is usually good enough (Unity mentions 4000x4000),
            //But if we're on an 8K LKG device, this isn't large enough!
            //Just to be sure, let's check our maxSize is large enough for the ideal rect we want to set our size to:
            Vector2 prevMaxSize = gameView.maxSize;
            if (prevMaxSize.x < idealRect.width ||
                prevMaxSize.y < idealRect.height)
                gameView.maxSize = idealRect.size;
            
            if (frameCount < 1)
                gameView.position = idealRect;

            if (!usedManualPreview) {
                //THIS ONLY WORKS WHEN DOCKED: Which never helps us lol..
                //gameView.maximized = true;

                //INSTEAD, let's do:
                gameView.AutoClickMaximizeButtonOnWindows();
            }

            gameView.SetFreeAspectSize();
            gameView.SetShowToolbar(false);
        }

        private void UpdateFromResolutionIfNeeded() {
            Rect position = gameView.position;
            Calibration cal = holoplay.Calibration;
            Calibration unmodifiedCal = holoplay.UnmodifiedCalibration;
            Vector2 area = gameView.GetTargetSize();

            //These 2 variables are used for the custom rendering resolution:
            Vector2Int customPos;
            Vector2Int customSize;

            if (supportsExperimentalDisplayDPIScaling) {
                //NOTE: The calibration works when using NON-scaled pixel coordinate values.
                //Even though this EditorWindow needs SCALED pixel coordinate values.
                Vector2Int scaledPos = new Vector2Int(
                    (int) position.x + (int) area.x,
                    (int) position.y + (int) area.y
                );

                Vector2Int scaledOffset = scaledPos - matchingMonitor.ScaledRect.position;
                Vector2Int unscaledOffset = Vector2Int.RoundToInt(matchingMonitor.UnscalePoint(scaledOffset));
                Vector2Int unscaledPos = unscaledOffset + matchingMonitor.NonScaledRect.position;

                Vector2Int scaledSize = new Vector2Int(
                    (int) area.x,
                    (int) area.y
                );

                Vector2Int unscaledSize = Vector2Int.RoundToInt(matchingMonitor.UnscalePoint(scaledSize));

                //Calibration uses NON-scaled values:
                customPos = unscaledPos;
                customSize = unscaledSize;
            } else {
                customPos = new Vector2Int(
                    (int) position.x,
                    (int) position.y
                );

                customSize = new Vector2Int(
                    (int) area.x,
                    (int) area.y
                );
            }

            if (!setCustomRenderSize &&
                (customPos.x != cal.xpos ||
                customPos.y != cal.ypos ||
                customSize.x != holoplay.ScreenWidth ||
                customSize.y != holoplay.ScreenHeight)) {
                holoplay.UseCustomRenderingResolution(customPos.x, customPos.y, customSize.x, customSize.y);
                holoplay.RenderQuilt(forceRender: true);
                setCustomRenderSize = true;
            }

            gameView.SetGameViewTargetDisplay((int) holoplay.TargetDisplay);
            gameView.SetGameViewZoom();
        }
    }
}
