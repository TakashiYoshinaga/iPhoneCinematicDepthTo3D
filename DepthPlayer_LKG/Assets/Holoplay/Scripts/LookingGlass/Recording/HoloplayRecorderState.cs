using System;

namespace LookingGlass {
    /// <summary>
    /// Describes the state of a <see cref="HoloplayRecorder"/>, including whether it is recording or not, or if it's paused.
    /// </summary>
    [Serializable]
    public enum HoloplayRecorderState {
        NotRecording = 0,
        Recording,
        Paused
    }
}
