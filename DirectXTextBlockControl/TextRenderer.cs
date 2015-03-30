using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace DirectXTextBlockControl
{
    public class TextRenderer : IDisposable, IDirect2DRenderer
    {
        /// <summary>
        /// Get or set the text alignment.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return _textAlignment; }
            set
            {
                _textAlignment = value;
                InvalidateRender();
            }
        }

        /// <summary>
        ///     Get or set how word wrapping performed
        /// </summary>
        public WordWrapping WordWrapping
        {
            get { return _wordWrapping; }
            set
            {
                _wordWrapping = value;
                InvalidateRender();
            }
        }


        /// <summary>
        /// Get or set the shadow offset
        /// </summary>
        public double ShadowOffset
        {
            get { return _shadowOffset; }
            set
            {
                _shadowOffset = value;
                // do nothing else, re-render would be invoked elsewhere                
            }
        }

        /// <summary>
        /// Get or set the text
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                InvalidateRender();
            }
        }

        /// <summary>
        /// Get or set the font family
        /// </summary>
        public string FontFamily
        {
            get { return _fontFamily; }
            set
            {
                _fontFamily = value;
                InvalidateRender();
            }
        }

        /// <summary>
        /// Get or set the font size
        /// </summary>
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                InvalidateRender();
            }
        }

        /// <summary>
        /// Get or set the font weight
        /// </summary>
        public FontWeight FontWeight
        {
            get { return _fontWeight; }
            set
            {
                _fontWeight = value;
                InvalidateRender();
            }
        }

        /// <summary>
        /// Get or set the text foreground
        /// </summary>
        public Color4 Foreground
        {
            get { return _foreground; }
            set
            {
                InvalidateForegroundSceneBrush();
                _foreground = value;
            }
        }

        /// <summary>
        /// Get or set the shadow colour
        /// </summary>
        public Color4 Shadow
        {
            get { return _shadow; }
            set
            {
                _shadow = value;
            }
        }

        /// <summary>
        /// Get or set the font style
        /// </summary>
        public FontStyle FontStyle
        {
            get { return _fontStyle; }
            set
            {
                _fontStyle = value;
                InvalidateRender();
            }
        }


        /// <summary>
        /// our cached components
        /// </summary>
        //private TextFormat _cachedTextFormat;
        //private Brush _cachedSceneColorBrush;
        //private TextLayout _cachedTextLayout;
        //private Bitmap _cachedRenderedBitmap;


        private Rect _logicalRect;


        private TextAlignment _textAlignment = TextAlignment.Leading;
        private WordWrapping _wordWrapping;

        private string _fontFamily = "Portfolio User Interface";
        private double _fontSize = 10.0;
        private Color4 _foreground;
        private string _text;

        private const FontStretch FontStretch = SharpDX.DirectWrite.FontStretch.Normal;
        private FontStyle _fontStyle = FontStyle.Normal;
        private FontWeight _fontWeight = FontWeight.Normal;
        private double _shadowOffset = 3.0;
        private Color4 _shadow = Color4.Black;

        /// <summary>
        /// Perform a draw
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public void DrawAsync(RenderContext context)
        {
            //var logicalDipsRect = new RectangleF((float)_logicalRect.Left,
            //    (float)_logicalRect.Top,
            //    (float)_logicalRect.Width,
            //    (float)_logicalRect.Height);

            var newRect = context.LogicalDimensions;

            if (_logicalRect.X != newRect.X ||
                _logicalRect.Y != newRect.Y ||
                _logicalRect.Width != newRect.Width ||
                _logicalRect.Height != newRect.Height)
            {
                InvalidateRender();
            }

            _logicalRect = newRect;

            using (var renderedTextBitmap = GetRender(context.DeviceContext))
            {
                context.BeginDraw();

                using (var composite = new TextShadowEffect(this, renderedTextBitmap)
                {
                    ShadowOffset = (float) _shadowOffset,
                    ShadowColor = Shadow
                })
                {
                    composite.Render(context.DeviceContext);
                }

                context.EndDraw();
            }
        }

        public void Reset(GraphicsDeviceContext gdc)
        {

        }


        /// <summary>
        /// Get a render of the current text as a bitmap
        /// </summary>
        /// <param name="gdc"></param>
        /// <returns></returns>
        /// <Remarks>The rendered bitmap is cached</Remarks>
        private Bitmap GetRender(GraphicsDeviceContext gdc)
        {
            var renderedBitmap = new Bitmap1(gdc.D2DContext, new Size2((int)gdc.ConvertDipsToPixels(_logicalRect.Width), (int)gdc.ConvertDipsToPixels(_logicalRect.Height)),
                new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), (float)gdc.LogicalDpi, (float)gdc.LogicalDpi, BitmapOptions.Target));

            var oldTarget = gdc.D2DContext.Target;

            gdc.D2DContext.Target = renderedBitmap;

            gdc.D2DContext.BeginDraw();

            gdc.D2DContext.Clear(null);

            gdc.D2DContext.DrawTextLayout(new Vector2((float)_logicalRect.Left, (float)_logicalRect.Top), GetTextLayout(gdc), new SolidColorBrush(gdc.D2DContext, Color4.Black));

            gdc.D2DContext.EndDraw();

            gdc.D2DContext.Target = oldTarget;

            return renderedBitmap;
        }

        /// <summary>
        /// Invalidate the current rendering, this results in cached bitmaps, text formatting and layouts are cleared.
        /// </summary>
        private void InvalidateRender()
        {
        }

        /// <summary>
        /// Get a brush to be used for the text rendering.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Brush GetForegroundSceneBrush(GraphicsDeviceContext context)
        {
            return new SolidColorBrush(context.D2DContext, _foreground);
        }

        /// <summary>
        /// Clear down any cached scene brushes
        /// </summary>
        private void InvalidateForegroundSceneBrush()
        {
        }

        /// <summary>
        /// Get a text layout
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public TextLayout GetTextLayout(GraphicsDeviceContext context)
        {
            using (var format = GetTextFormat(context))
            {
                return new TextLayout(context.FactoryDirectWrite, Text ?? "", format, (float) _logicalRect.Right,
                    (float) _logicalRect.Bottom);
            }
        }

        /// <summary>
        /// Get a text format
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public TextFormat GetTextFormat(GraphicsDeviceContext context)
        {
            return new TextFormat(context.FactoryDirectWrite, _fontFamily, _fontWeight, _fontStyle, FontStretch,
                           (float)_fontSize)
                       {
                           TextAlignment = this._textAlignment,
                           ParagraphAlignment = ParagraphAlignment.Near,
                           WordWrapping = this._wordWrapping,

                       }
            ;
        }


        /// <summary>
        /// Get the dimensions of a rectangle required to contain the current text.
        /// </summary>
        /// <param name="gdc"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public Rect Measure(GraphicsDeviceContext gdc, double maxWidth, double maxHeight)
        {
            using (var format = GetTextFormat(gdc))
            {
                var tt = new TextLayout(gdc.FactoryDirectWrite, Text ?? "", format, (float) maxWidth, (float) maxHeight);

                var metrics = tt.Metrics;

                return new Rect(0, 0, metrics.Width, metrics.Height);
            }
        }

        protected Boolean IsDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                InvalidateRender();
                InvalidateForegroundSceneBrush();
            }

            IsDisposed = true;
        }

        ~TextRenderer()
        {
            Dispose(false);
        }
    }
}
