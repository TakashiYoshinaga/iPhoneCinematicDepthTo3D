//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LookingGlass.Editor.EditorPropertyGroups;

using Object = UnityEngine.Object;

namespace LookingGlass.Editor {
    [CustomEditor(typeof(Holoplay))]
    [CanEditMultipleObjects]
    public class HoloplayEditor : UnityEditor.Editor {
        #region Static Section
        static HoloplayEditor() {
            Holoplay.onQuiltSettingsChanged += OnQuiltSettingsChanged;
        }

        private static void OnQuiltSettingsChanged(Holoplay holoplay) {
            if (holoplay != Holoplay.Instance)
                return;

            UpdateUserGameViews();
        }

        [MenuItem("GameObject/Holoplay Capture", false, 10)]
        public static void CreateHoloPlay() {
            GameObject asset = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Holoplay/Prefabs/Holoplay Capture.prefab", typeof(GameObject));
            if (asset == null) {
                Debug.LogWarning("[Holoplay] Couldn't find the holoplay capture folder or prefab.");
                return;
            }
            GameObject hp = Instantiate(asset, Vector3.zero, Quaternion.identity);
            hp.name = asset.name;
            Undo.RegisterCreatedObjectUndo(hp, "Create Holoplay Capture");
        }

        [MenuItem("Holoplay/Setup Player Settings", false, 1)]
        public static void OpenPlayerSettingsEditor() {

        }

        public static bool AddErrorCondition(HoloplayHelpMessageCondition condition) {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (errorConditions == null) {
                errorConditions = new List<HoloplayHelpMessageCondition>();
                errorConditions.Add(condition);
                return true;
            }
            if (errorConditions.Contains(condition))
                return false;
            errorConditions.Add(condition);
            return true;
        }

        public static bool RemoveErrorCondition(HoloplayHelpMessageCondition condition) {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            return errorConditions != null && errorConditions.Remove(condition);
        }
        #endregion

        private static GUIContent versionLabel1;
        private static GUIContent versionLabel2;

        private static int editorsOpen = 0;
        private static string[] lkgNames;
        private static string[] lkgIndices;

        private static bool showCalibrationData;
        private static List<HoloplayHelpMessageCondition> errorConditions;

        private Holoplay[] holoplays;

        private bool changedTargetDisplay = false;
        private bool changedLKGName = false;
        private bool changedLKGIndex = false;
        private bool changedEmulatedDevice = false;
        private bool changedQuiltPreset = false;
        private bool requiresPreviewUpdate = false;

        private static readonly EditorPropertyGroup[] Groups = new EditorPropertyGroup[] {
            new EditorPropertyGroup(
                "Camera Data",
                "Contains a set of fields that correspond to fields on a Unity Camera, with some extra holoplay fields.",
                new string[] {
                    nameof(Holoplay.m_Size),
                    nameof(Holoplay.m_NearClipFactor),
                    nameof(Holoplay.m_FarClipFactor),
                    nameof(Holoplay.m_ScaleFollowsSize),
                    nameof(Holoplay.m_ClearFlags),
                    nameof(Holoplay.m_Background),
                    nameof(Holoplay.m_CullingMask),
                    nameof(Holoplay.m_FieldOfView),
                    nameof(Holoplay.m_Depth),
                    nameof(Holoplay.m_RenderingPath),
                    nameof(Holoplay.useOcclusionCulling),
                    nameof(Holoplay.m_AllowHDR),
                    nameof(Holoplay.m_AllowMSAA),
                    nameof(Holoplay.m_AllowDynamicResolution),
                    nameof(Holoplay.m_UseFrustumTarget),
                    nameof(Holoplay.m_FrustumTarget),
                    nameof(Holoplay.m_ViewconeModifier),
                    nameof(Holoplay.m_CenterOffset),
                    nameof(Holoplay.m_HorizontalFrustumOffset),
                    nameof(Holoplay.m_VerticalFrustumOffset)
                }
            ),
            new EditorPropertyGroup(
                "Gizmos",
                "Contains settings for visualizations in the Scene View.",
                new string[] {
                    nameof(Holoplay.m_DrawHandles),
                    nameof(Holoplay.m_FrustumColor),
                    nameof(Holoplay.m_MiddlePlaneColor),
                    nameof(Holoplay.m_HandleColor)
                }
            ),
            new EditorPropertyGroup(
                "Events",
                "Contains Holoplay events, related to initialization and rendering.",
                new string[] {
                    nameof(Holoplay.m_OnHoloplayReady),
                    nameof(Holoplay.m_OnViewRender)
                }
            ),
            new EditorPropertyGroup(
                "Optimization",
                "Contains Holoplay optimization techniques to increase performance.",
                new string[] {
                    nameof(Holoplay.m_ViewInterpolation),
                    nameof(Holoplay.m_ReduceFlicker),
                    nameof(Holoplay.m_FillGaps),
                    nameof(Holoplay.m_BlendViews),
                }
            ),
            new EditorPropertyGroup(
                "Debugging",
                "Contains settings to ease debugging the Holoplay script.",
                new string[] {
                    nameof(Holoplay.m_ShowAllObjects),
                }
            )
        };

