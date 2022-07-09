using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Depravity
{

    [Serializable]
    [PostProcess(typeof(DeathEffectRenderer), PostProcessEvent.BeforeStack, "Depravity/Death")]
    public sealed class DeathPPEffect : PostProcessEffectSettings
    {
        [Tooltip("Seconds to complete effect.")]
        public FloatParameter speed = new FloatParameter { value = 1.0f };
    }

    public sealed class DeathEffectRenderer : PostProcessEffectRenderer<DeathPPEffect>
    {
        private Shader shader;
        private float startTime, aspectRatio;
        private int aspectRatioId, progressId;

        public override void Init()
        {
            shader = Shader.Find("Depravity/Death");
            aspectRatioId = Shader.PropertyToID("_AspectRatio");
            progressId = Shader.PropertyToID("_Progress");
            ResetHistory();
        }

        public override void ResetHistory()
        {
            startTime = Time.unscaledTime;
            aspectRatio = Screen.width / ((float)Screen.height);
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            var properties = sheet.properties;
            properties.SetFloat(aspectRatioId, aspectRatio);
            float prog = (Time.unscaledTime - startTime) / settings.speed;
            prog *= prog;
            properties.SetFloat(progressId, Mathf.Clamp01(prog * prog));
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}