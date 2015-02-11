using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;

namespace DirectXTextBlockControl
{
    public class TextShadowEffect : DirectXEffect
    {
        private Composite _compositeEffect;
        private Shadow _shadowEffect;
        private AffineTransform2D _affineTransformEffect;

        public Color4 ShadowColor { get; set; }

        public float ShadowOffset { get; set; }

        private readonly TextRenderer _textRenderer;
        private Image _inputImage;

        public TextShadowEffect(TextRenderer textRenderer, Image inputImage)
        {
            _textRenderer = textRenderer;
            _inputImage = inputImage;
        }

        private void BuildEffects(GraphicsDeviceContext context)
        {
            _shadowEffect = new Shadow(context.D2DContext);
            _affineTransformEffect = new AffineTransform2D(context.D2DContext);

            _shadowEffect.Color = ShadowColor;
            _shadowEffect.BlurStandardDeviation = 3.0f;

            _affineTransformEffect.TransformMatrix = Matrix3x2.Translation(ShadowOffset, ShadowOffset);

            _shadowEffect.SetInput(0, _inputImage, false);

            _affineTransformEffect.SetInputEffect(0, _shadowEffect);
        }

        public void Render(GraphicsDeviceContext context)
        {
            BuildEffects(context);

            context.D2DContext.DrawImage(_affineTransformEffect);

            context.D2DContext.DrawTextLayout(new Vector2(0.0f, 0.0f), _textRenderer.GetTextLayout(context), _textRenderer.GetForegroundSceneBrush(context));
        }

        protected override void Dispose(bool disposing)
        {
            if (_shadowEffect != null)
            {
                _shadowEffect.Dispose();
            }

            if (_compositeEffect != null)
            {
                _compositeEffect.Dispose();
            }

            if (_affineTransformEffect != null)
            {
                _affineTransformEffect.Dispose();
            }

            _inputImage = null;
            _shadowEffect = null;
            _compositeEffect = null;
            _affineTransformEffect = null;

            base.Dispose(disposing);
        }
    }
}
