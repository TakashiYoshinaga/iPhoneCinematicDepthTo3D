using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace LookingGlass.Editor {
    public static class MenuItemUtil {
        [MenuItem("Assets/Force Reserialize", priority = 41, validate = true)]
        private static bool ValidateReserializeSelectedAssets() {
            return Selection.assetGUIDs.Length > 0;
        }

        [MenuItem("Assets/Force Reserialize", priority = 41, validate = false)]
        private static void ForceReserializeSelectedAssets() {
            string[] assetGuids = Selection.assetGUIDs;

            HashSet<string> assetPaths = new HashSet<string>(assetGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)));
            HashSet<string> allPaths = new HashSet<string>(assetPaths);

            void RecordAssetsUnderFolder(string folderPath) {
                string[] subfolderPaths = Directory.GetDirectories(folderPath);
                allPaths.Add(folderPath);

                foreach (string path in subfolderPaths)
                    RecordAssetsUnderFolder(path);

                string[] files = Directory.GetFiles(folderPath);
                foreach (string filePath in files)
                    if (!filePath.EndsWith(".meta"))
                        allPaths.Add(filePath);
            }

            foreach (string originalAssetPath in assetPaths) {
                if (Directory.Exists(originalAssetPath))
                    RecordAssetsUnderFolder(originalAssetPath);
            }

            AssetDatabase.ForceReserializeAssets(allPaths);
        }
    }
}
