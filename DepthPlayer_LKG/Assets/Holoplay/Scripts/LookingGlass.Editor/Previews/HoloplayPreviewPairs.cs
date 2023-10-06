using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace LookingGlass.Editor {
    public static class HoloplayPreviewPairs {
        private static Dictionary<Holoplay, HoloplayPreviewWindow> all = new Dictionary<Holoplay, HoloplayPreviewWindow>();

        private static bool isEnumerating = false;
        private static List<Holoplay> toRemove = new List<Holoplay>();

        public static int Count => all.Count;
        public static IEnumerable<KeyValuePair<Holoplay, HoloplayPreviewWindow>> All {
            get {
                Clean();
                isEnumerating = true;
                try {
                    foreach (Holoplay h in all.Keys) {
                        HoloplayPreviewWindow preview = all[h];
                        Assert.IsNotNull(preview);
                        yield return new KeyValuePair<Holoplay, HoloplayPreviewWindow>(h, preview);
                    }
                } finally {
                    isEnumerating = false;
                }

                foreach (Holoplay h in toRemove)
                    all.Remove(h);
                toRemove.Clear();
            }
        }

        public static bool IsPaired(EditorWindow gameView) {
            if (gameView == null)
                throw new ArgumentNullException(nameof(gameView));

            foreach (KeyValuePair<Holoplay, HoloplayPreviewWindow> pair in All) {
                EditorWindow other = pair.Value.GameView;
                if (other != null && other == gameView)
                    return true;
            }
            return false;
        }

        public static HoloplayPreviewWindow GetPreview(Holoplay holoplay) {
            if (all.TryGetValue(holoplay, out HoloplayPreviewWindow preview))
                return preview;
            return null;
        }

        public static HoloplayPreviewWindow Create(Holoplay holoplay) {
            if (all.ContainsKey(holoplay))
                throw new InvalidOperationException(holoplay + " already has a game view created for it!");

            HoloplayPreviewWindow preview = HoloplayPreviewWindow.Create(holoplay);
            if (preview == null)
                return null;

            all.Add(holoplay, preview);
            return preview;
        }

        private static void Clean() {
            isEnumerating = true;
            try {
                foreach (Holoplay h in all.Keys) {
                    HoloplayPreviewWindow preview = all[h];
                    if (preview == null || preview.GameView == null) {
                        toRemove.Add(h);
                    }
                }
            } finally {
                isEnumerating = false;
            }

            foreach (Holoplay h in toRemove) {
                //NOTE: Do NOT destroy the Holoplay component! Destroy the preview window for it.
                HoloplayPreviewWindow preview = all[h];
                if (preview != null)
                    ScriptableObject.DestroyImmediate(preview);
                all.Remove(h);
            }
            toRemove.Clear();
        }

        public static bool IsPreviewOpenForDevice(string lkgName) {
            Clean();
            foreach (Holoplay h in all.Keys)
                if (h.TargetLKGName == lkgName)
                    return true;
            return false;
        }

        public static void Close(Holoplay holoplay) {
            if (!all.TryGetValue(holoplay, out HoloplayPreviewWindow preview)) {
                Debug.LogError("Failed to close " + holoplay + "'s window! It couldn't be found.");
                return;
            }
            preview.GameView.Close();
            ScriptableObject.DestroyImmediate(preview);

            if (isEnumerating) {
                toRemove.Add(holoplay);
            } else {
                all.Remove(holoplay);
            }
        }

        public static void CloseAll() {
            if (isEnumerating) {
                Debug.LogError("Failed to close all Holoplay game views: They're currently being enumerated, so the dictionary collection cannot be modified!");
                return;
            }

            foreach (HoloplayPreviewWindow preview in all.Values) {
                try {
                    preview.GameView.Close();
                    ScriptableObject.DestroyImmediate(preview);
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
            all.Clear();
            toRemove.Clear();
        }
    }
}