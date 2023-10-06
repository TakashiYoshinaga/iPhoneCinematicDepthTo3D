using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace LookingGlass {
    [Serializable]
    public abstract class HoloplayPropertyGroup : ISerializationCallbackReceiver {
        [SerializeField] protected Holoplay holoplay;

        internal void Init(Holoplay holoplay) {
            Assert.IsNotNull(holoplay);
            this.holoplay = holoplay;
        }

        public void OnBeforeSerialize() {
            //For some reason, there are some times where OnBeforeSerialize is called before...
            //  - The Holoplay constructor
            //  - Holoplay.Awake
            //  - Holoplay.OnEnable
            //So to prevent unnecessary NullReferenceExceptions, let's use inheritance here with our own OnValidate()
            //To only be called when the holoplay field is non-null:
            if (holoplay != null)
                OnValidate();
        }
        public void OnAfterDeserialize() { }

        protected virtual void OnValidate() { }
    }
}
