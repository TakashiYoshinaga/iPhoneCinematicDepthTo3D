using UnityEngine;
using UnityEditor;

namespace LookingGlass.Editor {
    [InitializeOnLoad]
    public static class AutoScriptExecutionOrderer {
        public const int HoloplayExecutionOrder = -1000;

        static AutoScriptExecutionOrderer() {
            EditorApplication.update += AutoCheckOrder;
        }

        private static void AutoCheckOrder() {
            EditorApplication.update -= AutoCheckOrder;

            string scriptName = nameof(Holoplay) + ".cs";
            string guid = "9de313519d88fe64f97b9e99db9c6c30";
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = null;
            if (string.IsNullOrWhiteSpace(assetPath) || (script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath)) == null) {
                Debug.LogError("Failed to find the " + scriptName + " script by GUID (This is needed to check if its order is set to " + HoloplayExecutionOrder + ")! Did its GUID accidentally change from " + guid + "?");
                return;
            }

            int currentOrder = MonoImporter.GetExecutionOrder(script);

            if (currentOrder != HoloplayExecutionOrder) {
                Debug.Log("Automatically setting the " + scriptName + " script to order " + HoloplayExecutionOrder + " so it is ready before other scripts during Awake, OnEnable, Start, etc.");
                MonoImporter.SetExecutionOrder(script, HoloplayExecutionOrder);
            }
        }
    }
}