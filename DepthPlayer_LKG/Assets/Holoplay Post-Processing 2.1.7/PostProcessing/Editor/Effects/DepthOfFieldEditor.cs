using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEditor.Rendering.PostProcessing
{
    [PostProcessEditor(typeof(DepthOfField))]
    internal sealed class DepthOfFieldEditor : PostProcessEffectEditor<DepthOfField>
    {
        SerializedParameterOverride m_HoloplayDrivenFocus;
        SerializedParameterOverride m_HoloplayIntensity;
        SerializedParameterOverride m_FocusDistance;
        SerializedParameterOverride m_Aperture;
        SerializedParameterOverride m_FocalLength;
        SerializedParameterOverride m_KernelSize;

        public override void OnEnable()
        {
            m_HoloplayDrivenFocus = FindParameterOverride(x => x.holoplayDrivenFocus);
            m_HoloplayIntensity = FindParameterOverride(x => x.holoplayIntensity);
            m_FocusDistance = FindParameterOverride(x => x.focusDistance);
            m_Aperture = FindParameterOverride(x => x.aperture);
            m_FocalLength = FindParameterOverride(x => x.focalLength);
            m_KernelSize = FindParameterOverride(x => x.kernelSize);
        }

        public override void OnInspectorGUI()
        {
            if (SystemInfo.graphicsShaderLevel < 35)
                EditorGUILayout.HelpBox("Depth Of Field is only supported on the following platforms:\nDX11+, OpenGL 3.2+, OpenGL ES 3+, Metal, Vulkan, PS4/XB1 consoles.", MessageType.Warning);

            PropertyField(m_HoloplayDrivenFocus);
            if (!m_HoloplayDrivenFocus.overrideState.boolValue || !m_HoloplayDrivenFocus.value.boolValue) {
                GUI.enabled = false;
            }
            PropertyField(m_HoloplayIntensity);
            GUI.enabled = true;
            if (m_HoloplayDrivenFocus.overrideState.boolValue && m_HoloplayDrivenFocus.value.boolValue) {
                GUI.enabled = false;
            }
            PropertyField(m_FocusDistance);
            PropertyField(m_Aperture);
            PropertyField(m_FocalLength);
            GUI.enabled = true;
            PropertyField(m_KernelSize);
        }
    }
}