        #region Unity Messages
        private void OnEnable() {
            Object[] targets = base.targets;
            holoplays = new Holoplay[targets.Length];
            for (int i = 0; i < holoplays.Length; i++)
                holoplays[i] = (Holoplay) targets[i];

            editorsOpen++;

            if (editorsOpen == 1) {
                CalibrationManager.onRefresh += RefreshLKGNames;
                RefreshLKGNames();
            }

            Groups[0].IsExpanded = true;
            serializedObject.FindProperty("customQuiltSettings").isExpanded = true;
        }

        private void OnDisable() {
            editorsOpen--;
            if (editorsOpen <= 0) {
                CalibrationManager.onRefresh -= RefreshLKGNames;
            }
        }

        protected virtual void OnSceneGUI() {
            Holoplay hp = (Holoplay) target;
            if (!hp.enabled || !hp.Gizmos.DrawHandles)
                return;

            // for some reason, doesn't need the gamma conversion like gizmos do
            Handles.color = hp.Gizmos.HandleColor;

            Matrix4x4 originalMatrix = Handles.matrix;
            Matrix4x4 hpMatrix = Matrix4x4.TRS(
                hp.transform.position,
                hp.transform.rotation,
                new Vector3(hp.SingleViewCamera.aspect, 1f, 1f));
            Handles.matrix = hpMatrix;

            float size = hp.CameraData.Size;
            Vector3[] dirs = new Vector3[] {
                new Vector3(-size, 0f),
                new Vector3( size, 0f),
                new Vector3(0f, -size),
                new Vector3(0f,  size),
            };
            float newSize = size;

            foreach (Vector3 d in dirs) {
                EditorGUI.BeginChangeCheck();
                Vector3 newDir = Handles.Slider(d, d, HandleUtility.GetHandleSize(d) * 0.03f, Handles.DotHandleCap, 0f);
                newSize = Vector3.Dot(newDir, d.normalized);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(hp, "Holoplay Size");
                    hp.CameraData.Size = Mathf.Clamp(newSize, 0.01f, Mathf.Infinity);
                    hp.ResetCamera();
                }
            }

            Handles.matrix = originalMatrix;
        }
        #endregion

        private static void RefreshLKGNames() {
            lkgNames = new string[CalibrationManager.CalibrationCount];
            for (int i = 0; i < lkgNames.Length; i++)
                lkgNames[i] = CalibrationManager.GetCalibration(i).LKGname;

            lkgIndices = new string[lkgNames.Length];
            for (int i = 0; i < lkgIndices.Length; i++) {
                lkgIndices[i] = i.ToString();
                if (i != CalibrationManager.GetCalibration(i).index)
                    Debug.LogError("It is assumed that each calibration's index field matches its actual index in the array!");
            }
        }


