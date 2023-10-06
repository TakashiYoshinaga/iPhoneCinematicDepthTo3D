//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LookingGlass {
    // NOTE: holoplayer one button types are deprecated.
    // they still map to 0, 1, 2, and 3 just like the Looking Glass buttons
    // if you do not wish to refactor your code, you can add HOLOPLAYER_ONE_BUTTONS to your scripting define symbols
    public enum ButtonType {
		NONE = -1,
		SQUARE = 0,
		LEFT = 1,
		RIGHT = 2,
		CIRCLE = 3,
#if HOLOPLAYER_ONE_BUTTONS
        ONE = 0,
		TWO = 1,
		THREE = 2,
		FOUR = 3,
#endif
	}

    [HelpURL("https://docs.lookingglassfactory.com/Unity/Scripts/ButtonManager/")]
	public class ButtonManager : MonoBehaviour {

		[Tooltip("Emulate the Looking Glass Buttons using 1/2/3/4 on alphanumeric keyboard.")]
        public bool emulateWithKeyboard = true;
        private int joystickNumber = -2;
        // balance checkInterval so it starts right away
        private float timeSinceLastCheck = -3f;
        private readonly float checkInterval = 3f;
        private Dictionary<int, KeyCode> buttonKeyCodes;
        private static ButtonManager instance;
        public static ButtonManager Instance {
            get {
                if (instance != null) return instance;
                instance = FindObjectOfType<ButtonManager>();
                if (instance != null) return instance;
                var HoloPlayManager = GameObject.Find("Holoplay Button Manager");
                if (HoloPlayManager == null)
                    HoloPlayManager = new GameObject("Holoplay Button Manager");
                instance = HoloPlayManager.AddComponent<ButtonManager>();
                return instance;
            }
        }

        void OnEnable() {
            if (Instance != null && Instance != this)
                Destroy(this);
            buttonKeyCodes = new Dictionary<int, KeyCode>();
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// This happens automatically every x seconds as called from Holoplay.
        /// No need for manually calling this function typically
        /// </summary>
        void ScanForHoloplayJoystick() {
            if (Time.unscaledTime - timeSinceLastCheck > checkInterval) {
                var joyNames = Input.GetJoystickNames();
                int i = 1;
                foreach (var joyName in joyNames) {
                    if (joyName.ToLower().Contains("holoplay")) {
                        // removed line to reduce console clutter
                        joystickNumber = i; // for whatever reason unity starts their joystick list at 1 and not 0
                        return;
                    }
                    i++;
                }
                if (joystickNumber == -2) {
                    Debug.LogWarning("[Holoplay] No Holoplay HID found");
                    joystickNumber = -1;
                }
                timeSinceLastCheck = Time.unscaledTime;
            }
        }

        public static bool GetButton(ButtonType button) {
            if (button == ButtonType.NONE) {
                return false;
            }
            return CheckButton((x) => UnityEngine.Input.GetKey(x), button);
        }

        public static bool GetButtonDown(ButtonType button) {
            if (button == ButtonType.NONE) {
                return false;
            }
            return CheckButton((x) => UnityEngine.Input.GetKeyDown(x), button);
        }

        public static bool GetButtonUp(ButtonType button) {
            if (button == ButtonType.NONE) {
                return false;
            }
            return CheckButton((x) => UnityEngine.Input.GetKeyUp(x), button);
        }

        /// <summary>
        /// Get any button down.
        /// </summary>
        public static bool GetAnyButtonDown() {
            for (int i = 0; i < 4; i++) {
                if (GetButtonDown((ButtonType)i)) return true;
            }
            return false;
        }

        static bool CheckButton(Func<KeyCode, bool> buttonFunc, ButtonType button) {
            bool buttonPress = false;
            // check keyboard if emulated
            if (Instance.emulateWithKeyboard)
                buttonPress = buttonFunc(ButtonToNumberOnKeyboard(button));
            if (Instance.joystickNumber < 0)
                Instance.ScanForHoloplayJoystick();
            if (Instance.joystickNumber >= 0)
                buttonPress |= buttonFunc(ButtonToJoystickKeycode(button));
            return buttonPress;
        }

        static KeyCode ButtonToJoystickKeycode(ButtonType button) {
            if (!Instance.buttonKeyCodes.ContainsKey((int)button)) {
                KeyCode buttonKey = (KeyCode)Enum.Parse(
                    typeof(KeyCode), "Joystick" + Instance.joystickNumber + "Button" + (int)button
                );
                Instance.buttonKeyCodes.Add((int)button, buttonKey);
            }
            return Instance.buttonKeyCodes[(int)button];
        }

        static KeyCode ButtonToNumberOnKeyboard(ButtonType button) {
            switch (button) {
                case ButtonType.SQUARE: return KeyCode.Alpha1;
                case ButtonType.LEFT: return KeyCode.Alpha2;
                case ButtonType.RIGHT: return KeyCode.Alpha3;
                case ButtonType.CIRCLE: return KeyCode.Alpha4;
                default: return KeyCode.Alpha1;
            }
        }
	}
}