//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LookingGlass {
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [HelpURL("https://docs.lookingglassfactory.com/Unity/Scripts/Holoplay/")]
    public partial class Holoplay : MonoBehaviour {
        private static List<Holoplay> all = new List<Holoplay>(8);

        /// <summary>
        /// Called when <see cref="Holoplay.All"/> is updated due to OnEnable or OnDisable on any <see cref="Holoplay"/> component.
        /// </summary>
        public static event Action onListChanged;

        internal static event Action<Holoplay> onQuiltSettingsChanged;

        /// <summary>
        /// The most-recently enabled <see cref="Holoplay"/> component, or <c>null</c> if there is none.
        /// </summary>
        public static Holoplay Instance {
            get {
                if (all == null || all.Count <= 0)
                    return null;
                return all[all.Count - 1];
            }
        }

        public static int Count => all.Count;
        public static bool AnyEnabled => all.Count > 0;
        public static Holoplay Get(int index) => all[index];
        public static IEnumerable<Holoplay> All {
            get {
                foreach (Holoplay h in all)
                    yield return h;
            }
        }

        private static void RegisterToList(Holoplay holoplay) {
            Assert.IsNotNull(holoplay);
            all.Add(holoplay);
            onListChanged?.Invoke();
        }

        private static void UnregisterFromList(Holoplay holoplay) {
            Assert.IsNotNull(holoplay);
            all.Remove(holoplay);
            onListChanged?.Invoke();
        }

#if UNITY_POST_PROCESSING_STACK_V2
        internal static HoloplayPostProcessSetup postProcessSetup;
        internal static HoloplayPostProcessDispose postProcessDispose;
#endif

        internal const string SingleViewCameraName = "Single-View Camera";
        private const string FinalScreenBlitterCameraName = "Final Screen Camera";

        private static readonly Quilt.Preset DefaultQuiltPreset = Quilt.Preset.Automatic;
        private static readonly Quilt.Settings DefaultCustomQuiltSettings = Quilt.GetSettings(HoloplayDevice.Type.Portrait);

        [SerializeField, HideInInspector] private SerializableVersion lastSavedVersion;

        [Space(20)]
        //TODO: [HPU~0] Remove this Hungarian notation when we remove the obsolete camelCase member!
        [Tooltip("The Unity display that the LKG device associated with this component will render to.")]
        [FormerlySerializedAs("targetDisplay")]
        [SerializeField] private DisplayTarget m_TargetDisplay;
        
        //NOTE: Data duplication below for lkgName and lkgIndex, because we need to save (serialize) them! (Because the calibration data is NOT serialized)
        [Tooltip("The name of the looking glass (LKG) device that this component is connected with.\n\n" +
            "A " + nameof(Holoplay) + " component is only connected to 1 device at a time.")]
        [SerializeField] private string targetLKGName;
        
        [Tooltip("The index of the looking glass (LKG) device that this component is connected with.\n\n" +
            "A " + nameof(Holoplay) + " component is only connected to 1 device at a time.")]
        [SerializeField] private int targetLKGIndex;
        
        [Tooltip("The type of device that is being emulated right now, since there are no LKG devices plugged in or recognized.")]
        [SerializeField] private HoloplayDevice.Type emulatedDevice = HoloplayDevice.Type.Portrait;

        [SerializeField] private bool preview2D = false;

        [Space(20)]
        [SerializeField] private Quilt.Preset quiltPreset = Quilt.Preset.Automatic;
        [SerializeField] private Quilt.Settings customQuiltSettings = Quilt.GetSettings(HoloplayDevice.Type.Portrait);
        [FormerlySerializedAs("quiltRT")]
        [SerializeField] private RenderTexture quiltTexture;
        [SerializeField] private bool saveQuilt;

        //TODO: [HPU~0] Remove this Hungarian notation when we remove the obsolete camelCase member!
        [FormerlySerializedAs("overrideQuilt")]
        [SerializeField] private Texture m_OverrideQuilt;
        [SerializeField] private bool renderOverrideBehind;

        [Space(20)]
        [Tooltip("Pressing this key will take a 2D screenshot, which will automatically save as an image file to the root directory of your project.")]
        [SerializeField] private KeyCode screenshot2DKey = KeyCode.F9;
        [Tooltip("Pressing this key will take a quilt screenshot, which will automatically save as an image file to the root directory of your project.")]
        [SerializeField] private KeyCode screenshotQuiltKey = KeyCode.F10;

        [SerializeField, HideInInspector] private HoloplayCameraData cameraData = new HoloplayCameraData();
        [SerializeField, HideInInspector] private HoloplayGizmos gizmos = new HoloplayGizmos();
        [SerializeField, HideInInspector] private HoloplayEvents events = new HoloplayEvents();
        [SerializeField, HideInInspector] private HoloplayOptimizationData optimization = new HoloplayOptimizationData();
        [SerializeField, HideInInspector] private HoloplayDebugging debugging = new HoloplayDebugging();

        private Camera singleViewCamera;
        private Camera postProcessCamera;
        private Camera finalScreenCamera;
        private HoloplayScreenBlitter screenBlitter;

        [NonSerialized] private bool hadPreview2D = false; //Used for detecting changes in the editor
        [NonSerialized] private bool wasSavingQuilt;

        [NonSerialized] private Material lightfieldMaterial;
        [NonSerialized] public LoadResults loadResults;
        [NonSerialized] public Calibration cal;
        
        //TODO: HoPS infrastructure improvements to avoid us from needing to subvert the
        //calibration provided by HoPS in this UnityPlugin! (See also: Calibration.cs)
        private Calibration unmodifiedCalibration;
        
        private float cameraDistance;

        private bool isUsingCustomRenderResolution;
        private int customXPos;
        private int customYPos;
        private int customRenderWidth;
        private int customRenderHeight;

        private bool frameRendered;
        private bool frameRendered2DPreview;
        private bool debugInfo;

        private RenderTexture preview2DRT;
        private RenderTexture singleViewRT;
        private bool renderBlack = false;
        private HoloplayGameViewResolutionType gameViewResolutionType;

        public event Action onTargetDisplayChanged;
        public event Action onQuiltChanged;
        public event Action onCalibrationReloaded;

        public bool Preview2D {
            get { return preview2D; }
            set {
                hadPreview2D = preview2D = value;

                //If we need anything to change immediately when setting Preview2D, we can do that here
            }
        }

        public HoloplayCameraData CameraData => cameraData;
        public HoloplayGizmos Gizmos => gizmos;
        public HoloplayEvents Events => events;
        public HoloplayOptimizationData Optimization => optimization;
        public HoloplayDebugging Debugging => debugging;

        #region DEPRECATED: v1.5.0
        [Obsolete("Use Holoplay.TargetDisplay instead.")]
        public int targetDisplay {
            get { return (int) TargetDisplay; }
            set { TargetDisplay = (DisplayTarget) value; }
        }

        [Obsolete("Use Holoplay.QuiltSettings instead.")]
        public Quilt.Settings quiltSettings {
            get { return QuiltSettings; }
        }

        [Obsolete("Use Holoplay.OverrideQuilt instead.")]
        public Texture overrideQuilt {
            get { return OverrideQuilt; }
            set { OverrideQuilt = value; }
        }

        [Obsolete("Use Holoplay.CameraDistance instead.")]
        public float camDist {
            get { return CameraDistance; }
        }

        [Obsolete("Use Holoplay.LightfieldMaterial instead.")]
        public Material lightfieldMat {
            get { return LightfieldMaterial; }
        }

        [Obsolete("Use Holoplay.CameraData.ClearFlags instead.")]
        public CameraClearFlags clearFlags {
            get { return cameraData.ClearFlags; }
            set { cameraData.ClearFlags = value; }
        }
        [Obsolete("Use Holoplay.CameraData.BackgroundColor instead.")]
        public Color background {
            get { return cameraData.BackgroundColor; }
            set { cameraData.BackgroundColor = value; }
        }
        [Obsolete("Use Holoplay.CameraData.CullingMask instead.")]
        public LayerMask cullingMask {
            get { return cameraData.CullingMask; }
            set { cameraData.CullingMask = value; }
        }
        [Obsolete("Use Holoplay.CameraData.FieldOfView instead.")]
        public float fov {
            get { return cameraData.FieldOfView; }
            set { cameraData.FieldOfView = value; }
        }
        [Obsolete("Use Holoplay.CameraData.Depth instead.")]
        public float depth {
            get { return cameraData.Depth; }
            set { cameraData.Depth = value; }
        }
        [Obsolete("Use Holoplay.CameraData.RenderingPath instead.")]
        public RenderingPath renderingPath {
            get { return cameraData.RenderingPath; }
            set { cameraData.RenderingPath = value; }
        }
        [Obsolete("Use Holoplay.CameraData.UseOcclusionCulling instead.")]
        public bool occlusionCulling {
            get { return cameraData.UseOcclusionCulling; }
            set { cameraData.UseOcclusionCulling = value; }
        }
        [Obsolete("Use Holoplay.CameraData.AllowHDR instead.")]
        public bool allowHDR {
            get { return cameraData.AllowHDR; }
            set { cameraData.AllowHDR = value; }
        }
        [Obsolete("Use Holoplay.CameraData.AllowMSAA instead.")]
        public bool allowMSAA {
            get { return cameraData.AllowMSAA; }
            set { cameraData.AllowMSAA = value; }
        }
#if UNITY_2017_3_OR_NEWER
        [Obsolete("Use Holoplay.CameraData.AllowDynamicResolution instead.")]
        public bool allowDynamicResolution {
            get { return cameraData.AllowDynamicResolution; }
            set { cameraData.AllowDynamicResolution = value; }
        }
#endif

        [Obsolete("Use Holoplay.Gizmos.FrustumColor instead.")]
        public Color frustumColor {
            get { return gizmos.FrustumColor; }
            set { gizmos.FrustumColor = value; }
        }
        [Obsolete("Use Holoplay.Gizmos.MiddlePlaneColor instead.")]
        public Color middlePlaneColor {
            get { return gizmos.MiddlePlaneColor; }
            set { gizmos.MiddlePlaneColor = value; }
        }
        [Obsolete("Use Holoplay.Gizmos.HandleColor instead.")]
        public Color handleColor {
            get { return gizmos.HandleColor; }
            set { gizmos.HandleColor = value; }
        }
        [Obsolete("Use Holoplay.Gizmos.DrawHandles instead.")]
        public bool drawHandles {
            get { return gizmos.DrawHandles; }
            set { gizmos.DrawHandles = value; }
        }

        [Obsolete("Use Holoplay.CameraData.Size instead.")]
        public float size {
            get { return cameraData.Size; }
            set { cameraData.Size = value; }
        }
        [Obsolete("Use Holoplay.CameraData.NearClipFactor instead.")]
        public float nearClipFactor {
            get { return cameraData.NearClipFactor; }
            set { cameraData.NearClipFactor = value; }
        }
        [Obsolete("Use Holoplay.CameraData.FarClipFactor instead.")]
        public float farClipFactor {
            get { return cameraData.FarClipFactor; }
            set { cameraData.FarClipFactor = value; }
        }
        [Obsolete("Use Holoplay.CameraData.ScaleFollowsSize instead.")]
        public bool scaleFollowsSize {
            get { return cameraData.ScaleFollowsSize; }
            set { cameraData.ScaleFollowsSize = value; }
        }
        [Obsolete("Use Holoplay.CameraData.ViewconeModifier instead.")]
        public float viewconeModifier {
            get { return cameraData.ViewconeModifier; }
            set { cameraData.ViewconeModifier = value; }
        }
        [Obsolete("Use Holoplay.CameraData.CenterOffset instead.")]
        public float centerOffset {
            get { return cameraData.CenterOffset; }
            set { cameraData.CenterOffset = value; }
        }
        [Obsolete("Use Holoplay.CameraData.HorizontalFrustumOffset instead.")]
        public float horizontalFrustumOffset {
            get { return cameraData.HorizontalFrustumOffset; }
            set { cameraData.HorizontalFrustumOffset = value; }
        }
        [Obsolete("Use Holoplay.CameraData.VerticalFrustumOffset instead.")]
        public float verticalFrustumOffset {
            get { return cameraData.VerticalFrustumOffset; }
            set { cameraData.VerticalFrustumOffset = value; }
        }
        [Obsolete("Use Holoplay.CameraData.UseFrustumTarget instead.")]
        public bool useFrustumTarget {
            get { return cameraData.UseFrustumTarget; }
            set { cameraData.UseFrustumTarget = value; }
        }
        [Obsolete("Use Holoplay.CameraData.FrustumTarget instead.")]
        public Transform frustumTarget {
            get { return cameraData.FrustumTarget; }
            set { cameraData.FrustumTarget = value; }
        }

        [Obsolete("Use Holoplay.Optimization.viewInterpolation instead.")]
        public ViewInterpolationType viewInterpolation {
            get { return optimization.ViewInterpolation; }
            set { optimization.ViewInterpolation = value; }
        }
        [Obsolete("Use Holoplay.Optimization.ReduceFlicker instead.")]
        public bool reduceFlicker {
            get { return optimization.ReduceFlicker; }
            set { optimization.ReduceFlicker = value; }
        }
        [Obsolete("Use Holoplay.Optimization.FillGaps instead.")]
        public bool fillGaps {
            get { return optimization.FillGaps; }
            set { optimization.FillGaps = value; }
        }
        [Obsolete("Use Holoplay.Optimization.BlendViews instead.")]
        public bool blendViews {
            get { return optimization.BlendViews; }
            set { optimization.BlendViews = value; }
        }

        [Obsolete("Use Holoplay.Events.OnHoloplayReady instead.")]
        public HoloplayLoadEvent onHoloplayReady {
            get { return events.OnHoloplayReady; }
            set { events.OnHoloplayReady = value; }
        }
        [Obsolete("Use Holoplay.Events.OnViewRender instead.")]
        public HoloplayViewRenderEvent onViewRender {
            get { return events.OnViewRender; }
            set { events.OnViewRender = value; }
        }
        #endregion


        public DisplayTarget TargetDisplay {
            get { return m_TargetDisplay; }
            set {
                m_TargetDisplay = value;
                if (finalScreenCamera != null)
                    finalScreenCamera.targetDisplay = (int) m_TargetDisplay;
            }
        }

        public bool HasTargetDevice => !string.IsNullOrWhiteSpace(targetLKGName);

        public string TargetLKGName {
            get { return targetLKGName; }
            set {
                targetLKGName = value;
                ReloadCalibrationByName();
            }
        }

        public int TargetLKGIndex {
            get { return targetLKGIndex; }
            set {
                targetLKGIndex = value;
                ReloadCalibrationByIndex();
            }
        }

        public HoloplayDevice.Type EmulatedDevice {
            get { return emulatedDevice; }
            set {
                emulatedDevice = value;
                cameraData.NearClipFactor = HoloplayDevice.GetSettings(emulatedDevice).nearClip;
                quiltPreset = Quilt.Preset.Automatic;
                ReloadCalibrationByName();
            }
        }

        public Quilt.Preset QuiltPreset {
            get { return quiltPreset; }
            set {
                quiltPreset = value;
                SetupQuilt();
                OnQuiltSettingsChanged();
            }
        }

        public Quilt.Settings QuiltSettings {
            get {
                Quilt.Settings result = (quiltPreset == Quilt.Preset.Custom) ? customQuiltSettings : Quilt.GetSettings(quiltPreset, unmodifiedCalibration);

                if (NeedsQuiltResetup(result))
                    SetupQuilt(result);
                return result;
            }
        }

        public Quilt.Settings CustomQuiltSettings {
            get { return customQuiltSettings; }
            set {
                customQuiltSettings = value;
                SetupQuilt();
                OnQuiltSettingsChanged();
            }
        }

        public bool HasOverrideQuilt => m_OverrideQuilt != null;
        public Texture OverrideQuilt {
            get { return m_OverrideQuilt; }
            set { m_OverrideQuilt = value; }
        }

        public bool RenderOverrideBehind {
            get { return renderOverrideBehind; }
            set { renderOverrideBehind = true; }
        }

        public KeyCode Screenshot2DKey {
            get { return screenshot2DKey; }
            set { screenshot2DKey = value; }
        }

        public KeyCode ScreenshotQuiltKey {
            get { return screenshotQuiltKey; }
            set { screenshotQuiltKey = value; }
        }

        public int ScreenWidth => loadResults.calibrationFound ? cal.screenWidth : HoloplayDevice.GetSettings(emulatedDevice).screenWidth;
        public int ScreenHeight => loadResults.calibrationFound ? cal.screenHeight : HoloplayDevice.GetSettings(emulatedDevice).screenHeight;

        public float Aspect {
            get {
                return loadResults.calibrationFound ?
                    cal.GetAspect() :
                    HoloplayDevice.GetSettings(emulatedDevice).aspectRatio;
            }
        }

        public string DeviceTypeName => loadResults.calibrationFound ? HoloplayDevice.GetName(cal) : HoloplayDevice.GetSettings(emulatedDevice).name;

        public RenderTexture QuiltTexture {
            get {
                if (NeedsQuiltResetup())
                    SetupQuilt();
                Assert.IsNotNull(quiltTexture);
                return quiltTexture;
            }
        }

        /// <summary>
        /// The material with the lightfield shader, used in the final graphics blit to the screen.
        /// It accepts the quilt texture as its main texture.
        /// </summary>
        public Material LightfieldMaterial {
            get {
                if (lightfieldMaterial == null)
                    CreateLightfieldMaterial();
                return lightfieldMaterial;
            }
        }

        public bool SaveQuilt {
            get { return saveQuilt; }
            set {
                wasSavingQuilt = saveQuilt = value;
#if UNITY_EDITOR
                if (saveQuilt && isActiveAndEnabled)
                    SaveQuiltAsset();
#endif
            }
        }

        //How the cameras work:
        //1. The finalScreenCamera begins rendering automatically, since it is enabled.
        //2. The singleViewCamera renders into RenderTextures,
        //        either for rendering the quilt, or the 2D preview.
        //3. Then, the postProcessCamera is set to render no Meshes, and discards its own RenderTexture source.
        //        INSTEAD, it takes a RenderTexture (quiltRT) from Holoplay.cs and blits it with the lightfield shader back into the RenderTexture.
        //4. Finally, the finalScreenCamera blits the result ONTO THE SCREEN.(A camera required for that), since its targetTexture is always null.

        /// <summary>
        /// <para>Renders individual views of the scene, where each view may be composited into the <see cref="Holoplay"/> quilt.</para>
        /// <para>When in 2D preview mode, only 1 view is rendered directly to the screen.</para>
        /// <para>This camera is not directly used for rendering to the screen. The results of its renders are used as intermediate steps in the rendering process.</para>
        /// </summary>
        public Camera SingleViewCamera => singleViewCamera;

        /// <summary>
        /// <para>The <see cref="Camera"/> used apply final post-processing to a single view of the scene, or a quilt of the scene.</para>
        /// <para>This camera is not directly used for rendering to the screen. It is only used for applying graphical changes in internal <see cref="RenderTexture"/>s.</para>
        /// </summary>
        public Camera PostProcessCamera => postProcessCamera;

        /// <summary>
        /// The camera used for blitting the final <see cref="RenderTexture"/> to the screen.<br />
        /// In Unity, the easiest and best-supported way to do this is by using a Camera directly.
        /// </summary>
        internal Camera FinalScreenCamera => finalScreenCamera;
        internal HoloplayScreenBlitter ScreenBlitter => screenBlitter;

        public RenderTexture Preview2DRT {
            get {
                if (preview2DRT == null || !frameRendered2DPreview)
                    RenderPreview2D();
                return preview2DRT;
            }
        }

        public Calibration Calibration {
            get { return cal; }
        }

        public Calibration UnmodifiedCalibration => unmodifiedCalibration;

        public float CameraDistance => cameraDistance;

        /// <summary>
        /// Defines whether or not the <see cref="Calibration"/> is temporarily modified to fit
        /// a width and height that's different from the target LKG device's native resolution.<br />
        /// When this is true, the <see cref="Calibration"/>'s <see cref="Calibration.screenWidth"/> and <see cref="Calibration.screenHeight"/>
        /// will be modified, as well as any other calibration fields that depend on those two fields.<br /><br />
        /// 
        /// Currently, this is only an internal, editor-only feature used to render the preview window in the Unity editor, due to the following:
        /// <list type="bullet">
        /// <item>The preview's editor window title bar can't easily and consistently be hidden across Windows, MacOS, and Linux.</item>
        /// <item>The OS task bar may prevent the window from becoming full-screen native resolution on the LKG display.</item>
        /// </list>
        /// <para>See also: <seealso cref="Calibration"/>, <seealso cref="UnmodifiedCalibration"/></para>
        /// </summary>
        public bool IsUsingCustomResolution {
            get {
                if (!Application.isEditor)
                    Assert.IsFalse(isUsingCustomRenderResolution, "The custom resolution feature has not yet been intended for or tested to work in builds!");
                return isUsingCustomRenderResolution;
            }
        }

        internal bool RenderBlack {
            get { return renderBlack; }
            set { renderBlack = value; }
        }

        public HoloplayGameViewResolutionType GameViewResolutionType => gameViewResolutionType;

        private Holoplay() {
            InitSections();
        }

        #region Unity Messages
        private void OnValidate() {
            if (preview2D != hadPreview2D)
                Preview2D = preview2D;
            if (saveQuilt != wasSavingQuilt)
                SaveQuilt = saveQuilt;
        }

        private void Awake() {
            InitSections();
        }

        private void OnEnable() {
            InitSections();
            RegisterToList(this);

#if !UNITY_2018_1_OR_NEWER || !UNITY_EDITOR
            PluginCore.Reset();
#endif

            SetupAllCameras();

            debugging.onShowAllObjectsChanged -= SetAllObjectHideFlags;
            debugging.onShowAllObjectsChanged += SetAllObjectHideFlags;
            SetAllObjectHideFlags();

            if (lightfieldMaterial == null)
                CreateLightfieldMaterial();

            Preview2D = preview2D;
            ReloadCalibrationByName();

            if (!Application.isEditor) {
                //NOTE: This is REQUIRED for using Display.SetParams(...)!
                //See Unity docs on this at: https://docs.unity3d.com/ScriptReference/Display.SetParams.html

                //NOTE: WITHOUT this line, subsequent calls to Display.displays[0].SetParams(...) HAVE NO EFFECT!
                Display.displays[0].Activate();
#if UNITY_STANDALONE_WIN
                Display.displays[0].SetParams(cal.screenWidth, cal.screenHeight, cal.xpos, cal.ypos);
#endif
            }

            //This sets up the window to play on the looking glass,
            //NOTE: This must be executed after display reposition
            //YAY! This FIXED the issue with cal.screenHeight or 0 as the SetParams height making the window only go about half way down the screen!
            //This also lets the lenticular shader render properly!
            Screen.SetResolution(cal.screenWidth, cal.screenHeight, true);

            SetupQuilt();

            events.OnHoloplayReady?.Invoke(loadResults);
        }

        private void OnDisable() {
            debugging.onShowAllObjectsChanged -= SetAllObjectHideFlags;
            UnregisterFromList(this);

            if (lightfieldMaterial != null)
                DestroyImmediate(lightfieldMaterial);
            if (quiltTexture != null)
                quiltTexture.Release();
            if (preview2DRT != null)
                preview2DRT.Release();
            if (finalScreenCamera != null)
                DestroyImmediate(finalScreenCamera.gameObject);

#if UNITY_POST_PROCESSING_STACK_V2
            postProcessDispose?.Invoke(this);
#endif


#if !UNITY_2018_1_OR_NEWER || !UNITY_EDITOR
            PluginCore.Reset();
#endif
        }

        private void OnDestroy() {
#if !UNITY_2018_1_OR_NEWER || !UNITY_EDITOR
            PluginCore.Reset();
#endif
        }

        private void Update() {
            frameRendered = false;
            frameRendered2DPreview = false;

            if (Input.GetKeyDown(screenshot2DKey))
                SaveAsPNGScreenshot(RenderPreview2D());

            if (Input.GetKeyDown(screenshotQuiltKey))
                SaveAsPNGScreenshot(QuiltTexture);

            if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.F8))
                debugInfo = !debugInfo;
            if (Input.GetKeyDown(KeyCode.Escape))
                debugInfo = false;

            UpdateLightfieldMaterial();
        }

        private void LateUpdate() {
            if (RenderPipelineUtil.IsHDRP) {
                RenderQuilt();
            }

            ResetCamera();
        }

        private void OnGUI() {
            if (debugInfo) {
                Color previousColor = GUI.color;

                // start drawing stuff
                int unitDiv = 20;
                int unit = Mathf.Min(Screen.width, Screen.height) / unitDiv;
                Rect rect = new Rect(unit, unit, unit * (unitDiv - 2), unit * (unitDiv - 2));

                GUI.color = Color.black;
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                rect = new Rect(unit * 2, unit * 2, unit * (unitDiv - 4), unit * (unitDiv - 4));

                GUILayout.BeginArea(rect);
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = unit;
                GUI.color = new Color(0.5f, 0.8f, 0.5f, 1);

                GUILayout.Label("Holoplay SDK " + Version.ToString() + VersionLabel, labelStyle);
                GUILayout.Space(unit);
                GUI.color = loadResults.calibrationFound ? new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);
                GUILayout.Label("calibration: " + (loadResults.calibrationFound ? "loaded" : "not found"), labelStyle);

                //TODO: This is giving a false positive currently
                //GUILayout.Space(unit);
                //GUI.color = new Color(0.5f, 0.5f, 0.5f, 1);
                //GUILayout.Label("lkg display: " + (loadResults.lkgDisplayFound ? "found" : "not found"), labelStyle);

                GUILayout.EndArea();

                GUI.color = previousColor;
            }
        }

        private void OnDrawGizmos() {
            gizmos.DrawGizmos(this);
        }

        private void OnApplicationQuit() {
#if !UNITY_2018_1_OR_NEWER || !UNITY_EDITOR
            PluginCore.Reset();
#endif
        }
        #endregion

        internal void InitSections() {
            cameraData.Init(this);
            gizmos.Init(this);
            events.Init(this);
            optimization.Init(this);
            debugging.Init(this);
        }

        private void OnQuiltSettingsChanged() {
            gameViewResolutionType = (QuiltPreset == Quilt.Preset.Custom) ? HoloplayGameViewResolutionType.MatchQuiltSettings : HoloplayGameViewResolutionType.MatchCalibration;
            onQuiltSettingsChanged?.Invoke(this);
        }

        //DEPRECATED: v1.5.0
        [Obsolete("Use Holoplay.QuiltPreset instead.")]
        public Quilt.Preset GetQuiltPreset() => QuiltPreset;

        //DEPRECATED: v1.5.0
        [Obsolete("Use Holoplay.QuiltPreset instead.")]
        public void SetQuiltPreset(Quilt.Preset preset) => QuiltPreset = preset;

        private void CreateLightfieldMaterial() {
            lightfieldMaterial = new Material(HoloplayUtil.FindShader("Holoplay/Lightfield"));
        }

        private void UpdateFinalCameraDepth() {
            if (finalScreenCamera != null)
                finalScreenCamera.depth = cameraData.Depth;
        }

        private void SetupAllCameras() {

#if UNITY_POST_PROCESSING_STACK_V2
            HoloplayPostProcessSetupData data;
            if (postProcessSetup != null && (data = postProcessSetup(this)).singleViewCamera != null) {
                postProcessCamera = data.postProcessCamera;
                singleViewCamera = data.singleViewCamera;
            } else
#endif
            {
                singleViewCamera = GetComponent<Camera>();
            }
            //NOTE: Only the finalScreenCamera is set with enabled = true, because it's the only camera here meant to write to the screen.
            //Thus, its targetTexture is null, and it's enabled to call OnRenderImage(...) and write each frame to the screen.
            //These other cameras are just for rendering intermediate results.
            singleViewCamera.enabled = false;
            if (postProcessCamera != null)
                postProcessCamera.enabled = false;

            finalScreenCamera = new GameObject(FinalScreenBlitterCameraName).AddComponent<Camera>();
            finalScreenCamera.transform.SetParent(transform);

#if UNITY_2017_3_OR_NEWER
            finalScreenCamera.allowDynamicResolution = false;
#endif
            finalScreenCamera.allowHDR = false;
            finalScreenCamera.allowMSAA = false;
            finalScreenCamera.cullingMask = 0;
            finalScreenCamera.clearFlags = CameraClearFlags.Nothing;
            finalScreenCamera.targetDisplay = (int) m_TargetDisplay;

            screenBlitter = finalScreenCamera.gameObject.AddComponent<HoloplayScreenBlitter>();
            screenBlitter.holoplay = this;

            ResetCamera();

            //NOTE: This is needed for better XR support:
            singleViewCamera.stereoTargetEye = StereoTargetEyeMask.None;
            finalScreenCamera.stereoTargetEye = StereoTargetEyeMask.None;
            if (postProcessCamera != null)
                postProcessCamera.stereoTargetEye = StereoTargetEyeMask.None;
        }

        private void SetAllObjectHideFlags() {
            SetHideFlagsOnObject(singleViewCamera);
            SetHideFlagsOnObject(postProcessCamera);
            SetHideFlagsOnObject(finalScreenCamera);
        }

        /// <summary>
        /// <para>Sets the hide flags on a temporary object used by this <see cref="Holoplay"/> script.</para>
        /// <para>If the <paramref name="tempComponent"/> is on the same <see cref="GameObject"/> as this script, it sets the component's hide flags.<br />
        /// When <paramref name="tempComponent"/> on a different game object from this <see cref="Holoplay"/> script, <paramref name="tempComponent"/>'s game object's hide flags are set instead.</para>
        /// </summary>
        private HideFlags SetHideFlagsOnObject(Component tempComponent) {
            HideFlags hideFlags = HideFlags.None;
            if (tempComponent == null)
                return hideFlags;

            bool isOnCurrentGameObject = tempComponent.gameObject == gameObject;
            bool hide = !debugging.ShowAllObjects;

            if (!isOnCurrentGameObject) {
                hideFlags |= HideFlags.DontSave;
                if (hide)
                    hideFlags |= HideFlags.HideInHierarchy;
            }
            if (hide)
                hideFlags |= HideFlags.HideInInspector;

            if (isOnCurrentGameObject)
                tempComponent.hideFlags = hideFlags;
            else
                tempComponent.gameObject.hideFlags = hideFlags;

            return hideFlags;
        }

        public void ResetCamera() {
            UpdateFinalCameraDepth();
            cameraDistance = GetCameraDistance();
            cameraData.SetCamera(singleViewCamera, transform, Calibration.GetAspect(), cameraDistance);
            
            switch (cameraData.ClearFlags) {
                case CameraClearFlags.Depth:
                case CameraClearFlags.Nothing:
                    //IMPORTANT: The single-view camera MUST clear after each render, or else there will be
                    //ghosting of previous single-view renders left in the next quilt views to render (getting more and more worse as more views are rendered)
                    singleViewCamera.clearFlags = CameraClearFlags.SolidColor;
                    singleViewCamera.backgroundColor = Color.clear;
                    break;
            }
        }

        public void UpdateLightfieldMaterial() => HoloplayRendering.SetLightfieldMaterialSettings(this, LightfieldMaterial);

        //NOTE: Custom rendering resolution is only needed for the editor preview window for now, NOT in builds.
