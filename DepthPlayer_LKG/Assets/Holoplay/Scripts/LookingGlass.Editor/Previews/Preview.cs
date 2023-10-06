//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using UnityEngine;
using UnityEditor;

namespace LookingGlass.Editor {
    public static class Preview {
        public const string togglePreviewShortcut =
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            "Toggle Preview ⌘E";
#else
            "Toggle Preview Ctrl + E";
#endif

        public const string manualSettingsPath = "Assets/HoloplayPreviewSettings.asset";
        private static ManualPreviewSettings manualPreviewSettings;

        public static bool IsActive {
            get {
                if (!Holoplay.AnyEnabled)
                    return false;

                return HoloplayPreviewWindow.Count > 0;
            }
        }

        public static bool UseManualPreview => manualPreviewSettings != null && manualPreviewSettings.manualPosition;
        public static ManualPreviewSettings ManualPreviewSettings => manualPreviewSettings;

        [InitializeOnLoadMethod]
        private static void InitPreview() {
            RuntimePreviewInternal.Initialize(() => IsActive, () => TogglePreview());
            EditorUpdates.Delay(1, AutoCloseExtraHoloplayWindows);
        }

        [MenuItem("Assets/Create/Holoplay/Manual Preview Settings")]
        private static void CreateManualPreviewAsset() {
            ManualPreviewSettings previewSettings = AssetDatabase.LoadAssetAtPath<ManualPreviewSettings>(manualSettingsPath);
            if (previewSettings == null) {
                previewSettings = ScriptableObject.CreateInstance<ManualPreviewSettings>();
                AssetDatabase.CreateAsset(previewSettings, manualSettingsPath);
                AssetDatabase.SaveAssets();
            }
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = previewSettings;
        }

        [MenuItem("Holoplay/Toggle Preview %e", false, 1)]
        public static bool TogglePreview() {
            if (manualPreviewSettings == null)
                manualPreviewSettings = AssetDatabase.LoadAssetAtPath<ManualPreviewSettings>(manualSettingsPath);
            return TogglePreviewInternal();
        }

        private static void AutoCloseExtraHoloplayWindows() {
            PluginCore.GetLoadResults();

            if (manualPreviewSettings != null && CalibrationManager.CalibrationCount < 1) {
                int count = HoloplayPreviewPairs.Count;
                if (count > 0)
                    Debug.Log("[Holoplay] Closing " + count + " extra Holoplay window(s).");

                HoloplayPreviewPairs.CloseAll();
            }
        }

        private static bool TogglePreviewInternal() {
            bool wasActive = IsActive;

            if (wasActive)
                CloseAllHoloplayWindows();
            else
                OpenAllHoloplayWindows();

            return !wasActive;
        }

        private static void OpenAllHoloplayWindows() {
            if (Holoplay.Count == 0)
                Debug.LogWarning("Unable to create a " + nameof(HoloplayPreviewWindow) + ": there was no " + nameof(Holoplay) + " instance available.");

            //WARNING: Potentially duplicate call to HoloPlay Core?
            LoadResults loadResults = PluginCore.GetLoadResults();
            if (!UseManualPreview && (!loadResults.attempted || !loadResults.lkgDisplayFound || !loadResults.calibrationFound)) {
                Debug.LogWarning("No Looking Glass detected. Please ensure your display is correctly connected and that HoloPlay Service is running.");
                CloseAllHoloplayWindows();
                return;
            }

            foreach (Holoplay holoplay in Holoplay.All) {
                if (holoplay.HasTargetDevice && HoloplayPreviewPairs.IsPreviewOpenForDevice(holoplay.TargetLKGName)) {
                    Debug.LogWarning("Skipping preview for " + holoplay.name + " because its target LKG device already has a preview showing! The game views would overlap.");
                    continue;
                }
                HoloplayPreviewWindow preview = HoloplayPreviewPairs.Create(holoplay);
            }

            HoloplayEditor.UpdateUserGameViews();
        }

        private static void CloseAllHoloplayWindows() {
            HoloplayPreviewPairs.CloseAll();
        }

        public static bool UpdatePreview() {
            if (!IsActive)
                return false;

            CloseAllHoloplayWindows();
            OpenAllHoloplayWindows();
            return true;
        }

        [Obsolete("Please use " + nameof(IsActive) + ", " + nameof(TogglePreview) + ", and/or " + nameof(UpdatePreview) + " instead.")]
        private static void HandlePreview(bool isToggling) {
            if (isToggling)
                TogglePreview();
            else
                UpdatePreview();
        }
    }
}
