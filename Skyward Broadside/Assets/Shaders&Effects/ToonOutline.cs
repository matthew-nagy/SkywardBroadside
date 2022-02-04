using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ToonOutlineRenderer), PostProcessEvent.AfterStack, "Custom/ToonOutline")]
public sealed class ToonOutline : PostProcessEffectSettings
{
    public IntParameter scale = new IntParameter { value = 2 };
    public FloatParameter edgeScaler = new FloatParameter { value = 800.0f };
    public FloatParameter depthDrawThreshold = new FloatParameter { value = 0.03f };
}

public sealed class ToonOutlineRenderer : PostProcessEffectRenderer<ToonOutline>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/ToonOutline"));
        sheet.properties.SetFloat("_Scale", settings.scale);
        sheet.properties.SetFloat("_EdgeScaler", settings.scale);
        sheet.properties.SetFloat("_DepthThreshold", settings.depthDrawThreshold);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}