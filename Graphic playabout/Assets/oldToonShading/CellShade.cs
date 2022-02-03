using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(CellShadeRenderer), PostProcessEvent.AfterStack, "Custom/CellShade")]
public sealed class CellShade : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("CellShade effect intensity.")]
    public FloatParameter blackCuttof = new FloatParameter { value = 0.05f };
    public FloatParameter whiteCuttof = new FloatParameter { value = 0.95f };
    public FloatParameter minRange = new FloatParameter { value = 0.65f };
    public FloatParameter lowLuminanceValue = new FloatParameter { value = 0.4f };
    public FloatParameter highLuminanceValue = new FloatParameter { value = 0.75f };
    public FloatParameter blendRange = new FloatParameter { value = 0.05f };
}

public sealed class CellShadeRenderer : PostProcessEffectRenderer<CellShade>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/CellShade"));
        sheet.properties.SetFloat("_BlackCuttof", settings.blackCuttof);
        sheet.properties.SetFloat("_WhiteCuttof", settings.whiteCuttof);
        sheet.properties.SetFloat("_HighLuminanceVal", settings.highLuminanceValue);
        sheet.properties.SetFloat("_LowLuminanceVal", settings.lowLuminanceValue);
        sheet.properties.SetFloat("_MinRange", settings.minRange);
        sheet.properties.SetFloat("_BlendRange", settings.blendRange);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}