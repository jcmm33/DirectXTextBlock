using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SharpDX;
using SharpDX.DirectWrite;
using Color = SharpDX.Color;
using TextAlignment = Windows.UI.Xaml.TextAlignment;
using FontStyle = Windows.UI.Text.FontStyle;

namespace DirectXTextBlockControl
{
    public class DirectXTextBlock : DirectXSurfaceControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public double BlurRadius
        {
            get { return (double)GetValue(BlurRadiusProperty); }
            set { SetValue(BlurRadiusProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public Brush Shadow
        {
            get { return (Brush)GetValue(ShadowProperty); }
            set { SetValue(ShadowProperty, value); }
        }

        public double ShadowOffset
        {
            get { return (double)GetValue(ShadowOffsetProperty); }
            set { SetValue(ShadowOffsetProperty, value); }
        }


        private readonly TextRenderer _effect = new TextRenderer();

        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty TextAlignmentProperty;
        public static readonly DependencyProperty ShadowProperty;
        public static readonly DependencyProperty TextWrappingProperty;
        public static readonly DependencyProperty ShadowOffsetProperty;
        public static readonly DependencyProperty BlurRadiusProperty;

        static DirectXTextBlock()
        {
            var tb = new TextBlock();

            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DirectXTextBlock), new PropertyMetadata("", OnTextChangedCallback));
            TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(DirectXTextBlock), new PropertyMetadata(tb.TextWrapping, TextWrappingPropertyChanged));
            TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(DirectXTextBlock), new PropertyMetadata(tb.TextAlignment, TextAlignmentPropertyChanged));
            ShadowProperty = DependencyProperty.Register("Shadow", typeof(Brush), typeof(DirectXTextBlock), new PropertyMetadata(new SolidColorBrush(Colors.Black), ShadowPropertyChanged));
            ShadowOffsetProperty = DependencyProperty.Register("ShadowOffset", typeof(double), typeof(DirectXTextBlock), new PropertyMetadata(3.0, ShadowOffsetPropertyChanged));
            BlurRadiusProperty = DependencyProperty.Register("BlurRadius", typeof(double), typeof(DirectXTextBlock), new PropertyMetadata(3.0, BlurRadiusPropertyChanged));
        }

        private static void BlurRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DirectXTextBlock)d).BlurRadiusChanged((double)e.NewValue);
        }

        private void BlurRadiusChanged(double newValue)
        {
            SetBlurRadius(newValue);
            QueueRender(true);
        }

        private static void ShadowOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DirectXTextBlock)d).ShadowOffsetChanged((double)e.NewValue);
        }

        private static void ShadowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DirectXTextBlock)d).ShadowChanged((Brush)e.NewValue);
        }

        private static void TextAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DirectXTextBlock)d).TextAlignmentChanged((Windows.UI.Xaml.TextAlignment)e.NewValue);
        }

        private static void OnTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DirectXTextBlock)d).TextChanged((string)e.NewValue);
        }

        private static void TextWrappingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DirectXTextBlock)d).TextWrappingChanged((Windows.UI.Xaml.TextWrapping)e.NewValue);
        }

        private void ShadowOffsetChanged(double newValue)
        {
            SetShadowOffset(newValue);
            QueueRender(true);
        }

        private void SetShadowOffset(double newValue)
        {
            _effect.ShadowOffset = newValue;
        }

        private void TextAlignmentChanged(TextAlignment newValue)
        {
            SetTextAlignment(newValue);

            QueueRender(true);
        }

        private void SetTextAlignment(TextAlignment newValue)
        {
            switch (newValue)
            {
                case TextAlignment.Center:
                    _effect.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                    break;

                case TextAlignment.Justify:
                    _effect.TextAlignment = SharpDX.DirectWrite.TextAlignment.Justified;
                    break;


                case TextAlignment.Left:
                    _effect.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
                    break;

                case TextAlignment.Right:
                    _effect.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing;
                    break;
            }
        }

        private void SetBlurRadius(double newValue)
        {
            // nothing yet !
        }

        private void ShadowChanged(Brush newValue)
        {
            SetShadow(newValue);

            QueueRender();
        }

        private void SetShadow(Brush newValue)
        {
            _effect.Shadow = ConvertBrushToColor(newValue);
        }

        private void TextWrappingChanged(TextWrapping newValue)
        {
            SetTextWrapping(newValue);

            QueueRender(true);
        }

        private void SetTextWrapping(TextWrapping newValue)
        {
            switch (newValue)
            {
                case TextWrapping.NoWrap:
                    _effect.WordWrapping = WordWrapping.NoWrap;
                    break;

                case TextWrapping.Wrap:
                    _effect.WordWrapping = WordWrapping.EmergencyBreak;
                    break;

                case TextWrapping.WrapWholeWords:
                    _effect.WordWrapping = WordWrapping.EmergencyBreak;
                    break;
            }
        }


        private void TextChanged(string newValue)
        {
            (_effect as TextRenderer).Text = newValue;
            QueueRender(true);
        }

        private void QueueRender(Boolean forceMeasure = false)
        {
            _renderRequired = true;

            if (forceMeasure)
            {
                InvalidateMeasure();
            }
        }

        private Image _renderedImage;

        private PropertyChangeNotifier _fontFamilyChangeNotifier;
        private PropertyChangeNotifier _fontSizeChangeNotifier;
        private PropertyChangeNotifier _foregroundChangeNotifier;
        private PropertyChangeNotifier _fontStyleChangeNotifier;
        private PropertyChangeNotifier _fontWeightChangeNotifier;

        public DirectXTextBlock()
        {
            _renderedImage = new Image();
            DefaultStyleKey = typeof(DirectXTextBlock);

            SizeChanged += DirectXTextBlock_SizeChanged;

            _fontFamilyChangeNotifier = new PropertyChangeNotifier(this, "FontFamily");
            _fontFamilyChangeNotifier.ValueChanged += _fontFamilyChangeNotifier_ValueChanged;

            _fontSizeChangeNotifier = new PropertyChangeNotifier(this, "FontSize");
            _fontSizeChangeNotifier.ValueChanged += _fontSizeChangeNotifier_ValueChanged;

            _fontStyleChangeNotifier = new PropertyChangeNotifier(this, "FontStyle");
            _fontStyleChangeNotifier.ValueChanged += _fontStyleChangeNotifier_ValueChanged;

            _fontWeightChangeNotifier = new PropertyChangeNotifier(this, "FontWeight");
            _fontWeightChangeNotifier.ValueChanged += _fontWeightChangeNotifier_ValueChanged;

            _foregroundChangeNotifier = new PropertyChangeNotifier(this, "Foreground");
            _foregroundChangeNotifier.ValueChanged += _foregroundChangeNotifier_ValueChanged;

            Direct2DRenderer = _effect;
        }

        void DirectXTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (e.NewSize.Width != e.PreviousSize.Width ||
                e.NewSize.Height != e.PreviousSize.Height)
            {
                QueueRender(true);
            }
        }

        void _fontFamilyChangeNotifier_ValueChanged(object sender, EventArgs e)
        {
            SetFontFamily();

            QueueRender(true);
        }

        private void SetFontFamily()
        {
            this._effect.FontFamily = this.FontFamily.Source;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _renderedImage = GetTemplateChild("DirectXRenderImage") as Image;

            _effect.FontFamily = this.FontFamily.Source;
            _effect.FontSize = this.FontSize;


            SetForeground();
            SetFontWeight();
            SetFontStyle();
            SetForeground();
            SetFontSize();
            SetFontFamily();
            SetTextAlignment(TextAlignment);
            SetTextWrapping(TextWrapping);
            SetShadow(Shadow);
            SetShadowOffset(ShadowOffset);
            SetBlurRadius(BlurRadius);
        }

        void _fontWeightChangeNotifier_ValueChanged(object sender, EventArgs e)
        {
            SetFontWeight();
            QueueRender(true);
        }

        void _fontStyleChangeNotifier_ValueChanged(object sender, EventArgs e)
        {
            SetFontStyle();

            QueueRender(true);
        }

        private void SetFontStyle()
        {
            switch (this.FontStyle)
            {
                case FontStyle.Normal:
                    _effect.FontStyle = SharpDX.DirectWrite.FontStyle.Normal;
                    break;

                case FontStyle.Italic:
                    _effect.FontStyle = SharpDX.DirectWrite.FontStyle.Italic;
                    break;

                case FontStyle.Oblique:
                    _effect.FontStyle = SharpDX.DirectWrite.FontStyle.Oblique;
                    break;
            }
        }

        void _foregroundChangeNotifier_ValueChanged(object sender, EventArgs e)
        {
            SetForeground();
            QueueRender();
        }


        private void SetForeground()
        {
            var scb = this.Foreground as SolidColorBrush;

            _effect.Foreground = scb != null ? ConvertBrushToColor(scb) : Color.White;
        }

        private void SetFontWeight()
        {
            _effect.FontWeight = (FontWeight)this.FontWeight.Weight;
        }

        private Color4 ConvertBrushToColor(Brush brush)
        {
            var scb = brush as SolidColorBrush;

            return scb != null
                ? new Color4(scb.Color.R / 255.0f, scb.Color.G / 255.0f, scb.Color.B / 255.0f, scb.Color.A / 255.0f)
                : Color4.White;
        }

        void _fontSizeChangeNotifier_ValueChanged(object sender, EventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            SetFontSize();

            QueueRender(true);
        }

        private void SetFontSize()
        {
            _effect.FontSize = this.FontSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var fullPixelBlurIncrement = Context.ConvertDipsToPixels(BlurRadius * 6.0);

            var extent = Context.ConvertPixelsToDips(fullPixelBlurIncrement / 2) + this.ShadowOffset;

            // now, then, we need to adjust the available size by this amount

            var rect = _effect.Measure(Context, availableSize.Width - extent, availableSize.Height - extent);

            // now then, lets also consider the shadow we are going to apply
            // what should be do exactly ?
            // well, we know that we have a shadow offset
            // which is going to go in both the vertical and manual size...
            // so perhaps we need to do a 'measure' using the shadow...

            return new Size(rect.Width, rect.Height);
        }

        private Boolean _renderRequired = true;


        protected override bool ShouldRender()
        {
            return _renderRequired;
        }

        protected override void SurfaceCreated(XamlSurfaceRenderer surfaceRenderer, Size dipsSize)
        {
            _renderedImage.Source = surfaceRenderer;
            _renderedImage.Width = dipsSize.Width;
            _renderedImage.Height = dipsSize.Height;
        }

        protected override Size GetSurfaceDimensions()
        {
            return new Size(Context.ConvertDipsToPixels(this.ActualWidth),
                Context.ConvertDipsToPixels(this.ActualHeight));
        }

        protected override void RenderCompleted()
        {
            _renderRequired = false;
        }

    }

}
