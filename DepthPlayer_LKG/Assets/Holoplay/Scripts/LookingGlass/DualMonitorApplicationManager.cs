//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Diagnostics;

namespace LookingGlass.DualMonitorApplication {

    // enum for pro workstation stuff
    public enum DualMonitorApplicationDisplay {
        LookingGlass,
        Window2D
    }
    [HelpURL("https://look.glass/unitydocs")]
    public class DualMonitorApplicationManager : MonoBehaviour {

        // cause path.combine seems to be glitchy?
        public const string separator = 
#if UNITY_EDITOR_WIN
            "\\";
#else
            "/";
#endif
        public DualMonitorApplicationDisplay display;
        public Process process;
        public static DualMonitorApplicationManager instance;
        public const string extendedUIString = "_extendedUI";
        public const string lkgDisplayString = "LKGDisplay";
        public const int sidePanelResolutionX = 600;
        public const int sidePanelResolutionY = 800;

        // must specify display before creation
        public DualMonitorApplicationManager(DualMonitorApplicationDisplay display) {
            this.display = display;
        }

        void Awake() {
            // only one should exist at a time, check for existing instances on awake
            var existingManagers = FindObjectsOfType<DualMonitorApplicationManager>();
            if (existingManagers.Length > 1) {
                // delete self if found
                DestroyImmediate(this.gameObject);
                return;
            }

            // otherwise this should be the only manager, make it an instance and keep it from being destroyed on scene change
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            // if this is the side panel scene
            if (!Application.isEditor && display == DualMonitorApplicationDisplay.Window2D) {

                // first adjust position
                //UnityEngine.Display.displays[0].Activate(0,0,0);
                UnityEngine.Display.displays[0].SetParams(sidePanelResolutionX, sidePanelResolutionY, 0, 0);
                //Screen.SetResolution(sidePanelResolutionX, sidePanelResolutionY, false);
                
                // launch the lkg version of the application
                if (process == null) {
                    var processPath = Application.streamingAssetsPath + separator + lkgDisplayString + ".exe";
                    ProcessStartInfo processStartInfo = new ProcessStartInfo( processPath );
                    //? not needed
                    //? processStartInfo.Arguments = "--args ";
                    //? processStartInfo.Arguments += "-screen-fullscreen 0 ";
                    process = Process.Start(processStartInfo);
                }
            }
            
            // if it's a looking glass
            if (display == DualMonitorApplicationDisplay.LookingGlass) {

                //? not necessary, this happens automatically in the holoplay capture now
                //// just set the window position
                PluginCore.GetLoadResults();
                UnityEngine.Display.displays[0].Activate(0,0,0);
                UnityEngine.Display.displays[0].SetParams(
                    CalibrationManager.GetCalibration(0).screenWidth, 
                    CalibrationManager.GetCalibration(0).screenHeight, 
                    CalibrationManager.GetCalibration(0).xpos, 
                    CalibrationManager.GetCalibration(0).ypos
                );
                
            }
        }
    }
}
