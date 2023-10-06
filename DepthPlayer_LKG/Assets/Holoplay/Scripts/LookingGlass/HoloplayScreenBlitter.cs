//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace LookingGlass {
    /// <summary>
    /// <para>A <see cref="MonoBehaviour"/> component that blits quilts and 2D previews to the screen using <see cref="OnRenderImage(RenderTexture, RenderTexture)"/>.</para>
    /// <para>NOTE: This only works in the built-in render pipeline.</para>
    /// <para>See also: <seealso cref="RenderPipelineUtil.GetRenderPipelineType"/></para>
    /// </summary>
    [ExecuteAlways]
    public class HoloplayScreenBlitter : MonoBehaviour {
        public Holoplay holoplay;

        public event Action<RenderTexture> onAfterScreenBlit;

        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Assert.IsTrue(RenderPipelineUtil.IsBuiltIn, nameof(OnRenderImage) + " is assumed to only be called in the built-in render pipeline!");

            if (holoplay.Preview2D) {
                holoplay.RenderPreview2D();
                Graphics.Blit(holoplay.Preview2DRT, destination);
            } else {
                holoplay.RenderQuilt();
                Graphics.Blit(holoplay.QuiltTexture, destination, holoplay.LightfieldMaterial);
            }

            if (onAfterScreenBlit != null) {
                RenderTexture screenTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                try {
                    Graphics.Blit(destination, screenTexture);

                    //TODO: Understand if this is a good thing to do throughout this entire script or not..
                    //We don't want to deal with silly RenderTextures that are flipped in our C# code, but it might have sife-effects throughout the rendering/shaders...
                    //For now, we'll just do this for the onAfterScreenBlit event.
                    if (SystemInfo.graphicsUVStartsAtTop)
                        HoloplayRendering.FlipGenericRenderTexture(screenTexture);

                    onAfterScreenBlit(screenTexture);
				} finally {
                    RenderTexture.ReleaseTemporary(screenTexture);
				}
			}
        }
    }
}