#if UNITY_EDITOR
        internal void UseCustomRenderingResolution(int xpos, int ypos, int width, int height) {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be greater than zero!");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, nameof(height) + " must be greater than zero!");

            isUsingCustomRenderResolution = true;
            customXPos = xpos;
            customYPos = ypos;
            customRenderWidth = width;
            customRenderHeight = height;
            ReloadCalibrationByName();
        }

        internal void ClearCustomRenderingResolution() {
            isUsingCustomRenderResolution = false;
            customXPos = 0;
            customYPos = 0;
            customRenderWidth = 0;
            customRenderHeight = 0;
            ReloadCalibrationByName();
        }
#endif

        [Obsolete("Please use " + nameof(ReloadCalibrationByName) + " or " + nameof(ReloadCalibrationByIndex) + " instead.")]
        public LoadResults ReloadCalibration() => ReloadCalibrationByName();

        public LoadResults ReloadCalibrationByName() => ReloadCalibration(() => {
            CalibrationManager.TryFindCalibration(TargetLKGName, out Calibration found);
            return found;
        });

        public LoadResults ReloadCalibrationByIndex() => ReloadCalibration(() => {
            CalibrationManager.TryGetCalibration(TargetLKGIndex, out Calibration found);
            return found;
        });

        private LoadResults ReloadCalibration(Func<Calibration> calibrationSource) {
            Assert.IsNotNull(calibrationSource);
            Quilt.Settings previousQuiltSettings = QuiltSettings;

            loadResults = PluginCore.GetLoadResults();
            Calibration cal = new Calibration(0, ScreenWidth, ScreenHeight);
            if (loadResults.calibrationFound) {
                cal = calibrationSource();
            } else {
                cal.serial = HoloplayDevice.GetSettings(emulatedDevice).name;
                singleViewCamera.aspect = cal.aspect = Aspect;
            }

            Assert.IsTrue(cal.GetType().IsValueType, "The copy below assumes that "
                + nameof(Calibration) + " is a value type (struct), so the single equals operator creates a deep copy!");

            unmodifiedCalibration = cal;
            if (isUsingCustomRenderResolution)
                this.cal = cal = unmodifiedCalibration.CopyWithCustomResolution(customXPos, customYPos, customRenderWidth, customRenderHeight);
            else
                this.cal = cal;

            targetLKGName = this.cal.LKGname;
            targetLKGIndex = this.cal.index;

            //We need to set up the quilt again because quilt settings may be changed.
            if (!previousQuiltSettings.Equals(QuiltSettings))
                SetupQuilt();

            UpdateLightfieldMaterial();
            onCalibrationReloaded?.Invoke();
            return loadResults;
        }

        //DEPRECATED: v1.5.0
        [Obsolete("Use Holoplay.GetCameraDistance() instead.")]
        public float GetCamDistance() => GetCameraDistance();

        /// <summary>
        /// Returns the camera's distance from the center.
        /// Will be a positive number.
        /// </summary>
        public float GetCameraDistance() {
            if (cameraData.UseFrustumTarget)
                return Mathf.Abs(cameraData.FrustumTarget.localPosition.z);
            return cameraData.Size / Mathf.Tan(cameraData.FieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        public void SetQuiltPresetAndSettings(Quilt.Preset quiltPreset, Quilt.Settings customQuiltSettings) {
            this.quiltPreset = quiltPreset;
            this.customQuiltSettings = customQuiltSettings;
            SetupQuilt();
            OnQuiltSettingsChanged();
        }

        private bool NeedsQuiltResetup() => NeedsQuiltResetup(QuiltSettings);
        private bool NeedsQuiltResetup(Quilt.Settings quiltSettings) {
            if (!isActiveAndEnabled)
                return false;

            RenderTexture quilt = quiltTexture;
            if (quilt == null)
                return true;

            if (quilt.width != quiltSettings.quiltWidth || quilt.height != quiltSettings.quiltHeight)
                return true;
            return false;
        }

        public RenderTexture SetupQuilt() => SetupQuilt(QuiltSettings);

        /// <summary>
        /// <para>Sets up the quilt and the quilt <see cref="RenderTexture"/>.</para>
        /// <para>This should be called after modifying custom quilt settings.</para>
        /// </summary>
        public RenderTexture SetupQuilt(Quilt.Settings quiltSettings) {
            customQuiltSettings.Setup(); // even if not custom quilt, just set this up anyway
            RenderTexture quilt = quiltTexture;
            if (quilt != null)
                quilt.Release();

            quilt = new RenderTexture(quiltSettings.quiltWidth, quiltSettings.quiltHeight, 0, RenderTextureFormat.Default) {
                filterMode = FilterMode.Point,
                hideFlags = (saveQuilt) ? HideFlags.None : HideFlags.DontSave
            };

            quilt.name = "HoloPlay Quilt";
            quilt.enableRandomWrite = true;
            quilt.Create();
            quiltTexture = quilt;
#if UNITY_EDITOR
            if (saveQuilt)
                SaveQuiltAsset();
#endif

            UpdateLightfieldMaterial();

            //Pass some stuff globally for post-processing
            Shader.SetGlobalVector("hp_quiltViewSize", new Vector4(
                (float) quiltSettings.ViewWidth / quiltSettings.quiltWidth,
                (float) quiltSettings.ViewHeight / quiltSettings.quiltHeight,
                quiltSettings.ViewWidth,
                quiltSettings.ViewHeight
            ));
            return quilt;
        }

#if UNITY_EDITOR
        private string SaveQuiltAsset() {
            try {
                RenderTexture quilt = QuiltTexture;
                quilt.hideFlags &= ~HideFlags.DontSave;
                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                string holoplayFolderName = "Holoplay";
                int index = prefabPath.IndexOf(holoplayFolderName);

                Assert.IsTrue(index >= 0);
                prefabPath = prefabPath.Substring(0, index + holoplayFolderName.Length);
                string quiltPath = prefabPath + "/Textures/" + quilt.name + ".renderTexture";
                AssetUtil.EditorReplaceAsset(quiltPath, quilt);

                return quiltPath;
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError("Failed to save the quilt texture!");
                return null;
            }
        }
#endif

        public void RenderQuilt(bool forceRender = false) {
            if (!forceRender && frameRendered)
                return;
            frameRendered = true;

            HoloplayRendering.ClearBeforeRendering(this);

            if (renderBlack) {
                Graphics.Blit(Texture2D.blackTexture, quiltTexture);
                return;
            }

            if (HasOverrideQuilt) {
                Graphics.Blit(m_OverrideQuilt, quiltTexture);
                // if only rendering override, exit here
                if (!renderOverrideBehind)
                    return;
            }

            UpdateFinalCameraDepth();
            HoloplayRendering.RenderQuilt(this, (int viewIndex) => {
                events.OnViewRender?.Invoke(this, viewIndex);
            });
        }

        public RenderTexture RenderPreview2D(bool forceRender = false) {
            if (!forceRender && frameRendered2DPreview)
                return preview2DRT;
            frameRendered2DPreview = true;

            if (renderBlack) {
                //TODO: Create a method similar to SetupQuilt(...) but for the Preview2D texture..
                RenderTexture t = Preview2DRT;
                if (t != null) {
                    Graphics.Blit(Texture2D.blackTexture, t);
                    return t;
                }
            }

            RenderTexture next = HoloplayRendering.RenderPreview2D(this);
            if (next != preview2DRT) {
                if (preview2DRT != null) {
                    if (Application.IsPlaying(gameObject))
                        Destroy(preview2DRT);
                    else
                        DestroyImmediate(preview2DRT);
                }
                preview2DRT = next;
            }
            return preview2DRT;
        }

        internal Texture2D ReadFrom(RenderTexture source) {
            Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);

            RenderTexture.active = source;
            result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            result.Apply();
            RenderTexture.active = source;

            return result;
        }

        internal string GetDefaultFilePathToSave(RenderTexture screenshotSource) {
            string quiltInfo = "qs" + QuiltSettings.viewColumns + "x" + QuiltSettings.viewRows + "a" + cal.aspect;
            string filePath = string.Format("{0}/screen_{1}x{2}_{3}_{4}.png",
                Path.GetFullPath("."), screenshotSource.width, screenshotSource.height,
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), quiltInfo);
            return filePath;
        }

        internal void SaveAsPNGScreenshot(RenderTexture source) => SaveAsPNGScreenshot(source, out Texture2D discardTex, out string discardPath);
        internal void SaveAsPNGScreenshot(RenderTexture source, out Texture2D cpuTexture) => SaveAsPNGScreenshot(source, out cpuTexture, out string discardPath);
        internal void SaveAsPNGScreenshot(RenderTexture source, out Texture2D cpuTexture, out string filePath) {
            filePath = GetDefaultFilePathToSave(source);
            SaveAsPNGScreenshotAt(source, filePath, out cpuTexture);
        }

        internal void SaveAsPNGScreenshotAt(RenderTexture source, string filePath) => SaveAsPNGScreenshotAt(source, filePath, out Texture2D discardTex);
        internal void SaveAsPNGScreenshotAt(RenderTexture source, string filePath, out Texture2D cpuTexture) {
            cpuTexture = ReadFrom(source);

            byte[] bytes = cpuTexture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            
            Debug.Log(string.Format("Took screenshot to: {0}", filePath));
        }

        //DEPRECATED: v1.5.0
        [Obsolete("Use HoloplayRendering.CopyViewToQuilt(Quilt.Settings, int, RenderTexture, RenderTexture[, bool]) instead.")]
        public void CopyViewToQuilt(int view, RenderTexture viewRT, RenderTexture quiltRT, bool forceDrawTex = false) =>
            HoloplayRendering.CopyViewToQuilt(QuiltSettings, view, viewRT, quiltRT, forceDrawTex);

        //DEPRECATED: v1.5.0
        [Obsolete("Use HoloplayRendering.FlipRenderTexture(RenderTexture) instead.")]
        public void FlipRenderTexture(RenderTexture rt) =>
            HoloplayRendering.FlipRenderTexture(rt);

        //DEPRECATED: v1.5.0
        [Obsolete("Use HoloplayRendering.InterpolateViewsOnQuilt(Holoplay, RenderTexture, RenderTexture) instead.")]
        public void InterpolateViewsOnQuilt(RenderTexture quiltRTDepth) =>
            HoloplayRendering.InterpolateViewsOnQuilt(this, QuiltTexture, quiltRTDepth);

        //DEPRECATED: v1.5.0
        [Obsolete("Use HoloplayRendering.SetLightfieldMaterialSettings(Holoplay, Material) instead.")]
        public void PassSettingsToMaterial(Material lightfieldMat) =>
            HoloplayRendering.SetLightfieldMaterialSettings(this, lightfieldMat);
    }
}
