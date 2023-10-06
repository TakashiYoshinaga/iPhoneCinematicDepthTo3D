using UnityEngine;
using UnityEngine.UI;

namespace LookingGlass.UI {
    [ExecuteAlways]
    [RequireComponent(typeof(RawImage))]
    public class HoloplayRawImageUpdater : MonoBehaviour {
        [SerializeField] private Holoplay holoplay;
        private RawImage rawImage;

        private Holoplay Holoplay {
            get { return holoplay; }
            set {
                if (holoplay != null)
                    UnregisterEvents();

                holoplay = value;
                UpdateQuilt();
                UpdateTargetDisplay();

                if (holoplay != null) {
                    UnregisterEvents();
                    RegisterEvents();
                }
            }
        }

        #region Unity Messages
        private void Reset() {
            Holoplay = GetComponentInParent<Holoplay>();
        }

        private void Awake() {
            rawImage = GetComponent<RawImage>();
        }

        private void OnEnable() {
            Holoplay = holoplay;
        }
        #endregion

        private void RegisterEvents() {
            holoplay.onTargetDisplayChanged += UpdateTargetDisplay;
            holoplay.onQuiltChanged += UpdateQuilt;
        }

        private void UnregisterEvents() {
            holoplay.onTargetDisplayChanged -= UpdateTargetDisplay;
            holoplay.onQuiltChanged -= UpdateQuilt;
        }

        private void UpdateQuilt() {
            rawImage.texture = holoplay.QuiltTexture;
            rawImage.material = holoplay.LightfieldMaterial;
        }

        private void UpdateTargetDisplay() {
            Canvas c = rawImage.GetComponentInParent<Canvas>();
            c.targetDisplay = (int) holoplay.TargetDisplay;
        }
    }
}