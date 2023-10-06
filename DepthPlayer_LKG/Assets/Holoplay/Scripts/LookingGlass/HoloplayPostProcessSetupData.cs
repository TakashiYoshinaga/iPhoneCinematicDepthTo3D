using System;
using UnityEngine;

namespace LookingGlass {
    /// <summary>
    /// When post-processing is present, this is the result generated during <see cref="Holoplay.OnEnable"/>.
    /// </summary>
    [Serializable]
    internal struct HoloplayPostProcessSetupData {
        //TODO: Document why we need a 2nd camera for post-processing effects?

        /// <summary>
        /// The main camera used for rendering.
        /// </summary>
        public Camera singleViewCamera;

        /// <summary>
        /// The camera used for rendering post-processing effects.
        /// </summary>
        public Camera postProcessCamera;
    }
}
