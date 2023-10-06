using System;
using System.Text;
using System.Runtime.InteropServices;

namespace LookingGlass {
    //NOTE: WINDOWS-ONLY as currently written!
    internal static class OSWindowUtil {
        private const string User32DLL = "User32";

        [DllImport(User32DLL)]
        public static extern bool EnumWindows(Func<IntPtr, int, bool> callbackPerWindow, int unknown);

        [DllImport(User32DLL)]
        public static extern bool MoveWindow(IntPtr window, int x, int y, int width, int height, bool repaint);

        [DllImport(User32DLL)]
        private static extern int GetWindowTextW(IntPtr window, StringBuilder title, int maxCharacters);
        public static string GetWindowTextW(IntPtr window) {
            int maxCharacters = 256;
            StringBuilder buffer = new StringBuilder(maxCharacters);
            int count = GetWindowTextW(window, buffer, maxCharacters);
            return buffer.ToString();
        }
    }
}