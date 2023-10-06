//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using UnityEngine;
using UnityEditor;

namespace LookingGlass.Editor {
        // Ensure class initializer is called whenever scripts recompile
    [InitializeOnLoad]
    public class EditorStateTracking
    {
        static void Quitting()
        {
			// Debug.Log("Editor quitting");
            PluginCore.Reset();
        }

        static EditorStateTracking()
        {
			PluginCore.Reset();
#if UNITY_2018_1_OR_NEWER
            EditorApplication.quitting += Quitting;
#endif
        }
	
    }
}
