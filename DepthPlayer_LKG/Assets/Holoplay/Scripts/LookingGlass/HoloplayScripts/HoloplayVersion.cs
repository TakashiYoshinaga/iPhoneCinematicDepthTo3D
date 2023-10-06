//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using UnityEngine;

namespace LookingGlass {
    public partial class Holoplay {
        /// <summary>
        /// The currently-running version of the HoloPlay Unity Plugin.
        /// </summary>
        public static readonly Version Version = new Version(1, 5, 0);

        /// <summary>
        /// A human-readable label for the current version, if any.
        /// </summary>
        public const string VersionLabel = "";

        internal static readonly Version MajorHDRPRefactorVersion = new Version(1, 5, 0);
        internal static bool IsUpdatingBetween(Version previous, Version next, Version threshold)
            => previous < next && previous < threshold && next >= threshold;
    }
}
