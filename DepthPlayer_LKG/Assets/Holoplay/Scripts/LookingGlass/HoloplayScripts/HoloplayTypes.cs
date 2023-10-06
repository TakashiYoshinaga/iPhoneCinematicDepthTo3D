using System;
using UnityEngine;
using UnityEngine.Events;

namespace LookingGlass {
    /// <summary>
    /// The callback signature used for setting up post-processing, if it is present.
    /// </summary>
    /// <param name="behaviour">The <see cref="Holoplay"/> component in the scene.</param>
    internal delegate HoloplayPostProcessSetupData HoloplayPostProcessSetup(Holoplay behaviour);

    /// <summary>
    /// The callback signature used for disposing of post-processing, if it is present.
    /// </summary>
    /// <param name="behaviour">The <see cref="Holoplay"/> component in the scene.</param>
    internal delegate void HoloplayPostProcessDispose(Holoplay behaviour);

    /// <summary>
    /// An event that gets fired when a single view is rendered.
    /// </summary>
    [Serializable]
    public class HoloplayViewRenderEvent : UnityEvent<Holoplay, int> { };

    public partial class Holoplay {
        [Serializable]
        public enum DisplayTarget {
            Display1 = 0,
            Display2,
            Display3,
            Display4,
            Display5,
            Display6,
            Display7,
            Display8,
        }

        [Serializable]
        public enum ViewInterpolationType {
            None,
            EveryOther,
            Every4th,
            Every8th,
            _4Views,
            _2Views
        }
    }
}