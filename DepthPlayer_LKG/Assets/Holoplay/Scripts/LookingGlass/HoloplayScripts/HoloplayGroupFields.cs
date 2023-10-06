using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace LookingGlass {
    public partial class Holoplay {
        #region CameraData
        //NOTE: This is NOT orthographicSize.. TODO: Document this...
        [FormerlySerializedAs("size")]
        [SerializeField] internal float m_Size = 5;
        [Range(0.01f, 5)]
        [FormerlySerializedAs("nearClipFactor")]
        [SerializeField] internal float m_NearClipFactor = HoloplayDevice.GetSettings(HoloplayDevice.Type.Portrait).nearClip;
        [Range(0.01f, 40)]
        [FormerlySerializedAs("farClipFactor")]
        [SerializeField] internal float m_FarClipFactor = 4;
        [FormerlySerializedAs("scaleFollowsSize")]
        [SerializeField] internal bool m_ScaleFollowsSize = false;

        [UnityImitatingClearFlags]
        [FormerlySerializedAs("clearFlags")]
        [SerializeField] internal CameraClearFlags m_ClearFlags = CameraClearFlags.Color;
        [FormerlySerializedAs("background")]
        [SerializeField] internal Color m_Background = Color.black;
        [FormerlySerializedAs("cullingMask")]
        [SerializeField] internal LayerMask m_CullingMask = -1;

        [Range(5, 90)]
        [FormerlySerializedAs("fov")]
        [SerializeField] internal float m_FieldOfView = 14;
        [FormerlySerializedAs("depth")]
        [SerializeField] internal float m_Depth = 0;

        [Tooltip("The rendering path to use for rendering each of the single-views.\n\nYou may choose to use the player settings, or explicitly use deferred or forward rendering.")]
        [FormerlySerializedAs("renderingPath")]
        [SerializeField] internal RenderingPath m_RenderingPath = RenderingPath.UsePlayerSettings;

        [FormerlySerializedAs("occlusionCulling")]
        [SerializeField] internal bool useOcclusionCulling = true;
        [FormerlySerializedAs("allowHDR")]
        [SerializeField] internal bool m_AllowHDR = true;
        [FormerlySerializedAs("allowMSAA")]
        [SerializeField] internal bool m_AllowMSAA = true;
#if UNITY_2017_3_OR_NEWER
        [FormerlySerializedAs("allowDynamicResolution")]
        [SerializeField] internal bool m_AllowDynamicResolution = false;
#endif

        [Tooltip("Determines whether or not the frustum target will be used.")]
        [FormerlySerializedAs("useFrustumTarget")]
        [SerializeField] internal bool m_UseFrustumTarget;
        [FormerlySerializedAs("frustumTarget")]
        [SerializeField] internal Transform m_FrustumTarget;

        [Tooltip("Represents how 3-dimensional the final screen image on the LKG device will appear, as a percentage in the range of [0, 1].\n\n" +
            "The default value is 1.")]
        [Range(0, 1)]
        [FormerlySerializedAs("viewconeModifier")]
        [SerializeField] internal float m_ViewconeModifier = 1;

        [Tooltip("Offsets the cycle of horizontal views based on the observer's viewing angle, represented as a percentage on a scale of [-0.5, 0.5].\n\n" +
            "Only applies in the Unity editor, ignored in builds.\n\n" +
            "The default value is 0.")]
        [Range(-0.5f, 0.5f)]
        [FormerlySerializedAs("centerOffset")]
        [SerializeField] internal float m_CenterOffset;
        [Range(-90, 90)]
        [FormerlySerializedAs("horizontalFrustumOffset")]
        [SerializeField] internal float m_HorizontalFrustumOffset;
        [Range(-90, 90)]
        [FormerlySerializedAs("verticalFrustumOffset")]
        [SerializeField] internal float m_VerticalFrustumOffset;
        #endregion

        #region Gizmos
        [FormerlySerializedAs("drawHandles")]
        [SerializeField] internal bool m_DrawHandles = true;
        [FormerlySerializedAs("frustumColor")]
        [SerializeField] internal Color m_FrustumColor = new Color32(0, 255, 0, 255);
        [FormerlySerializedAs("middlePlaneColor")]
        [SerializeField] internal Color m_MiddlePlaneColor = new Color32(150, 50, 255, 255);
        [FormerlySerializedAs("handleColor")]
        [SerializeField] internal Color m_HandleColor = new Color32(75, 100, 255, 255);
        #endregion

        #region Events
        [Tooltip("If you have any functions that rely on the calibration having been loaded " +
            "and the screen size having been set, let them trigger here")]
        [FormerlySerializedAs("onHoloplayReady")]
        [SerializeField] internal HoloplayLoadEvent m_OnHoloplayReady;
        [Tooltip("Will fire before each individual view is rendered. " +
            "Passes [0, numViews), then fires once more passing numViews (in case cleanup is needed)")]
        [FormerlySerializedAs("onViewRender")]
        [SerializeField] internal HoloplayViewRenderEvent m_OnViewRender;
        #endregion

        #region Optimization
        [FormerlySerializedAs("viewInterpolation")]
        [SerializeField] internal ViewInterpolationType m_ViewInterpolation = ViewInterpolationType.None;
        [FormerlySerializedAs("reduceFlicker")]
        [SerializeField] internal bool m_ReduceFlicker;
        [FormerlySerializedAs("fillGaps")]
        [SerializeField] internal bool m_FillGaps;
        [FormerlySerializedAs("blendViews")]
        [SerializeField] internal bool m_BlendViews;
        #endregion

        #region Debugging
        [Tooltip("When set to true, this reveals hidden objects used by this " +
            nameof(Holoplay) + " component, such as the cameras used for rendering.")]
        [FormerlySerializedAs("showAllObjects")]
        [SerializeField] internal bool m_ShowAllObjects = false;
        #endregion
    }
}
