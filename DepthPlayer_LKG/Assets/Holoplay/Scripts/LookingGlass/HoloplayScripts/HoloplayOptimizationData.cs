using System;
using UnityEngine;

namespace LookingGlass {
    [Serializable]
    public class HoloplayOptimizationData : HoloplayPropertyGroup {
        public Holoplay.ViewInterpolationType ViewInterpolation {
            get { return holoplay.m_ViewInterpolation; }
            set { holoplay.m_ViewInterpolation = value; }
        }

        //TODO: Better document what this means.. the API isn't that self-descriptive.
        public int GetViewInterpolation(int numViews) {
            switch (holoplay.m_ViewInterpolation) {
                case Holoplay.ViewInterpolationType.None:
                default:
                    return 1;
                case Holoplay.ViewInterpolationType.EveryOther:
                    return 2;
                case Holoplay.ViewInterpolationType.Every4th:
                    return 4;
                case Holoplay.ViewInterpolationType.Every8th:
                    return 8;
                case Holoplay.ViewInterpolationType._4Views:
                    return numViews / 3;
                case Holoplay.ViewInterpolationType._2Views:
                    return numViews;
            }
        }

        public bool ReduceFlicker {
            get { return holoplay.m_ReduceFlicker; }
            set { holoplay.m_ReduceFlicker = value; }
        }

        public bool FillGaps {
            get { return holoplay.m_FillGaps; }
            set { holoplay.m_FillGaps = value; }
        }

        public bool BlendViews {
            get { return holoplay.m_BlendViews; }
            set { holoplay.m_BlendViews = value; }
        }
    }
}
