using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace LookingGlass.Editor {
    /// <summary>
    /// A class that ensures our Preview game views close before assembly reloads, and re-open (if needed) after assembly reloads.
    /// </summary>
    [InitializeOnLoad]
    public static class PreviewPersistence {
        /// <summary>
        /// A data class that's serialized to a JSON file to remember whether or not the <see cref="Preview"/> was <see cref="Preview.IsActive">active</see>or not.
        /// </summary>
        [Serializable]
        private class PersistentPreviewData {
            public bool wasPreviewActive;
        }

        /// <summary>
        /// The JSON file that saves data between
        /// assembly reloads and playmode state changes.
        /// </summary>
        private static string PreviousStateFilePath => Path.Combine(Application.temporaryCachePath, "Preview Data.json");

        static PreviewPersistence() {
            Holoplay.onListChanged += OnHoloplayListChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            EditorApplication.quitting += DeleteFile;
            EditorSceneManager.sceneOpened += RecheckDisplayTargetOnSceneOpened;
        }

        private static void OnHoloplayListChanged() {
            Preview.UpdatePreview();
        }

        private static void OnBeforeAssemblyReload() {
            SaveAndClosePreview(PreviousStateFilePath);
        }

        private static void OnAfterAssemblyReload() {
            EditorUpdates.Delay(1, () => {
                ConsumeLoadPreview(PreviousStateFilePath);
            });
        }

        private static void PlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.ExitingPlayMode:
                    SaveAndClosePreview(PreviousStateFilePath);
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    EditorUpdates.Delay(1, () => {
                        ConsumeLoadPreview(PreviousStateFilePath);
                    });
                    break;
            }
        }

        private static void SaveAndClosePreview(string filePath) {
            //If we're already waiting to consume the previous data, DO NOT overwrite it (upon 2nd attempt to save our state) because we close the preview --
            //It'd ALWAYS write false on every subsequent call to this method!
            if (File.Exists(filePath))
                return;

            string json = JsonUtility.ToJson(new PersistentPreviewData() {
                wasPreviewActive = Preview.IsActive
            }, true);

            if (Preview.IsActive)
                Preview.TogglePreview();

            File.WriteAllText(filePath, json);
        }

        private static void ConsumeLoadPreview(string filePath) {
            string json = !File.Exists(filePath) ? "{ }" : File.ReadAllText(filePath);
            PersistentPreviewData data = JsonUtility.FromJson<PersistentPreviewData>(json);
            File.Delete(filePath);

            if (data.wasPreviewActive != Preview.IsActive)
                Preview.TogglePreview();
        }

        private static void DeleteFile() {
            File.Delete(PreviousStateFilePath);
        }

        private static void RecheckDisplayTargetOnSceneOpened(Scene openScene, OpenSceneMode openSceneMode) {
            //NOTE: If we don't wait 1 frame, auto-clicking on the maximize button doesn't seem to work..
            //So let's just wait a frame then! ;)
            EditorUpdates.Delay(1, () => {
                if (!Preview.IsActive)
                    Preview.TogglePreview();
            });
        }
    }
}