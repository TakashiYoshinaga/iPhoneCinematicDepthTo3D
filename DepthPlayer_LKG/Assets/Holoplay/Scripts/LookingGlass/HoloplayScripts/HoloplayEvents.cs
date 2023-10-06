using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace LookingGlass {
    /// <summary>
    /// Contains the events that a <see cref="Holoplay"/> component will fire off.
    /// </summary>
    [Serializable]
    public class HoloplayEvents : HoloplayPropertyGroup {
        public HoloplayLoadEvent OnHoloplayReady {
            get { return holoplay.m_OnHoloplayReady; }
            internal set { holoplay.m_OnHoloplayReady = value; } //NOTE: Setter available for serialization layout updates
        }

        public HoloplayViewRenderEvent OnViewRender {
            get { return holoplay.m_OnViewRender; }
            internal set { holoplay.m_OnViewRender = value; } //NOTE: Setter available for serialization layout updates
        }
    }
}
