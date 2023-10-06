//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using UnityEngine;

namespace LookingGlass {
    public static class Quilt {
        [Serializable]
        public enum Preset {
            Custom = -2,
            Automatic = -1,
            Portrait = 0,
            HiResPortrait = 1,
            FourKStandard = 2,
            EightKStandard = 3,
        }

        [Serializable]
        public struct Settings : ISerializationCallbackReceiver {
            public const int MinSize = 256;
            public const int MaxSize = 8192;
            public const int MinRowColumnCount = 1;
            public const int MaxRowColumnCount = 32;
            public const int MinViews = 1;
            public const int MaxViews = 128;

            //WARNING: There are still public-exposed fields!
            [Range(MinSize, MaxSize)]
            public int quiltWidth;

            [Range(MinSize, MaxSize)]
            public int quiltHeight;

            [Range(MinRowColumnCount, MaxRowColumnCount)]
            public int viewColumns;

            [Range(MinRowColumnCount, MaxRowColumnCount)]
            public int viewRows;

            [Range(MinViews, MaxViews)]
            public int numViews;

            [Tooltip("The aspect ratio of the target LKG device's native resolution, (screenWidth / screenHeight).\n" +
                "To use the default aspect for the current Looking Glass, set this value to -1.")]
            public float aspect;

            [Tooltip("If custom aspect differs from current Looking Glass aspect, " +
                "this will toggle between overscan (zoom w/ crop) or letterbox (black borders)")]
            public bool overscan;

            //TODO: [HPU~0] Remove this Hungarian notation when we remove the obsolete camelCase member!
            private int m_ViewWidth;
            private int m_ViewHeight;
            private int m_PaddingHorizontal;
            private int m_PaddingVertical;
            private float m_ViewPortionHorizontal;
            private float m_ViewPortionVertical;

            //DEPRECATED: v1.5.0
            [Obsolete("Use Quilt.Settings.ViewWidth instead.")]
            public int viewWidth => ViewWidth;

            //DEPRECATED: v1.5.0
            [Obsolete("Use Quilt.Settings.ViewHeight instead.")]
            public int viewHeight => ViewHeight;
            
            //DEPRECATED: v1.5.0
            [Obsolete("Use Quilt.Settings.PaddingHorizontal instead.")]
            public int paddingHorizontal => PaddingHorizontal;
            
            //DEPRECATED: v1.5.0
            [Obsolete("Use Quilt.Settings.PaddingVertical instead.")]
            public int paddingVertical => PaddingVertical;
            
            //DEPRECATED: v1.5.0
            [Obsolete("Use Quilt.Settings.ViewPortionHorizontal instead.")]
            public float viewPortionHorizontal => ViewPortionHorizontal;
            
            //DEPRECATED: v1.5.0
            [Obsolete("Use Quilt.Settings.ViewPortionVertical instead.")]
            public float viewPortionVertical => ViewPortionVertical;

            public int ViewWidth => m_ViewWidth;
            public int ViewHeight => m_ViewHeight;
            public int PaddingHorizontal => m_PaddingHorizontal;
            public int PaddingVertical => m_PaddingVertical;
            public float ViewPortionHorizontal => m_ViewPortionHorizontal;
            public float ViewPortionVertical => m_ViewPortionVertical;

            public Settings(
                int quiltWidth,
                int quiltHeight,
                int viewColumns,
                int viewRows,
                int numViews,
                float aspect = -1,
                bool overscan = false) : this() {

                this.quiltWidth = quiltWidth;
                this.quiltHeight = quiltHeight;
                this.viewColumns = Mathf.Clamp(viewColumns, MinRowColumnCount, MaxRowColumnCount);
                this.viewRows = Mathf.Clamp(viewRows, MinRowColumnCount, MaxRowColumnCount);
                this.numViews = numViews;

                this.aspect = aspect;
                this.overscan = overscan;

                Setup();
            }

            public void OnBeforeSerialize() { }
            public void OnAfterDeserialize() {
                Setup();
            }

