//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LookingGlass {
    public struct LoadResults {
        public bool attempted;
        public bool calibrationFound;
        public bool lkgDisplayFound;
        public LoadResults(bool attempted, bool calibrationFound, bool lkgDisplayFound) {
            this.attempted = attempted;
            this.calibrationFound = calibrationFound;
            this.lkgDisplayFound = lkgDisplayFound;
        }
    }

    [System.Serializable]
    public class HoloplayLoadEvent : UnityEvent<LoadResults> { };

    public static partial class PluginCore {
        public enum hpc_client_error {
            hpc_CLIERR_NOERROR,
            hpc_CLIERR_NOSERVICE,
            hpc_CLIERR_VERSIONERR,
            hpc_CLIERR_SERIALIZEERR,
            hpc_CLIERR_DESERIALIZEERR,
            hpc_CLIERR_MSGTOOBIG,
            hpc_CLIERR_SENDTIMEOUT,
            hpc_CLIERR_RECVTIMEOUT,
            hpc_CLIERR_PIPEERROR,
        };

        public enum hpc_license_type {
            hpc_LICENSE_NONCOMMERCIAL,
            hpc_LICENSE_COMMERCIAL
        }

        [DllImport("HoloPlayCore")] private static extern hpc_client_error hpc_InitializeApp(string app_name, hpc_license_type app_type);
        [DllImport("HoloPlayCore")] private static extern int hpc_CloseApp();
        [DllImport("HoloPlayCore")] private static extern hpc_client_error hpc_RefreshState();

        [DllImport("HoloPlayCore")] private static extern int hpc_GetStateAsJSON(StringBuilder out_buf, int out_buf_sz);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetNumDevices();
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyInt(int dev_index, string query_string);
        [DllImport("HoloPlayCore")] internal static extern float hpc_GetDevicePropertyFloat(int dev_index, string query_string);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyString(int dev_index, string query_string, StringBuilder out_buf, int out_buf_sz);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetHoloPlayCoreVersion(StringBuilder out_buf, int out_buf_sz);

        /// <summary>
        /// Gets the current version of HoloPlay Service.
        /// </summary>
        [DllImport("HoloPlayCore")] private static extern int hpc_GetHoloPlayServiceVersion(StringBuilder out_buf, int out_buf_sz);

        //Queries per-device string values.
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDeviceHDMIName(int dev_index, StringBuilder out_buf, int out_buf_sz);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDeviceSerial(int dev_index, StringBuilder out_buf, int out_buf_sz);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDeviceType(int dev_index, StringBuilder out_buf, int out_buf_sz);

        //Queries for per-device calibration and window parameters.
        //These will return 0 if the device or calibration isn't found.
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyScreenW(int dev_index);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyScreenH(int dev_index);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyWinX(int dev_index);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyWinY(int dev_index);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyInvView(int dev_index);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyRi(int dev_index);
        [DllImport("HoloPlayCore")] private static extern int hpc_GetDevicePropertyBi(int dev_index);
        [DllImport("HoloPlayCore")] private static extern float hpc_GetDevicePropertyPitch(int dev_index);
        [DllImport("HoloPlayCore")] private static extern float hpc_GetDevicePropertyCenter(int dev_index);
        [DllImport("HoloPlayCore")] private static extern float hpc_GetDevicePropertyTilt(int dev_index);
        [DllImport("HoloPlayCore")] private static extern float hpc_GetDevicePropertyDisplayAspect(int dev_index);
        [DllImport("HoloPlayCore")] private static extern float hpc_GetDevicePropertyFringe(int dev_index);
        [DllImport("HoloPlayCore")] private static extern float hpc_GetDevicePropertySubp(int dev_index);

        private static int hpc_GetViewCone(int dev_index) { return (int) hpc_GetDevicePropertyFloat(dev_index, "/calibration/viewCone/value"); }

        private static string GetHoloPlayCoreVersion() {
            StringBuilder str = new StringBuilder(100);
            hpc_GetHoloPlayCoreVersion(str, 100);
            return str.ToString();
        }

        private static string GetHoloPlayServiceVersion() {
            StringBuilder str = new StringBuilder(100);
            hpc_GetHoloPlayServiceVersion(str, 100);
            return str.ToString();
        }

        private static string GetDeviceStatus(int dev_index) {
            StringBuilder str = new StringBuilder(100);
            hpc_GetDevicePropertyString(dev_index, "/state", str, 100);
            return str.ToString();
        }

        private static string GetSerial(int dev_index) {
            StringBuilder str = new StringBuilder(100);
            hpc_GetDeviceSerial(dev_index, str, 100);
            return str.ToString();
        }
        private static string GetLKGName(int dev_index) {
            StringBuilder str = new StringBuilder(100);
            hpc_GetDeviceHDMIName(dev_index, str, 100);
            return str.ToString();
        }

        public static int GetLKGunityIndex(int dev_index) {
            return hpc_GetDevicePropertyInt(dev_index, "/unityIndex");
        }

        private const string InitKey = "isHoloPlayCoreInit";
        private const int DEFAULT = -1;

        public static hpc_client_error InitHoloPlayCore() {
            hpc_client_error error;
            string unity_status = Application.isEditor ? "Editor" : "Player";
            error = hpc_InitializeApp("Unity_" + unity_status + "_" + Application.productName, hpc_license_type.hpc_LICENSE_NONCOMMERCIAL);
            if (error == hpc_client_error.hpc_CLIERR_NOERROR) {
                // isInit = true;
                Debug.Log("[HoloPlay] HoloPlay Core Initialization: (HoloPlay Core version: "
                    + GetHoloPlayCoreVersion() + ", HoloPlay Service version: "
                    + GetHoloPlayServiceVersion() + ")");
            }
            return error;
        }

        public static LoadResults GetLoadResults() {
            bool isInit = PlayerPrefs.GetInt(InitKey, DEFAULT) > DEFAULT;
            LoadResults results = new LoadResults(false, false, false);
            hpc_client_error error;

            if (!isInit) {
                error = InitHoloPlayCore();
            } else {
                error = hpc_RefreshState();
            }

            results.attempted = error == hpc_client_error.hpc_CLIERR_NOERROR;
            if (results.attempted) {
                int num_displays = hpc_GetNumDevices();
                // TODO: compare json
                bool isChanged = !isInit || PlayerPrefs.GetInt(InitKey, DEFAULT) != num_displays;

                PlayerPrefs.SetInt(InitKey, num_displays);

                if (isChanged) Debug.Log("[HoloPlay] Found: " + num_displays + " Looking Glass" + (num_displays <= 1 ? "" : "es"));

                results.lkgDisplayFound = num_displays > 0;
                if (results.lkgDisplayFound) {
                    results.calibrationFound = true;
                    for (int i = 0; i < num_displays; i++) {
                        string str = GetDeviceStatus(i);
                        if (str != "nocalibration") {
                            continue;
                        }
                        Debug.Log("[HoloPlay] No calibration found for Looking Glass:" + GetLKGName(i));
                        results.calibrationFound = false;
                    }
                }
            } else {
                PrintError(error);
            }

            CalibrationManager.Refresh();
            return results;
        }

        public static void Reset() {
            if (PlayerPrefs.GetInt(InitKey, DEFAULT) > DEFAULT)
                hpc_CloseApp();
            PlayerPrefs.SetInt(InitKey, DEFAULT);
        }

        private static void PrintError(hpc_client_error errorCode) {
            if (errorCode != hpc_client_error.hpc_CLIERR_NOERROR) {
                string errorMessage;
                switch (errorCode) {
                    case hpc_client_error.hpc_CLIERR_NOSERVICE:
                        errorMessage = "HoloPlay Service not running";
                        break;
                    case hpc_client_error.hpc_CLIERR_SERIALIZEERR:
                        errorMessage = "Client message could not be serialized";
                        break;
                    case hpc_client_error.hpc_CLIERR_VERSIONERR:
                        errorMessage = "Incompatible version of HoloPlay Service";
                        break;
                    case hpc_client_error.hpc_CLIERR_PIPEERROR:
                        errorMessage = "Interprocess pipe broken. Check if HoloPlay Service is still running";
                        break;
                    case hpc_client_error.hpc_CLIERR_SENDTIMEOUT:
                        errorMessage = "Interprocess pipe send timeout";
                        break;
                    case hpc_client_error.hpc_CLIERR_RECVTIMEOUT:
                        errorMessage = "Interprocess pipe receive timeout";
                        break;
                    default:
                        errorMessage = "Unknown error";
                        break;
                }
                Debug.LogWarning(string.Format("[Error] Client access error (code = {0}): {1}!", errorCode, errorMessage));
            }
        }

        public static Calibration[] GetCalibrationArray() {
            int num_displays = hpc_GetNumDevices();
            if (num_displays < 1)
                return null;

            Calibration[] calibrations = new Calibration[num_displays];
            for (int i = 0; i < num_displays; i++) {
                int screenWidth = hpc_GetDevicePropertyScreenW(i);
                int screenHeight = hpc_GetDevicePropertyScreenH(i);
                float subp = hpc_GetDevicePropertySubp(i);
                float viewCone = hpc_GetViewCone(i);
                float aspect = hpc_GetDevicePropertyDisplayAspect(i);
                float pitch = hpc_GetDevicePropertyPitch(i);
                float slope = hpc_GetDevicePropertyTilt(i);
                float center = hpc_GetDevicePropertyCenter(i);
                float fringe = hpc_GetDevicePropertyFringe(i);
                string serial = GetSerial(i);
                string LKGname = GetLKGName(i);
                int xpos = hpc_GetDevicePropertyWinX(i);
                int ypos = hpc_GetDevicePropertyWinY(i);

                float flipImageX = hpc_GetDevicePropertyFloat(i, "/calibration/flipImageX/value");
                float rawSlope = hpc_GetDevicePropertyFloat(i, "/calibration/slope/value");
                float dpi = hpc_GetDevicePropertyFloat(i, "/calibration/DPI/value");

                Calibration newCal = new Calibration(
                    i,
                    GetLKGunityIndex(i),
                    screenWidth,
                    screenHeight,
                    subp,
                    viewCone,
                    aspect,
                    pitch,
                    slope,
                    center,
                    fringe,
                    serial,
                    LKGname,
                    xpos, 
                    ypos,
                    rawSlope,
                    flipImageX,
                    dpi
                );
                calibrations[i] = newCal;
            }
            return calibrations;
        }
    }
}