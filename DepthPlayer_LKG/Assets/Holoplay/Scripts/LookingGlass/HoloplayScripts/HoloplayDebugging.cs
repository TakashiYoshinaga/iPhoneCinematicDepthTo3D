using System;
using UnityEngine;

namespace LookingGlass {
    /// <summary>
    /// Contains several options, useful in the inspector, for debugging a <see cref="Holoplay"/> component.
    /// </summary>
    [Serializable]
    public class HoloplayDebugging : HoloplayPropertyGroup {
        [NonSerialized] private bool wasShowingObjects = false;

        internal event Action onShowAllObjectsChanged;

        public bool ShowAllObjects {
            get { return holoplay.m_ShowAllObjects; }
            set {
                wasShowingObjects = holoplay.m_ShowAllObjects = value;
                onShowAllObjectsChanged?.Invoke();
            }
        }

        protected override void OnValidate() {
            if (ShowAllObjects != wasShowingObjects)
                ShowAllObjects = ShowAllObjects;
        }
    }
}