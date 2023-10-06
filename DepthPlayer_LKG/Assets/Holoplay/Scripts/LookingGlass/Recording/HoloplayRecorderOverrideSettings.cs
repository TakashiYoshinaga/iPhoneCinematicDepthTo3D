using System;
using UnityEngine;

namespace LookingGlass {
    /// <summary>
    /// Represents the settings that a <see cref="HoloplayRecorder"/> may use when recording, instead of using the values on its <see cref="Holoplay"/> component.
    /// </summary>
    [Serializable]
    public class HoloplayRecorderOverrideSettings {
        [Tooltip("When set to true, the override settings below will be used over the " + nameof(Holoplay) + "'s values during recording.")]
        [SerializeField] private bool enabled;

        [Tooltip("The quilt settings to use during recording.")]
        [SerializeField] private Quilt.Settings quiltSettings = Quilt.GetSettings(HoloplayDevice.Type.Portrait);

        [Tooltip("The near clip factor to use during recording.")]
        [SerializeField] private float nearClipFactor = HoloplayDevice.GetSettings(HoloplayDevice.Type.Portrait).nearClip;

        /// <summary>
        /// <para>Should any of these settings be used over the <see cref="Holoplay"/> component's values?</para>
        /// <para>Note that only all settings will be overridden, or none will be overridden.</para>
        /// </summary>
        public bool Enabled {
            get { return enabled; }
            set { enabled = value; }
        }

        public Quilt.Settings QuiltSettings {
            get { return quiltSettings; }
            set { quiltSettings = value; }
        }

        public float NearClipFactor {
            get { return nearClipFactor; }
            set { nearClipFactor = value; }
        }
    }
}
