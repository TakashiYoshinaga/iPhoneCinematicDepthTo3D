using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LookingGlass {
    internal static class HoloplayUtil {
        private static bool alreadyAttemptedReimport = false;

        //NOTE: This is a more robust replacement for calling Shader.Find(string), that detects when Unity accidentally returns null.
        //This may occur when downgrading the Unity project, and perhaps in other unidentified scenarios.
        //Instead of leaving the developer to be scratching their head why the Lightfield shader is null, we just reimport the Resources folder that contains it and automatically re-reference it.
        //This speeds up our workflow!
        public static Shader FindShader(string name) {
            Shader shader = Shader.Find(name);

#if UNITY_EDITOR
            if (shader == null && !alreadyAttemptedReimport) {
                alreadyAttemptedReimport = true;
                try {
                    string resourcesFolderGuid = "4d49f158eb8fe48a89f792f8dd9c09af";
                    Debug.Log("Forcing reimport of the resources folder (GUID = " + resourcesFolderGuid + ") because " + nameof(Shader) + "." + nameof(Shader.Find) + "(string) was returning null.");
                    string resourcesFolderPath = AssetDatabase.GUIDToAssetPath(resourcesFolderGuid);
                    AssetDatabase.ImportAsset(resourcesFolderPath, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);

                    shader = Shader.Find(name);
                } catch (Exception e) {
                    Debug.LogException(e);
                    return null;
                }
            }
#endif

            return shader;
        }
    }
}