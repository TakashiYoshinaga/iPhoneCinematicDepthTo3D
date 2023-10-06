using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace LookingGlass.Editor {
    /// <summary>
    /// <para>Contains extension methods for dealing with <see cref="GameView"/>s, which are an internal derived type of <see cref="EditorWindow"/>.</para>
    /// </summary>
    public static class GameViewExtensions {
        private static Type gameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
        private static BindingFlags bindingFlags =
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public;

        private static MethodInfo getGroup;
        private static object gameViewSizesInstance;

        public static Type GameViewType => gameViewType;

        public static GameViewSizeGroupType CurrentGameViewSizeGroupType {
            get {
                Type sizesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instanceProp = singleType.GetProperty("instance");
                getGroup = sizesType.GetMethod("GetGroup");
                gameViewSizesInstance = instanceProp.GetValue(null, null);
                var getCurrentGroupTypeProp = gameViewSizesInstance.GetType().GetProperty("currentGroupType");
                var currentGroupType = (GameViewSizeGroupType) (int) getCurrentGroupTypeProp.GetValue(gameViewSizesInstance, null);
                return currentGroupType;
            }
        }

        //We want to avoid directly just getting EditorWindow.GetWindow(Type) cause it'll
        //re-use ANY currently open window of that type!
        //We don't want to disturb the user's Game View that they're actually using from their window layout.
        //We want a brand new instance of our own to play with.
        private static EditorWindow GetUniqueGameView() {
            EditorWindow gameView = (EditorWindow) ScriptableObject.CreateInstance(gameViewType);
            gameView.Show(); //IMPORTANT! Without this, we may come across internal Unity errors on Mac & Linux for some reason
            return gameView;
        }

        public static EditorWindow[] FindAllGameViews() => (EditorWindow[]) Resources.FindObjectsOfTypeAll(gameViewType);
        public static EditorWindow[] FindUnpairedGameViews() => FindUnpairedGameViewsInternal().ToArray();

        private static IEnumerable<EditorWindow> FindUnpairedGameViewsInternal() {
            foreach (EditorWindow gameView in FindAllGameViews())
                if (!HoloplayPreviewPairs.IsPaired(gameView))
                    yield return gameView;
        }

        public static EditorWindow CreateGameView() => GetUniqueGameView();
        public static EditorWindow CreateGameView(string name) {
            EditorWindow gameView = CreateGameView();
            gameView.name = name;
            return gameView;
        }

        public static void SetGameViewTargetDisplay(this EditorWindow gameView, int targetDisplay) {
#if UNITY_2019_3_OR_NEWER
			gameViewType.GetMethod("set_targetDisplay", bindingFlags).Invoke(gameView, new object[] { targetDisplay });
#else
            FieldInfo targetDisplayField = gameViewType.GetField("m_TargetDisplay", bindingFlags);
            targetDisplayField.SetValue(gameView, targetDisplay);
#endif
        }

        public static int GetGameViewTargetDisplay(this EditorWindow gameView) {
#if UNITY_2019_3_OR_NEWER
            return (int) gameViewType.GetMethod("get_targetDisplay", bindingFlags).Invoke(gameView, new object[] { });
#else
            FieldInfo targetDisplayField = gameViewType.GetField("m_TargetDisplay", bindingFlags);
            return (int) targetDisplayField.GetValue(gameView);
#endif
        }

        public static void SetGameViewZoom(this EditorWindow gameViewWindow) {
            float targetScale = 1;
            var areaField = gameViewType.GetField("m_ZoomArea", bindingFlags);
            var areaObj = areaField.GetValue(gameViewWindow);
            var scaleField = areaObj.GetType().GetField("m_Scale", bindingFlags);
            scaleField.SetValue(areaObj, new Vector2(targetScale, targetScale));
        }

        public static void SetGameViewResolution(this EditorWindow gameView, int width, int height, string deviceTypeName) {
            PropertyInfo selectedSizeIndexProp = gameViewType.GetProperty("selectedSizeIndex", bindingFlags);

            GameViewSizeGroupType groupType = CurrentGameViewSizeGroupType;
            string customSizeName = deviceTypeName + string.Format(" {0} x {1}", width, height);
            bool sizeExists = FindSize(groupType, customSizeName, out int index);

            if (!sizeExists) {
                AddCustomSize(groupType, width, height, deviceTypeName);
            }

            selectedSizeIndexProp.SetValue(gameView, index, null);

            ForceUnityToRepaintScene();
            gameView.Repaint();
        }

        public static Vector2 GetTargetSize(this EditorWindow gameView) {
            PropertyInfo property = gameViewType.GetProperty("targetSize", bindingFlags);
            return (Vector2) property.GetValue(gameView);
        }

        public static Vector2 GetTargetRenderSize(this EditorWindow gameView) {
            PropertyInfo property = gameViewType.GetProperty("targetRenderSize", bindingFlags);
            return (Vector2) property.GetValue(gameView);
        }

        public static void SetFreeAspectSize(this EditorWindow gameView) {
            PropertyInfo property = gameViewType.GetProperty("selectedSizeIndex", bindingFlags);
            property.SetValue(gameView, 0);
        }

        public static int GetSelectedSizeIndex(this EditorWindow gameView) {
            PropertyInfo property = gameViewType.GetProperty("selectedSizeIndex", bindingFlags);
            return (int) property.GetValue(gameView);
        }

        public static void SetShowToolbar(this EditorWindow gameView, bool value) {
            PropertyInfo property = gameViewType.GetProperty("showToolbar", bindingFlags);

            //NOTE: The GameView.showToolbar API member doesn't exist in Unity 2018.4.
            //It DOES exist by 2019.4, but I haven't tested which versions exactly it's in, since it's just a nice-to-have feature to hide it.
            //So if it doesn't exist yet, don't worry about it -- just do nothing and return:
            if (property == null)
                return;
            
            property.SetValue(gameView, value);
        }

        //TODO: Is there really no way to maximize an EditorWindow through code?
        //NOTE: EditorWindow.maximized does NOT provide the functionality we want, it's a different feature that only works when the window is docked.
        /// <summary>
        /// Sends a mouse-down and mouse-up event near the top-right of the <paramref name="window"/>, in hopes of maximizing the window.
        /// Note that on non-Windows platforms, this method does nothing.
        /// This has been tested to work on Unity 2018.4 - 2021.2.
        /// </summary>
        /// <param name="window"></param>
        public static void AutoClickMaximizeButtonOnWindows(this EditorWindow window) {
#if UNITY_EDITOR_WIN
            Event maximizeMouseDown = new Event() {
                type = EventType.MouseDown,
                button = 0,
                mousePosition = new Vector2(0 + window.position.size.x - 25, 8),
            };

            Event maximizeMouseUp = new Event(maximizeMouseDown) {
                type = EventType.MouseUp
            };
            window.SendEvent(maximizeMouseDown);
            window.SendEvent(maximizeMouseUp);
            EditorApplication.QueuePlayerLoopUpdate();
            window.Repaint();
#endif
        }

        //TODO: Remove no-longer necessary API members from here

        #region Zoom Area Size
        [Serializable]
        private class ZoomableArea {
            public DrawArea m_DrawArea; //NOTE: This field's name corresponds to an internal Unity field name to match in JSON serialization
        }

        [Serializable]
        private struct DrawArea {
            public float width; //NOTE: This field's name corresponds to an internal Unity field name to match in JSON serialization
            public float height; //NOTE: This field's name corresponds to an internal Unity field name to match in JSON serialization
        }

        public static Vector2 GetZoomAreaSize(this EditorWindow gameView) {
            FieldInfo field = gameViewType.GetField("m_ZoomArea", bindingFlags);
            object zoomArea = field.GetValue(gameView);

            //NOTE: We could use reflection, but I was lazy and decided to just grab the values from JSON instead.
            string json = EditorJsonUtility.ToJson(zoomArea, true);
            
            ZoomableArea objectFromJson = new ZoomableArea();
            EditorJsonUtility.FromJsonOverwrite(json, objectFromJson);
            Assert.IsFalse(objectFromJson.m_DrawArea.width <= 0, "The game view's zoom area width is expected to be greater than zero! " +
                "(Instead, it was " + objectFromJson.m_DrawArea.width + ". Did it serialize properly?)");
            Assert.IsFalse(objectFromJson.m_DrawArea.height <= 0, "The game view's zoom area height is expected to be greater than zero! " +
                "(Instead, it was " + objectFromJson.m_DrawArea.height + ". Did it serialize properly?)");

            return new Vector2(
                objectFromJson.m_DrawArea.width,
                objectFromJson.m_DrawArea.height
            );
        }

        //Ex: rawWidth = 1536
        //    rawHeight = 2048
        public static Vector2 CalculateRenderAreaSize(this EditorWindow gameView, int rawWidth, int rawHeight) {
            Vector2 zoomArea = gameView.GetZoomAreaSize();
            float zoomAspect = zoomArea.x / zoomArea.y;
            float rawAspect = (float) rawWidth / rawHeight;

            if (zoomAspect > rawAspect) {
                return new Vector2(
                    rawWidth * (zoomArea.y / rawHeight),        //Render area width is scaled by the same percentage of (zoom height / raw height)
                    zoomArea.y
                );
            } else if (zoomAspect < rawAspect) {
                return new Vector2(
                    zoomArea.x,
                    rawHeight * (zoomArea.x / rawWidth)         //Render area height is scaled by the same percentage of (zoom width / raw width)
                );
            } else {
                return zoomArea;
            }
        }
        #endregion

        /// <summary>
        /// Adds a game view size.
        /// </summary>
        /// <param name="sizeGroupType">Build target</param>
        /// <param name="width">Width of game view resolution</param>
        /// <param name="height">Height of game view resolution</param>
        /// <param name="text">Label of game view resolution</param>
        public static void AddCustomSize(GameViewSizeGroupType sizeGroupType, int width, int height, string text) {
            object group = GetGroup(sizeGroupType);
            MethodInfo addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
            Type gameViewSizeType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSize");

            // first parameter is 1 bc it will be fixed resolution in current use cases
            object[] gameViewSizeConstructorArgs = new object[] { 1, width, height, text };

            // select a constructor which has 4 elements which are enums/ints/strings
            ConstructorInfo gameViewSizeConstructor = gameViewSizeType.GetConstructors()
                .FirstOrDefault(x => {
                    // lambda function defines a filter/predicate of ConstructorInfo objects.
                    // The first constructor, if any exists, which satisfies the predicate (true) will be returned
                    if (x.GetParameters().Length != gameViewSizeConstructorArgs.Length)
                        return false;

                    // iterate through constructor types + constructor args. If any mismatch, reject
                    for (int i = 0; i < gameViewSizeConstructorArgs.Length; i++) {
                        Type constructorParamType = x.GetParameters()[i].ParameterType;
                        Type constructorArgType = gameViewSizeConstructorArgs[i].GetType();

                        bool isMatch = constructorParamType == constructorArgType || constructorParamType.IsEnum && constructorArgType == typeof(int);
                        if (!isMatch)
                            return false;
                    }

                    // constructor with these params must be able to receive these args
                    return true;
                });

            if (gameViewSizeConstructor != null) {
                object newSize = gameViewSizeConstructor.Invoke(gameViewSizeConstructorArgs);
                addCustomSize.Invoke(group, new object[] { newSize });
            }
            Type sizesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            sizesType.GetMethod("SaveToHDD").Invoke(gameViewSizesInstance, null);
        }

        public static object GetGroup(GameViewSizeGroupType type) {
            return getGroup.Invoke(gameViewSizesInstance, new object[] { (int) type });
        }

        /// <summary>
        /// Retrieves index of a resolution in GetDisplayTexts collection, if it exists in the collection.
        /// </summary>
        /// <param name="sizeGroupType">Group to search: Standalone/Android</param>
        /// <param name="text">String to search GetDisplayTexts for. Only [0-9] chars in label and GetDisplayTexts are actually considered in search</param>
        /// <param name="index">Index of match if a match was found, or first out-of-bounds index if no match was found</param>
        /// <returns>True if resolution in collection, false if resolution is not in collection</returns>
        public static bool FindSize(GameViewSizeGroupType sizeGroupType, string text, out int index) {
            index = -1;

            text = Regex.Replace(text, @"[\D]", "");
            var group = GetGroup(sizeGroupType);
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
            for (int i = 0; i < displayTexts.Length; i++) {
                // compare the digits of the known resolution names, to the digits of the ideal resolution
                // if digits are a one-for-one match using string ==, then we have a match
                string display = Regex.Replace(displayTexts[i], @"[\D]", "");
                if (display == text) {
                    index = i;
                    return true;
                }
            }

            // otherwise set to first index outside of array bounds, return false to warn user that size is not actually in array
            // inside of SetGameViewSize we will add the as-of-yet unknown size at index [first_index_outside_of_array_bounds]
            index = displayTexts.Length;
            return false;
        }

        private static void ForceUnityToRepaintScene() {
            // Spawn an object, then immediately destroy it.
            // This forces Unity to repaint scene, but does not generate a diff in the Unity scene serialization which would require scene to be re-saved
            // Repainting the scene causes Unity to recalculate UI positions for resized GameViewWindow : EditorWindow
            GameObject go = new GameObject();
            GameObject.DestroyImmediate(go);
        }
    }
}