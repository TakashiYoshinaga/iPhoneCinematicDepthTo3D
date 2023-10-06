// Dual Monitor Application is only supported on 2018 or later
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER

// imported packages below
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using LookingGlass.DualMonitorApplication;

using Debug = UnityEngine.Debug;

namespace LookingGlass.Editor.DualMonitorApplication {
    [HelpURL("https://look.glass/unitydocs")]
    [CustomEditor(typeof(DualMonitorApplicationManager))]
    public class DualMonitorApplicationManagerEditor : UnityEditor.Editor {

        const string proBuildDirPrefsName = "_PROBUILDDIR";
        string separator = DualMonitorApplicationManager.separator;
        string prefsBuildKey {
            get { return Application.productName + proBuildDirPrefsName; }
        }
        string GetPrefsBuildDir() {
            return EditorPrefs.GetString(prefsBuildKey, Path.GetFullPath("."));
        }
        void SaveBuildDirToPrefs(string dir) {
            EditorPrefs.SetString(prefsBuildKey, dir);
        }

        public override void OnInspectorGUI() {
            if (Application.platform == RuntimePlatform.OSXEditor) {
                EditorGUILayout.HelpBox(
                    "Dual Monitor Application Build is Windows Only. Please only build for windows",
                    MessageType.Error
                );
            }

            // setup
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Open or create the windowed UI scene",
                EditorStyles.wordWrappedLabel
            );
            if (GUILayout.Button("Setup/Open Scenes")) {
                SetupScenes();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Build for the Dual Monitor Application. " +  
                "This is a special build command that creates two executables that automatically work for the monitor and the Looking Glass. " +
                "(normal builds won't work without extensive manual configuration, attempting this is not recommended)",
                EditorStyles.wordWrappedLabel
            );
            GUI.color = Color.Lerp(Color.white, Color.green, 0.2f);
            if (GUILayout.Button("Build (Dual Monitor Application)")) {
                BuildProScenes(false);
            }
            GUI.color = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Same as above, but will build and run",
                EditorStyles.wordWrappedLabel
            );
            if (GUILayout.Button("Build and Run (Dual Monitor Application)")) {
                BuildProScenes(true);
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Open the folder containing the Dual Monitor Application build",
                EditorStyles.wordWrappedLabel
            );
            if (GUILayout.Button("Open Builds Folder")) {
                OpenBuildsFolder();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }

