using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LookingGlass {
    internal static class HoloplayRendering {
        [Serializable]
        private struct ViewInterpolationProperties {
            private bool initialized;
            public bool Initialized => initialized;

            public ShaderPropertyId result;
            public ShaderPropertyId resultDepth;
            public ShaderPropertyId nearClip;
            public ShaderPropertyId farClip;
            public ShaderPropertyId focalDist;
            public ShaderPropertyId perspw;
            public ShaderPropertyId viewSize;
            public ShaderPropertyId viewPositions;
            public ShaderPropertyId viewOffsets;
            public ShaderPropertyId baseViewPositions;
            public ShaderPropertyId spanSize;
            public ShaderPropertyId px;

            public void InitializeAll() {
                initialized = true;

                result = "Result";
                resultDepth = "ResultDepth";
                nearClip = "_NearClip";
                farClip = "_FarClip";
                focalDist = "focalDist";
                perspw = "perspw";
                viewSize = "viewSize";
                viewPositions = "viewPositions";
                viewOffsets = "viewOffsets";
                baseViewPositions = "baseViewPositions";
                spanSize = "spanSize";
                px = "px";

            }
        }
        [Serializable]
        private struct LightfieldProperties {
            private bool initialized;
            public bool Initialized => initialized;

            public ShaderPropertyId pitch;
            public ShaderPropertyId slope;
            public ShaderPropertyId center;
            public ShaderPropertyId subpixelSize;
            public ShaderPropertyId tile;
            public ShaderPropertyId viewPortion;
            public ShaderPropertyId aspect; //NOTE: CORRESPONDS TO Calibration.aspect
            public ShaderPropertyId verticalOffset;

            public void InitializeAll() {
                initialized = true;

                pitch = "pitch";
                slope = "slope";
                center = "center";
                subpixelSize = "subpixelSize";
                tile = "tile";
                viewPortion = "viewPortion";
                aspect = "aspect";
                verticalOffset = "verticalOffset";
            }
        }

        private static ComputeShader interpolationComputeShader;
        private static ViewInterpolationProperties interpolationProperties;
        private static LightfieldProperties lightfieldProperties;

        internal static void ClearBeforeRendering(Holoplay holoplay) {
            if ((holoplay.CameraData.ClearFlags | CameraClearFlags.SolidColor) != 0) {
                RenderTexture.active = holoplay.QuiltTexture;
                GL.Clear(true, true, holoplay.CameraData.BackgroundColor, 1);
            }
        }

        internal static void RenderQuilt(Holoplay holoplay, Action<int> onViewRender) {
            RenderTexture quilt = holoplay.QuiltTexture;
            Quilt.Settings quiltSettings = holoplay.QuiltSettings;

            Camera singleViewCamera = holoplay.SingleViewCamera;
            Camera postProcessCamera = holoplay.PostProcessCamera;
            HoloplayCameraData cameraData = holoplay.CameraData;

            Matrix4x4 centerViewMatrix = singleViewCamera.worldToCameraMatrix;
            Matrix4x4 centerProjMatrix = singleViewCamera.projectionMatrix;
            float singleViewAspect = (float) quiltSettings.ViewWidth / quiltSettings.ViewHeight;

            holoplay.UpdateLightfieldMaterial();

            Calibration cal = holoplay.Calibration;
            float cameraDistance = holoplay.GetCameraDistance();

            float viewCone = Application.isPlaying && cal.viewCone == 0 ? Calibration.DEFAULT_VIEWCONE : cal.viewCone;
            float viewConeSweep = -cameraDistance * Mathf.Tan(viewCone * cameraData.ViewconeModifier * Mathf.Deg2Rad);

            // projection matrices must be modified in terms of focal plane size
            float projModifier = 1 / (cameraData.Size * singleViewCamera.aspect);

            RenderTexture viewRT = null;
            RenderTexture viewRTDepth = null;
            RenderTexture quiltRTDepth = null;

            RenderTextureDescriptor depthDescriptor = quilt.descriptor;
            depthDescriptor.colorFormat = RenderTextureFormat.RFloat;
            quiltRTDepth = RenderTexture.GetTemporary(depthDescriptor);
            quiltRTDepth.Create();

            RenderTexture.active = quiltRTDepth;
            GL.Clear(true, true, Color.black, 1);
            RenderTexture.active = null;

#if UNITY_POST_PROCESSING_STACK_V2
            bool hasPPCam = postProcessCamera != null;
            if (hasPPCam)
                postProcessCamera.CopyFrom(singleViewCamera);
#endif

            //NOTE: This FOV trick is on purpose, to keep shadows from disappearing.

            //We use a large 135° FOV so that lights and shadows DON'T get culled out in our individual single-views!
            //But, this FOV is ignored when we actually render, because we modify the camera matrices.
            //So, we get the best of both worlds -- rendering correctly with no issues with culling.
            singleViewCamera.fieldOfView = 135;

            int viewInterpolation = holoplay.Optimization.GetViewInterpolation(quiltSettings.numViews);
            for (int i = 0; i < quiltSettings.numViews; i++) {
                if (i % viewInterpolation != 0 && i != quiltSettings.numViews - 1)
                    continue;

                //TODO: Is there a reason we don't notify after the view has **finished** rendering? (below, at the bottom of this for loop block)
                onViewRender?.Invoke(i);

                viewRT = RenderTexture.GetTemporary(quiltSettings.ViewWidth, quiltSettings.ViewHeight, 24);
                viewRTDepth = RenderTexture.GetTemporary(quiltSettings.ViewWidth, quiltSettings.ViewHeight, 24, RenderTextureFormat.Depth);

                singleViewCamera.SetTargetBuffers(viewRT.colorBuffer, viewRTDepth.depthBuffer);
                singleViewCamera.aspect = singleViewAspect;

                // move the camera
                Matrix4x4 viewMatrix = centerViewMatrix;
                Matrix4x4 projMatrix = centerProjMatrix;

                float currentViewLerp = 0; // if numviews is 1, take center view
                if (quiltSettings.numViews > 1)
                    currentViewLerp = (float) i / (quiltSettings.numViews - 1) - 0.5f;

                viewMatrix.m03 += currentViewLerp * viewConeSweep;
                projMatrix.m02 += currentViewLerp * viewConeSweep * projModifier;
                singleViewCamera.worldToCameraMatrix = viewMatrix;
                singleViewCamera.projectionMatrix = projMatrix;

                singleViewCamera.Render();
                CopyViewToQuilt(quiltSettings, i, viewRT, quilt);

                // gotta create a weird new viewRT now
                RenderTextureDescriptor viewRTRFloatDesc = viewRT.descriptor;
                viewRTRFloatDesc.colorFormat = RenderTextureFormat.RFloat;
                RenderTexture viewRTRFloat = RenderTexture.GetTemporary(viewRTRFloatDesc);
                Graphics.Blit(viewRTDepth, viewRTRFloat);

                CopyViewToQuilt(quiltSettings, i, viewRTRFloat, quiltRTDepth);

                singleViewCamera.targetTexture = null;
                RenderTexture.ReleaseTemporary(viewRT);
                RenderTexture.ReleaseTemporary(viewRTDepth);
                RenderTexture.ReleaseTemporary(viewRTRFloat);

                //NOTE: This helps 3D cursor ReadPixels faster
                GL.Flush();
            }
            // onViewRender final pass
            onViewRender?.Invoke(quiltSettings.numViews);

            //Reset stuff back to what they were originally:
            singleViewCamera.ResetAspect();
            singleViewCamera.worldToCameraMatrix = centerViewMatrix;
            singleViewCamera.projectionMatrix = centerProjMatrix;
            singleViewCamera.fieldOfView = cameraData.FieldOfView;

            // if interpolation is happening, release
            if (viewInterpolation > 1) {
                //TODO: interpolate on the quilt itself
                InterpolateViewsOnQuilt(holoplay, quilt, quiltRTDepth);
            }

#if UNITY_POST_PROCESSING_STACK_V2
            if (hasPPCam) {
#if !UNITY_2018_1_OR_NEWER
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 ||
                    SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12) {
                    FlipRenderTexture(quilt);
                }
#endif
                RunPostProcess(holoplay, quilt, quiltRTDepth);
            }
#endif
            SimpleDOF dof = holoplay.GetComponent<SimpleDOF>();
            if (dof != null && dof.enabled) {
                dof.DoDOF(quilt, quiltRTDepth);
            }
            RenderTexture.ReleaseTemporary(quiltRTDepth);
        }

        public static void CopyViewToQuilt(Quilt.Settings quiltSettings, int viewIndex, RenderTexture view, RenderTexture quilt, bool forceDrawTex = false) {
            //NOTE: not using Graphics.CopyTexture(...) because it's an exact per-pixel copy (100% overwrite, no alpha-blending support).

            int reversedViewIndex = quiltSettings.viewColumns * quiltSettings.viewRows - viewIndex - 1;

            int targetX = (viewIndex % quiltSettings.viewColumns) * quiltSettings.ViewWidth;
            int targetY = (reversedViewIndex / quiltSettings.viewColumns) * quiltSettings.ViewHeight + quiltSettings.PaddingVertical; //NOTE: Reversed here because Y is taken from the top

            Rect viewRect = new Rect(targetX, targetY, quiltSettings.ViewWidth, quiltSettings.ViewHeight);
            Graphics.SetRenderTarget(quilt);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, quiltSettings.quiltWidth, quiltSettings.quiltHeight, 0);
            Graphics.DrawTexture(viewRect, view);
            GL.PopMatrix();
            Graphics.SetRenderTarget(null);
        }

        /// <summary>
        /// Applies post-processing effects to the <paramref name="target"/> texture.<br />
        /// Note that this method does NOT draw anything to the screen. It only writes into the <paramref name="target"/> render texture.
        /// </summary>
        /// <param name="target">The render texture to apply post-processing into.</param>
        /// <param name="depthTexture">The depth texture to use for post-processing effects. This is useful, because you can provide a custom depth texture instead of always using a single <see cref="Camera"/>'s depth texture.</param>
        public static void RunPostProcess(Holoplay holoplay, RenderTexture target, RenderTexture depthTexture) {
            Camera postProcessCamera = holoplay.PostProcessCamera;
            postProcessCamera.cullingMask = 0;
            postProcessCamera.clearFlags = CameraClearFlags.Nothing;
            postProcessCamera.targetTexture = target;

            Shader.SetGlobalTexture("_FAKEDepthTexture", depthTexture);
            postProcessCamera.Render();
        }

        public static RenderTexture RenderPreview2D(Holoplay holoplay) {
            Profiler.BeginSample(nameof(RenderPreview2D), holoplay);
            try {
                Profiler.BeginSample("Create " + nameof(RenderTexture) + "s", holoplay);
                int width = holoplay.ScreenWidth;
                int height = holoplay.ScreenHeight;
                RenderTexture preview2DRT = holoplay.Preview2DRT;
                Camera singleViewCamera = holoplay.SingleViewCamera;
                Camera postProcessCamera = holoplay.PostProcessCamera;

                if (preview2DRT == null
                    || preview2DRT.width != width
                    || preview2DRT.height != height) {
                    preview2DRT = new RenderTexture(width, height, 24);
                }
                RenderTexture depth = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
                Profiler.EndSample();

                Profiler.BeginSample("Rendering", holoplay);
                try {
                    singleViewCamera.SetTargetBuffers(preview2DRT.colorBuffer, depth.depthBuffer);
                    singleViewCamera.Render();

#if UNITY_POST_PROCESSING_STACK_V2
                    bool hasPPCam = postProcessCamera != null;
                    if (hasPPCam) {
                        postProcessCamera.CopyFrom(singleViewCamera);
#if !UNITY_2018_1_OR_NEWER
                        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 ||
                            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12)
                            FlipRenderTexture(preview2DRT);
#endif
                        RunPostProcess(holoplay, preview2DRT, depth);
                    }
#endif
                } finally {
                    RenderTexture.ReleaseTemporary(depth);
                    Profiler.EndSample();
                }
                return preview2DRT;
            } finally {
                Profiler.EndSample();
            }
        }

        public static void FlipRenderTexture(RenderTexture texture) {
            RenderTexture rtTemp = RenderTexture.GetTemporary(texture.descriptor);
            rtTemp.Create();
            Graphics.CopyTexture(texture, rtTemp);
            Graphics.SetRenderTarget(texture);
            Rect rtRect = new Rect(0, 0, texture.width, texture.height);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, rtRect.width, 0, rtRect.height);
            Graphics.DrawTexture(rtRect, rtTemp);
            GL.PopMatrix();
            Graphics.SetRenderTarget(null);
            RenderTexture.ReleaseTemporary(rtTemp);
        }

        public static void FlipGenericRenderTexture(RenderTexture texture) {
            RenderTexture rtTemp = RenderTexture.GetTemporary(texture.descriptor);
            rtTemp.Create();
            Graphics.CopyTexture(texture, rtTemp);
            Graphics.SetRenderTarget(texture);
            Rect rtRect = new Rect(0, 0, texture.width, texture.height);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, texture.width, 0, texture.height);
            Graphics.DrawTexture(rtRect, rtTemp);
            GL.PopMatrix();
            Graphics.SetRenderTarget(null);
            RenderTexture.ReleaseTemporary(rtTemp);
        }

        public static void InterpolateViewsOnQuilt(Holoplay holoplay, RenderTexture quilt, RenderTexture quiltRTDepth) {
            if (interpolationComputeShader == null)
                interpolationComputeShader = Resources.Load<ComputeShader>("ViewInterpolation");
            Assert.IsNotNull(interpolationComputeShader);

            if (!interpolationProperties.Initialized)
                interpolationProperties.InitializeAll();

            Calibration cal = holoplay.Calibration;
            Camera singleViewCamera = holoplay.SingleViewCamera;
            HoloplayCameraData cameraData = holoplay.CameraData;
            Quilt.Settings quiltSettings = holoplay.QuiltSettings;
            HoloplayOptimizationData optimization = holoplay.Optimization;
            int viewInterpolation = optimization.GetViewInterpolation(quiltSettings.numViews);

            int kernelFwd = interpolationComputeShader.FindKernel("QuiltInterpolationForward");
            int kernelBack = optimization.BlendViews ?
                interpolationComputeShader.FindKernel("QuiltInterpolationBackBlend") :
                interpolationComputeShader.FindKernel("QuiltInterpolationBack");
            int kernelFwdFlicker = interpolationComputeShader.FindKernel("QuiltInterpolationForwardFlicker");
            int kernelBackFlicker = optimization.BlendViews ?
                interpolationComputeShader.FindKernel("QuiltInterpolationBackBlendFlicker") :
                interpolationComputeShader.FindKernel("QuiltInterpolationBackFlicker");

            interpolationComputeShader.SetTexture(kernelFwd, interpolationProperties.result, quilt);
            interpolationComputeShader.SetTexture(kernelFwd, interpolationProperties.resultDepth, quiltRTDepth);
            interpolationComputeShader.SetTexture(kernelBack, interpolationProperties.result, quilt);
            interpolationComputeShader.SetTexture(kernelBack, interpolationProperties.resultDepth, quiltRTDepth);
            interpolationComputeShader.SetTexture(kernelFwdFlicker, interpolationProperties.result, quilt);
            interpolationComputeShader.SetTexture(kernelFwdFlicker, interpolationProperties.resultDepth, quiltRTDepth);
            interpolationComputeShader.SetTexture(kernelBackFlicker, interpolationProperties.result, quilt);
            interpolationComputeShader.SetTexture(kernelBackFlicker, interpolationProperties.resultDepth, quiltRTDepth);
            interpolationComputeShader.SetFloat(interpolationProperties.nearClip, singleViewCamera.nearClipPlane);
            interpolationComputeShader.SetFloat(interpolationProperties.farClip, singleViewCamera.farClipPlane);
            interpolationComputeShader.SetFloat(interpolationProperties.focalDist, holoplay.GetCameraDistance());

            //Used for perspective w component:
            float aspectCorrectedFOV = Mathf.Atan(cal.GetAspect() * Mathf.Tan(0.5f * cameraData.FieldOfView * Mathf.Deg2Rad));
            interpolationComputeShader.SetFloat(interpolationProperties.perspw, 2 * Mathf.Tan(aspectCorrectedFOV));
            interpolationComputeShader.SetVector(interpolationProperties.viewSize, new Vector4(
                quiltSettings.ViewWidth,
                quiltSettings.ViewHeight,
                1f / quiltSettings.ViewWidth,
                1f / quiltSettings.ViewHeight
            ));

            List<int> viewPositions = new List<int>();
            List<float> viewOffsets = new List<float>();
            List<int> baseViewPositions = new List<int>();
            int validViewIndex = -1;
            int currentInterp = 1;
            for (int i = 0; i < quiltSettings.numViews; i++) {
                var positions = new[] {
                    i % quiltSettings.viewColumns * quiltSettings.ViewWidth,
                    i / quiltSettings.viewColumns * quiltSettings.ViewHeight,
                };
                if (i != 0 && i != quiltSettings.numViews - 1 && i % viewInterpolation != 0) {
                    viewPositions.AddRange(positions);
                    viewPositions.AddRange(new[] { validViewIndex, validViewIndex + 1 });
                    int div = Mathf.Min(viewInterpolation, quiltSettings.numViews - 1);
                    int divTotal = quiltSettings.numViews / div;
                    if (i > divTotal * viewInterpolation) {
                        div = quiltSettings.numViews - divTotal * viewInterpolation;
                    }
                    float viewCone = Application.isPlaying && cal.viewCone == 0 ? Calibration.DEFAULT_VIEWCONE : cal.viewCone;
                    float offset = div * Mathf.Tan(viewCone * cameraData.ViewconeModifier * Mathf.Deg2Rad) / (quiltSettings.numViews - 1f);
                    float lerp = (float) currentInterp / div;
                    currentInterp++;
                    viewOffsets.AddRange(new[] { offset, lerp });
                } else {
                    baseViewPositions.AddRange(positions);
                    validViewIndex++;
                    currentInterp = 1;
                }
            }

            int viewCount = viewPositions.Count / 4;
            ComputeBuffer viewPositionsBuffer = new ComputeBuffer(viewPositions.Count / 4, 4 * sizeof(int));
            ComputeBuffer viewOffsetsBuffer = new ComputeBuffer(viewOffsets.Count / 2, 2 * sizeof(float));
            ComputeBuffer baseViewPositionsBuffer = new ComputeBuffer(baseViewPositions.Count / 2, 2 * sizeof(int));
            viewPositionsBuffer.SetData(viewPositions);
            viewOffsetsBuffer.SetData(viewOffsets);
            baseViewPositionsBuffer.SetData(baseViewPositions);

            interpolationComputeShader.SetBuffer(kernelFwd, interpolationProperties.viewPositions, viewPositionsBuffer);
            interpolationComputeShader.SetBuffer(kernelFwd, interpolationProperties.viewOffsets, viewOffsetsBuffer);
            interpolationComputeShader.SetBuffer(kernelFwd, interpolationProperties.baseViewPositions, baseViewPositionsBuffer);
            interpolationComputeShader.SetBuffer(kernelBack, interpolationProperties.viewPositions, viewPositionsBuffer);
            interpolationComputeShader.SetBuffer(kernelBack, interpolationProperties.viewOffsets, viewOffsetsBuffer);
            interpolationComputeShader.SetBuffer(kernelBack, interpolationProperties.baseViewPositions, baseViewPositionsBuffer);
            interpolationComputeShader.SetBuffer(kernelFwdFlicker, interpolationProperties.viewPositions, viewPositionsBuffer);
            interpolationComputeShader.SetBuffer(kernelFwdFlicker, interpolationProperties.viewOffsets, viewOffsetsBuffer);
            interpolationComputeShader.SetBuffer(kernelFwdFlicker, interpolationProperties.baseViewPositions, baseViewPositionsBuffer);
            interpolationComputeShader.SetBuffer(kernelBackFlicker, interpolationProperties.viewPositions, viewPositionsBuffer);
            interpolationComputeShader.SetBuffer(kernelBackFlicker, interpolationProperties.viewOffsets, viewOffsetsBuffer);
            interpolationComputeShader.SetBuffer(kernelBackFlicker, interpolationProperties.baseViewPositions, baseViewPositionsBuffer);

            uint blockX, blockY, blockZ;
            interpolationComputeShader.GetKernelThreadGroupSizes(kernelFwd, out blockX, out blockY, out blockZ);
            int computeX = quiltSettings.ViewWidth / (int) blockX + Mathf.Min(quiltSettings.ViewWidth % (int) blockX, 1);
            int computeY = quiltSettings.ViewHeight / (int) blockY + Mathf.Min(quiltSettings.ViewHeight % (int) blockY, 1);
            int computeZ = viewCount / (int) blockZ + Mathf.Min(viewCount % (int) blockZ, 1);

            if (optimization.ReduceFlicker) {
                int spanSize = 2 * viewInterpolation;
                interpolationComputeShader.SetInt(interpolationProperties.spanSize, spanSize);
                for (int i = 0; i < spanSize; i++) {
                    interpolationComputeShader.SetInt(interpolationProperties.px, i);
                    interpolationComputeShader.Dispatch(kernelFwd, quiltSettings.ViewWidth / spanSize, computeY, computeZ);
                    interpolationComputeShader.Dispatch(kernelBack, quiltSettings.ViewWidth / spanSize, computeY, computeZ);
                }
            } else {
                interpolationComputeShader.Dispatch(kernelFwdFlicker, computeX, computeY, computeZ);
                interpolationComputeShader.Dispatch(kernelBackFlicker, computeX, computeY, computeZ);
            }

            if (optimization.FillGaps) {
                var fillgapsKernel = interpolationComputeShader.FindKernel("FillGaps");
                interpolationComputeShader.SetTexture(fillgapsKernel, interpolationProperties.result, quilt);
                interpolationComputeShader.SetTexture(fillgapsKernel, interpolationProperties.resultDepth, quiltRTDepth);
                interpolationComputeShader.SetBuffer(fillgapsKernel, interpolationProperties.viewPositions, viewPositionsBuffer);
                interpolationComputeShader.Dispatch(fillgapsKernel, computeX, computeY, computeZ);
            }

            viewPositionsBuffer.Dispose();
            viewOffsetsBuffer.Dispose();
            baseViewPositionsBuffer.Dispose();
        }
        public static void SetLightfieldMaterialSettings(Holoplay holoplay, Material lightfieldMaterial) {
            if (lightfieldMaterial == null)
                throw new ArgumentNullException(nameof(lightfieldMaterial));

            if (!lightfieldProperties.Initialized)
                lightfieldProperties.InitializeAll();

            Calibration cal = holoplay.Calibration;
            Quilt.Settings quiltSettings = holoplay.QuiltSettings;
            float aspect = holoplay.Aspect;
            lightfieldMaterial.SetFloat(lightfieldProperties.pitch, cal.pitch);

            lightfieldMaterial.SetFloat(lightfieldProperties.slope, cal.slope);
            lightfieldMaterial.SetFloat(lightfieldProperties.center, cal.center
#if UNITY_EDITOR
                + holoplay.CameraData.CenterOffset
#endif
            );
            lightfieldMaterial.SetFloat(lightfieldProperties.subpixelSize, cal.subp);
            lightfieldMaterial.SetVector(lightfieldProperties.tile, new Vector4(
                quiltSettings.viewColumns,
                quiltSettings.viewRows,
                quiltSettings.numViews,
                quiltSettings.viewColumns * quiltSettings.viewRows
            ));
            lightfieldMaterial.SetVector(lightfieldProperties.viewPortion, new Vector4(
                quiltSettings.ViewPortionHorizontal,
                quiltSettings.ViewPortionVertical
            ));

            lightfieldMaterial.SetVector(lightfieldProperties.aspect, new Vector4(
                aspect,
                aspect,
                quiltSettings.overscan ? 1 : 0
            ));

// #if UNITY_EDITOR_OSX && UNITY_2019_3_OR_NEWER
//             lightfieldMaterial.SetFloat(lightfieldProperties.verticalOffset, (float) -21 / cal.screenHeight);
// #elif UNITY_EDITOR_OSX && UNITY_2019_1_OR_NEWER
//             lightfieldMaterial.SetFloat(lightfieldProperties.verticalOffset, (float) -19 / cal.screenHeight);
// #endif
            //TODO: Setting this to non-zero values (above) messes with the center, where you see the seam between 2 views in your LKG device.
            //Perhaps we don't need this verticalOffset uniform property anymore in the Lightfield shader,
            //and we can remove this code?
            lightfieldMaterial.SetFloat(lightfieldProperties.verticalOffset, 0);
        }
    }
}
