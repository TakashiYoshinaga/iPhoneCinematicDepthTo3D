//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using UnityEngine;

namespace LookingGlass {
    /// <summary>
    /// A callback for receiving notifications when calibrations are refreshed from HoloPlay Core.
    /// </summary>
    public delegate void CalibrationRefreshEvent();

    public static class CalibrationManager {
        private static bool initialized = false;
        private static Calibration[] calibrations;

        private static class WarningMessages {
            public const string NotInitialized = "The " + nameof(CalibrationManager) + " has not initialized yet!";
        }

        /// <summary>
        /// An event that gets fired when calibrations are refreshed from HoloPlay Core.
        /// </summary>
        public static event CalibrationRefreshEvent onRefresh;

        public static bool HasAnyCalibrations => calibrations != null && calibrations.Length > 0;
        public static int CalibrationCount => calibrations?.Length ?? 0;

        public static bool IsIndexValid(int lkgIndex) => initialized && lkgIndex >= 0 && lkgIndex < calibrations.Length;

        public static void Refresh() {
            calibrations = PluginCore.GetCalibrationArray();
            initialized = true;
            onRefresh?.Invoke();
        }

        public static Calibration GetCalibration(int lkgIndex) {
            Calibration calibration = default;
            if (!initialized) {
                Debug.LogWarning(WarningMessages.NotInitialized);
                return calibration;
            }

            if (CalibrationCount > 0)
                calibration = calibrations[0];

            if (!IsIndexValid(lkgIndex)) {
                Debug.LogWarning("Calibration index " + lkgIndex + " is invalid! (There are " + CalibrationCount + " calibrations).");
                return calibration;
            }

            calibration = calibrations[lkgIndex];
            return calibration;
        }

        public static bool TryGetCalibration(int lkgIndex, out Calibration calibration) {
            calibration = GetCalibration(lkgIndex);
            return calibration.IsValid;
        }

        /// <summary>
        /// Attempts to find a <see cref="Calibration"/> by its <see cref="Calibration.LKGname"/>.
        /// </summary>
        /// <param name="lkgName">The name of the LKG device you are searching for.</param>
        /// <param name="calibration">The resulting calibration that was found, if any.</param>
        public static bool TryFindCalibration(string lkgName, out Calibration calibration) {
            calibration = default;
            if (!initialized) {
                Debug.LogWarning(WarningMessages.NotInitialized);
                return false;
            }

            if (CalibrationCount > 0)
                calibration = GetCalibration(0);

            for (int i = 0; i < calibrations.Length; i++) {
                if (calibrations[i].LKGname == lkgName) {
                    calibration = calibrations[i];
                    return true;
                }
            }
            return false;
        }
    }
}