            public void Setup() {
                if (viewColumns == 0 || viewRows == 0) {
                    m_ViewWidth = quiltWidth;
                    m_ViewHeight = quiltHeight;
                } else {
                    m_ViewWidth = quiltWidth / viewColumns;
                    m_ViewHeight = quiltHeight / viewRows;
                }
                m_ViewPortionHorizontal = (float) viewColumns * ViewWidth / (float) quiltWidth;
                m_ViewPortionVertical = (float) viewRows * ViewHeight / (float) quiltHeight;
                m_PaddingHorizontal = quiltWidth - viewColumns * ViewWidth;
                m_PaddingVertical = quiltHeight - viewRows * ViewHeight;
            }

            public bool Equals(Settings otherSettings) {
                if (quiltWidth == otherSettings.quiltWidth
                    && quiltHeight == otherSettings.quiltHeight
                    && viewColumns == otherSettings.viewColumns
                    && viewRows == otherSettings.viewRows
                    && numViews == otherSettings.numViews
                    && aspect == otherSettings.aspect
                    && overscan == otherSettings.overscan
                    )
                    return true;
                return false;
            }

            //TODO: What's this todo for? Haha
            // todo: have an override that only takes view count, width, and height
            // and creates as square as possible quilt settings from that
        }

        public static readonly Settings[] presetSettings = new Settings[] {
            new Settings(3360 , 3360 , 8, 6, 48),  // portrait
            new Settings(3840, 3840, 8, 6, 48),  // hi res portrait
            new Settings(4096, 4096, 5, 9, 45), // 4k standard
            new Settings(8192, 8192, 5, 9, 45), // 8k standard
            // new Settings(2048, 2048, 4, 8, 32), // standard
            // new Settings(7680, 6400, 6, 8, 48), // ultra hi
            // new Settings(1600, 1440, 4, 6, 24), // extra low
        };

        //DEPRECATED: v1.5.0
        [Obsolete("Use Quilt.Settings.presetSettings instead.")]
        public static Settings[] presets => presetSettings;

        //WARNING: The calibration data should be UNMODIFIED so that its screenWidth and screenHeight are matched exactly!
        //Otherwise, it'll incorrectly default to Portrait quilt settings.
        private static Preset CalculateAutomaticPreset(Calibration calibration) {
            // making an exception here for portrait
            if (calibration.IsPortrait)
                return Preset.Portrait;

            foreach (HoloplayDevice.Settings preset in HoloplayDevice.presets) {
                if (calibration.screenWidth == preset.screenWidth &&
                    calibration.screenHeight == preset.screenHeight)
                    return preset.quiltPreset;
            }
            Debug.LogWarning("There was no " + nameof(HoloplayDevice)
                + " preset that matched the screen width and height of the given calibration data ("
                + calibration.screenWidth + "x" + calibration.screenHeight + ")! Defaulting to Portrait quilt settings.");
            return Preset.Portrait;
        }

        public static Settings GetSettings(Preset preset, Calibration calibration) {
            Preset actualPresetToUse = preset;
            if (preset == Preset.Automatic)
                actualPresetToUse = CalculateAutomaticPreset(calibration);

            return presetSettings[(int) actualPresetToUse];
        }

        /// <summary>
        /// Gets the default <see cref="Quilt.Settings"/> for a specific type of device.
        /// </summary>
        /// <param name="emulatedDevice">The type of LKG device to get the default settings of.</param>
        public static Settings GetSettings(HoloplayDevice.Type emulatedDevice) {
            HoloplayDevice.Settings deviceSettings = HoloplayDevice.GetSettings(emulatedDevice);
            Settings quiltSettings = presetSettings[(int) deviceSettings.quiltPreset];

            //NOTE: We force the aspect ratio to be that of emulated device,
            //  because it defaults to what it reads from calibration.
            quiltSettings.aspect = deviceSettings.aspectRatio;

            return quiltSettings;
        }
    }
}