        public void SetupScenes() {
            // first check if the scene is already open
            if (EditorSceneManager.sceneCount > 1) {
                // multiple scenes are open, check if the second one is an extendedUI scene
                if (EditorSceneManager.GetSceneAt(1).name.Contains(DualMonitorApplicationManager.extendedUIString)) {
                    Debug.Log("extendedUI scene open already");
                    return;
                }
                // multiple scenes are open, but not extended UI scene
                Debug.Log("secondary scene is open, but is not an extendedUI scene. please close extra scenes and try again.");
                return;
            } else {
                // if only one scene is open
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.SaveOpenScenes();
                Debug.Log("active scene saved!");

                // first see if this scene is already the extended UI scene
                string secondSceneName = "";
                string newPath = "";
                if (activeScene.name.Contains(DualMonitorApplicationManager.extendedUIString)) {
                    // if the active scene is already open, try to load the regular scene
                    secondSceneName = activeScene.name.Substring(0, activeScene.name.IndexOf(DualMonitorApplicationManager.extendedUIString));
                    newPath = activeScene.path;
                    newPath = newPath.Substring(0, newPath.LastIndexOf(DualMonitorApplicationManager.extendedUIString)) + ".unity";
                    // Debug.Log(secondSceneName);
                    // Debug.Log(newPath);
                } else {
                    // try to load _extendedUI version of that scene instead
                    secondSceneName = activeScene.name + DualMonitorApplicationManager.extendedUIString;
                    newPath = activeScene.path;
                    newPath = newPath.Insert( newPath.LastIndexOf(".unity"), DualMonitorApplicationManager.extendedUIString );
                }

                bool loadedSceneSuccessfully = TryLoadingScene( newPath );
                if (loadedSceneSuccessfully) {
                    Debug.Log("found existing complementary scene, loading");
                } else {
                    // if it doesn't exist, create and save it
                    Scene extendedScene = EditorSceneManager.NewScene( NewSceneSetup.EmptyScene, NewSceneMode.Additive );
                    // extendedScene.name = secondSceneName;
                    EditorSceneManager.SaveScene( extendedScene, newPath );
                    Debug.Log( "didn't find existing complementary scene, creating it" );
                }

                // set active scene to lkg one, just to fix lighting
                Scene lkgScene = EditorSceneManager.GetActiveScene();
                for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
                    if (!EditorSceneManager.GetSceneAt(i).name.Contains(DualMonitorApplicationManager.extendedUIString)) {
                        lkgScene = EditorSceneManager.GetSceneAt(i);
                    }
                }
                EditorSceneManager.SetActiveScene(lkgScene);
            }
        }

        public bool TryLoadingScene(string path, bool additive = true) {
            try {
                EditorSceneManager.OpenScene( path, additive ? OpenSceneMode.Additive : OpenSceneMode.Single );
            } catch (Exception e) {
                Debug.Log(e.Message);
                return false;
            }
            return true;
        }

        public void BuildProScenes(bool buildAndRun) {

            // first let user select directory to save to
            string buildPath = GetPrefsBuildDir();
            // if it's not build and run, or if the path is still the default, open the dialog
            // or even do so if it is build and run but there isn't an existing build path
            if (!buildAndRun || 
                buildPath == Path.GetFullPath(".") || 
                (buildAndRun && !Directory.Exists(buildPath))) {
                buildPath = EditorUtility.SaveFolderPanel("Choose Build Directory", GetPrefsBuildDir(), "");
                if (buildPath == "") {
                    return;
                }
            }
            int lastSeparatorIndex = Mathf.Max(buildPath.LastIndexOf('\\'), buildPath.LastIndexOf('/'));
            string filename = buildPath.Substring(lastSeparatorIndex + 1);
            // Debug.Log(filename);

            // check if anything is in there before proceeding
            if (Directory.GetDirectories(buildPath).Length > 0 ||
                Directory.GetFiles(buildPath).Length > 0
            ) {
                // if only just building, give people a chance to cancel overwrite
                if (!buildAndRun) {
                    if (!EditorUtility.DisplayDialog(
                            "Dual Monitor Application Build",
                            "Warning: Build directory is not empty. Overwrite?",
                            "Overwrite",
                            "Cancel")
                    ) {
                        return;
                    }
                }

                // if they confirmed, or if it's build and run, just delete and go ahead
                try {
                    Directory.Delete(buildPath, true);
                } catch (Exception e) {
                    Debug.Log(e.Message);
                    Debug.LogWarning("[Holoplay] Couldn't overwrite existing build. Make sure build is not running and try again");
                }
            }

            // first make sure run in background is enabled
            if (PlayerSettings.runInBackground == false) {
                PlayerSettings.runInBackground = true;
                Debug.Log("note: run in background set to true in playersettings");
            }
            if (PlayerSettings.displayResolutionDialog == ResolutionDialogSetting.Enabled) {
                PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.HiddenByDefault;
                Debug.Log("note: resolution dialog switched to hidden by default");
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            List<String> sceneNamesLKG = new List<string>(); 
            List<String> sceneNamesExtendedUI = new List<string>(); 
            foreach (var s in EditorBuildSettings.scenes) {
                if (s.enabled) {
                    if (s.path.Contains(DualMonitorApplicationManager.extendedUIString)) {
                        sceneNamesExtendedUI.Add(s.path);
                    } else {
                        sceneNamesLKG.Add(s.path);
                    }
                }
            }

            if (sceneNamesExtendedUI.Count == 0) {
                Debug.LogError("no extendedUI scenes found! please add the extendedUI scenes to build settings as well (File -> Build Settings)");
                return;
            } else {
                // first build the extendedUI scenes and make that an exe
                buildPlayerOptions.scenes = sceneNamesExtendedUI.ToArray();
                buildPlayerOptions.locationPathName = 
                    buildPath + separator +
                    filename + ".exe";
                buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                buildPlayerOptions.options = BuildOptions.None;
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;
                if (summary.result == BuildResult.Succeeded) {
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                }
                if (summary.result == BuildResult.Failed) {
                    Debug.Log("Build failed");
                    // exit if the build fails
                    return;
                }
            }
            if (sceneNamesLKG.Count == 0) {
                Debug.Log("no LKG scenes found! please add the LKG scenes to build settings as well");
            } else {
                // first build the extendedUI scenes and make that an exe
                buildPlayerOptions.scenes = sceneNamesLKG.ToArray();
                buildPlayerOptions.locationPathName = 
                    buildPath + separator +
                    filename + "_Data" + separator +
                    "StreamingAssets" + separator +
                    DualMonitorApplicationManager.lkgDisplayString + ".exe";
                buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                buildPlayerOptions.options = BuildOptions.None;
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;
                if (summary.result == BuildResult.Succeeded) {
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                }
                if (summary.result == BuildResult.Failed) {
                    Debug.Log("Build failed");
                    // exit if the build fails
                    return;
                }
            }
            // if the builds succeeded, record the directory to prefs
            SaveBuildDirToPrefs(buildPath);
            if (!buildAndRun) {
                OpenBuildsFolder();
            } else {
                var processPath = buildPath + separator + filename + ".exe";
                ProcessStartInfo processStartInfo = new ProcessStartInfo( processPath );
                Process.Start(processStartInfo);
            }
        }

        public void OpenBuildsFolder() {
            EditorUtility.RevealInFinder(GetPrefsBuildDir());
        }
    }
}

#endif