        public override void OnInspectorGUI() {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            //WARNING: This script has the [CanEditMultipleObjects] attribute, yet we directly use base.target instead of base.targets!!
            Holoplay hp = (Holoplay) target;
            
            if (errorConditions != null) {
                foreach (Holoplay h in holoplays) {
                    foreach (HoloplayHelpMessageCondition condition in errorConditions) {
                        HoloplayHelpMessage helpMessage = condition(h);
                        if (helpMessage.HasMessage) {
                            EditorGUILayout.HelpBox(helpMessage.message, helpMessage.type);
                            continue;
                        }
                    }
                }
            }

            changedTargetDisplay = false;
            changedLKGName = false;
            changedLKGIndex = false;
            changedEmulatedDevice = false;
            changedQuiltPreset = false;
            requiresPreviewUpdate = false;

            if (versionLabel1 == null) {
                versionLabel1 = new GUIContent("Version");
                versionLabel2 = new GUIContent(Holoplay.Version.ToString() + Holoplay.VersionLabel, "HoloPlay Unity Plugin v" + Holoplay.Version);
            }
            EditorGUILayout.LabelField(versionLabel1, versionLabel2, EditorStyles.miniLabel);

            SectionGUILayout.DrawDefaultInspectorWithSections(serializedObject, Groups, CustomGUI);

            //Let's make sure to save here, in case we used any non-SerializedProperty Editor GUI:
            if (serializedObject.ApplyModifiedProperties())
                EditorUtility.SetDirty(hp);

            bool needsToSaveAgain = false;
            if (changedTargetDisplay) {
                needsToSaveAgain = true;
                hp.TargetDisplay = hp.TargetDisplay;
            }
            if (changedLKGName) {
                needsToSaveAgain = true;
                hp.TargetLKGName = hp.TargetLKGName;
            }
            if (changedLKGIndex) {
                needsToSaveAgain = true;
                hp.TargetLKGIndex = hp.TargetLKGIndex;
            }
            if (changedEmulatedDevice) {
                needsToSaveAgain = true;
                hp.EmulatedDevice = hp.EmulatedDevice;

                //NOTE: This assumes the emulatedDevice field only shows in the inspector when 0 LKG devices are recognized.
                //Thus, all game views should be set to the resolution of the Holoplay's emulated device
                foreach (EditorWindow gameView in GameViewExtensions.FindAllGameViews()) {
                    Calibration cal = hp.Calibration;
                    gameView.SetGameViewResolution(cal.screenWidth, cal.screenHeight, hp.DeviceTypeName);
                }
            }
            if (changedQuiltPreset) {
                needsToSaveAgain = true;
                hp.QuiltPreset = hp.QuiltPreset;
            }

            if (needsToSaveAgain)
                EditorUtility.SetDirty(hp);
            if (requiresPreviewUpdate) {
                //TODO: I have no idea why updating the preview below
                //wasn't displaying on the device properly.. but it works if we wait some number of more frames...
                EditorUpdates.Delay(6, () => Preview.UpdatePreview());
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            if (GUILayout.Button(Preview.togglePreviewShortcut))
                Preview.TogglePreview();

            if (GUILayout.Button("Reload Calibration")) {
                hp.ReloadCalibrationByName();
                if (Preview.IsActive)
                    Preview.UpdatePreview();
            }

            using (new EditorGUI.DisabledScope(true)) {
                showCalibrationData = EditorGUILayout.Foldout(showCalibrationData, "Calibration Data");
                if (showCalibrationData) {
                    EditorGUI.indentLevel++;
                    try {
                        //NOTE: When a custom resolution is NOT being used, we still print out a helpbox with 2 lines of no text,
                        //For UX -- to avoid shifting the view of the text area on the user!
                        EditorGUILayout.HelpBox(hp.IsUsingCustomResolution ? "A custom resolution is being used that differs from the LKG device's native resolution.\n" +
                            "Calibration values may be different than the native device's values." : "\n", MessageType.Info);
                        EditorGUILayout.TextArea(JsonUtility.ToJson(hp.cal, true));
                    } finally {
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private bool CustomGUI(SerializedProperty property) {
            Holoplay hp = holoplays[0];
            if (property.name == "customQuiltSettings" && hp.QuiltPreset != Quilt.Preset.Custom) {
                Quilt.Settings quiltSettings = hp.QuiltSettings;

                EditorGUILayout.LabelField("Quilt Size: ", quiltSettings.quiltWidth + " x " + quiltSettings.quiltHeight);
                EditorGUILayout.LabelField("View Size: ", quiltSettings.ViewWidth + " x " + quiltSettings.ViewHeight);
                EditorGUILayout.LabelField("Tiling: ", quiltSettings.viewColumns + " x " + quiltSettings.viewRows);
                EditorGUILayout.LabelField("Views Total: ", quiltSettings.numViews.ToString());

                return true;
            }

            if (property.name == "targetDisplay") {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property, true);

                if (EditorGUI.EndChangeCheck()) {
                    changedTargetDisplay = true;
                    requiresPreviewUpdate = true;
                }
                return true;
            }

            if (property.name == "targetLKGName") {
                if (lkgNames.Length > 0) {
                    bool changed = false;
                    int index = Array.IndexOf(lkgNames, hp.TargetLKGName);

                    EditorGUI.BeginChangeCheck();
                    Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));

                    EditorGUI.BeginProperty(rect, new GUIContent("Target LKG Name"), property);
                    index = EditorGUI.Popup(rect, "Target LKG Name", index, lkgNames);
                    EditorGUI.EndProperty();

                    if (index < 0 || index >= lkgNames.Length) {
                        index = 0;
                        changed = true;
                    }
                    changed |= EditorGUI.EndChangeCheck();
                    if (changed) {
                        property.stringValue = lkgNames[index];
                        changedLKGName = true;
                        requiresPreviewUpdate = true;
                    }
                }
                return true;
            }

            if (property.name == "targetLKGIndex") {
                if (lkgIndices.Length > 0) {
                    bool changed = false;
                    int index = Array.IndexOf(lkgIndices, hp.TargetLKGIndex.ToString());

                    EditorGUI.BeginChangeCheck();
                    Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));

                    EditorGUI.BeginProperty(rect, new GUIContent("Target LKG Name"), property);
                    index = EditorGUI.Popup(rect, "Target LKG Index", index, lkgIndices);
                    EditorGUI.EndProperty();

                    GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                    if (index < 0 || index >= lkgIndices.Length) {
                        index = 0;
                        changed = true;
                    }
                    changed |= EditorGUI.EndChangeCheck();
                    if (changed) {
                        property.intValue = int.Parse(lkgIndices[index]);
                        changedLKGIndex = true;
                        requiresPreviewUpdate = true;
                    }
                }
                return true;
            }

            if (property.name == "emulatedDevice") {
                //NOTE: We hide this field when at least 1 LKG device (calibration) is found.
                if (CalibrationManager.HasAnyCalibrations)
                    return true;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property);

                if (EditorGUI.EndChangeCheck()) {
                    changedEmulatedDevice = true;
                    requiresPreviewUpdate = true;
                }
                return true;
            }

