using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LookingGlass {
    /// <summary>
    /// A set of fields that correspond to fields on a Unity <see cref="Camera"/>, with some extra holoplay fields.
    /// </summary>
    [Serializable]
    public class HoloplayCameraData : HoloplayPropertyGroup {
        public float Size {
            get { return holoplay.m_Size; }
            set { holoplay.m_Size = Mathf.Max(0.01f, value); }
        }

        public float NearClipFactor {
            get { return holoplay.m_NearClipFactor; }
            set { holoplay.m_NearClipFactor = Mathf.Clamp(value, 0.01f, 5); }
        }

        public float FarClipFactor {
            get { return holoplay.m_FarClipFactor; }
            set { holoplay.m_FarClipFactor = Mathf.Clamp(value, 0.01f, 40); }
        }

        public bool ScaleFollowsSize {
            get { return holoplay.m_ScaleFollowsSize; }
            set { holoplay.m_ScaleFollowsSize = value; }
        }

        public CameraClearFlags ClearFlags {
            get { return holoplay.m_ClearFlags; }
            set { holoplay.m_ClearFlags = value; }
        }

        public Color BackgroundColor {
            get { return holoplay.m_Background; }
            set { holoplay.m_Background = value; }
        }

        public LayerMask CullingMask {
            get { return holoplay.m_CullingMask; }
            set { holoplay.m_CullingMask = value; }
        }

        public float FieldOfView {
            get { return holoplay.m_FieldOfView; }
            set { holoplay.m_FieldOfView = Mathf.Clamp(value, 5, 90); }
        }

        public float Depth {
            get { return holoplay.m_Depth; }
            set { holoplay.m_Depth = Mathf.Clamp(value, -100, 100); }
        }

        public RenderingPath RenderingPath {
            get { return holoplay.m_RenderingPath; }
            set { holoplay.m_RenderingPath = value; }
        }

        public bool UseOcclusionCulling {
            get { return holoplay.useOcclusionCulling; }
            set { holoplay.useOcclusionCulling = value; }
        }

        public bool AllowHDR {
            get { return holoplay.m_AllowHDR; }
            set { holoplay.m_AllowHDR = value; }
        }

        public bool AllowMSAA {
            get { return holoplay.m_AllowMSAA; }
            set { holoplay.m_AllowMSAA = value; }
        }

#if UNITY_2017_3_OR_NEWER
        public bool AllowDynamicResolution {
            get { return holoplay.m_AllowDynamicResolution; }
            set { holoplay.m_AllowDynamicResolution = value; }
        }
#endif

        public bool UseFrustumTarget {
            get { return holoplay.m_UseFrustumTarget; }
            set { holoplay.m_UseFrustumTarget = value; }
        }

        public Transform FrustumTarget {
            get { return holoplay.m_FrustumTarget; }
            set { holoplay.m_FrustumTarget = value; }
        }

        public float ViewconeModifier {
            get { return holoplay.m_ViewconeModifier; }
            set { holoplay.m_ViewconeModifier = Mathf.Clamp01(value); }
        }

        public float CenterOffset {
            get { return holoplay.m_CenterOffset; }
            set { holoplay.m_CenterOffset = Mathf.Clamp01(value); }
        }

        public float HorizontalFrustumOffset {
            get { return holoplay.m_HorizontalFrustumOffset; }
            set { holoplay.m_HorizontalFrustumOffset = Mathf.Clamp(value, -90, 90); }
        }

        public float VerticalFrustumOffset {
            get { return holoplay.m_VerticalFrustumOffset; }
            set { holoplay.m_VerticalFrustumOffset = Mathf.Clamp(value, -90, 90); }
        }

        protected override void OnValidate() {
            Depth = Depth;
        }

        public void SetCamera(Camera camera, Transform scaleTarget, float aspect, float distance) {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            float size = Size;

            if (ScaleFollowsSize)
                scaleTarget.localScale = new Vector3(size, size, size);

            camera.orthographic = false;
            if (UseFrustumTarget)
                camera.fieldOfView = 2 * Mathf.Atan(Mathf.Abs(size / FrustumTarget.localPosition.z)) * Mathf.Rad2Deg;
            else
                camera.fieldOfView = FieldOfView;

            camera.ResetWorldToCameraMatrix();
            camera.ResetProjectionMatrix();
            Matrix4x4 centerViewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 centerProjMatrix = camera.projectionMatrix;
            centerViewMatrix.m23 -= distance;

            if (UseFrustumTarget) {
                Vector3 targetPos = -FrustumTarget.localPosition;
                centerViewMatrix.m03 += targetPos.x;
                centerProjMatrix.m02 += targetPos.x / (size * aspect);
                centerViewMatrix.m13 += targetPos.y;
                centerProjMatrix.m12 += targetPos.y / size;
            } else {
                if (HorizontalFrustumOffset != 0) {
                    float offset = distance * Mathf.Tan(Mathf.Deg2Rad * HorizontalFrustumOffset);
                    centerViewMatrix.m03 += offset;
                    centerProjMatrix.m02 += offset / (size * aspect);
                }
                if (VerticalFrustumOffset != 0) {
                    float offset = distance * Mathf.Tan(Mathf.Deg2Rad * VerticalFrustumOffset);
                    centerViewMatrix.m13 += offset;
                    centerProjMatrix.m12 += offset / size;
                }
            }
            camera.worldToCameraMatrix = centerViewMatrix;
            camera.projectionMatrix = centerProjMatrix;


            camera.nearClipPlane = Mathf.Max(distance - size * NearClipFactor, 0.1f);
            camera.farClipPlane = Mathf.Max(distance + size * FarClipFactor, camera.nearClipPlane);

            camera.clearFlags = ClearFlags;
            
            //TODO: Does this work properly in HDRP?
            //(I had seen somewhere that we need to change a field on the HDAdditionalCameraData component)
            camera.backgroundColor = BackgroundColor;

            camera.depth = Depth;

            camera.cullingMask = CullingMask;
            camera.renderingPath = RenderingPath;
            camera.useOcclusionCulling = UseOcclusionCulling;
            camera.allowHDR = AllowHDR;
            camera.allowMSAA = AllowMSAA;
#if UNITY_2017_3_OR_NEWER
            camera.allowDynamicResolution = AllowDynamicResolution;
#endif
        }
    }
}