            if (property.name == "quiltPreset") {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property);

                if (EditorGUI.EndChangeCheck()) {
                    changedQuiltPreset = true;
                    requiresPreviewUpdate = true;
                }
                return true;
            }
            return false;
        }

        public static bool UpdateUserGameViews() {
            Holoplay main = Holoplay.Instance;
            if (main == null)
                return false;

            HoloplayGameViewResolutionType resolutionType = main.GameViewResolutionType;
            Vector2Int resolution;
            string deviceTypeName;

            switch (resolutionType) {
                case HoloplayGameViewResolutionType.MatchCalibration:
                    Calibration cal = main.UnmodifiedCalibration;
                    resolution = new Vector2Int(cal.screenWidth, cal.screenHeight);
                    deviceTypeName = main.DeviceTypeName;
                    break;
                case HoloplayGameViewResolutionType.MatchQuiltSettings:
                    Quilt.Settings quiltSettings = main.QuiltSettings;
                    resolution = new Vector2Int(quiltSettings.ViewWidth, quiltSettings.ViewHeight);
                    if (main.QuiltPreset == Quilt.Preset.Automatic)
                        deviceTypeName = main.DeviceTypeName;
                    else
                        deviceTypeName = main.QuiltPreset.ToString();
                    break;
                default:
                    throw new NotSupportedException("Unsupported game view resolution type: " + resolutionType);
            }
            foreach (EditorWindow gameView in GameViewExtensions.FindUnpairedGameViews())
                gameView.SetGameViewResolution(resolution.x, resolution.y, deviceTypeName);

            return true;
        }
    }
}
