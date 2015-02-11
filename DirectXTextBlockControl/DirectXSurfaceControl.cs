using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Point = SharpDX.Point;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace DirectXTextBlockControl
{
    public class DirectXSurfaceControl : ContentControl
    {
        protected readonly GraphicsDeviceContext Context;

        private Boolean _isLoaded = false;

        public DirectXSurfaceControl()
        {
            Context = new GraphicsDeviceContext();

            CompositionTarget.SurfaceContentsLost += CompositionTarget_SurfaceContentsLost;

            this.Loaded += ControlLoaded;
            this.Unloaded += ControlUnloaded;

            this.LayoutUpdated += DirectXSurfaceControl_LayoutUpdated;

            // is this a good place for this ?
            if (DesignMode.DesignModeEnabled) return;

            var displayInformation = Context.DisplayInformation;
            displayInformation.DpiChanged += displayInformation_DpiChanged;


        }

        void DirectXSurfaceControl_LayoutUpdated(object sender, object e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
            }
        }

        private EventHandler<object> _renderingCallback;

        void ControlUnloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= _renderingCallback;
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            _renderingCallback = new EventHandler<object>(this.CompositionTarget_Rendering);

            CompositionTarget.Rendering += _renderingCallback;
        }

        private Boolean _isRendering = false;


        public Boolean IsLoaded
        {
            get { return _isLoaded; }
        }

        private XamlSurfaceRenderer _surfaceRenderer;

        public IDirect2DRenderer Direct2DRenderer
        {
            get { return (IDirect2DRenderer)GetValue(Direct2DRendererProperty); }
            set { SetValue(Direct2DRendererProperty, value); }
        }

        public static readonly DependencyProperty Direct2DRendererProperty =
            DependencyProperty.Register("Direct2DRenderer", typeof(IDirect2DRenderer), typeof(DirectXSurfaceControl), new PropertyMetadata(null, Direct2DRenderedPropertyChanged));

        private static void Direct2DRenderedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DirectXSurfaceControl)d).Direct2DRendererChanged((IDirect2DRenderer)e.NewValue);
        }

        private void Direct2DRendererChanged(IDirect2DRenderer newValue)
        {
            // the renderer has changed, we need to indicate that the surface needs to change
            _surfaceRenderer = null;
        }


        async void CompositionTarget_Rendering(object sender, object e)
        {
            if (!ShouldRender())
            {
                return;
            }

            if (_isRendering) return;

            _isRendering = true;

            // now then, this is our rendering opportunity
            await Render();

            _isRendering = false;
        }


        protected virtual void CreateRenderSurface()
        {
            // we need to adjust the width and height to allow for the effect 
            // could well adjust based upon the width and height...

            var effectLogicalDimensions = GetSurfaceDimensions();

            var pixelDimensions = new Size2(Context.ConvertDipsToPixels(effectLogicalDimensions.Width), Context.ConvertDipsToPixels(effectLogicalDimensions.Height));

            if (_surfaceRenderer != null)
            {
                var existingSize = _surfaceRenderer.SurfaceSize;

                if (existingSize.Width == pixelDimensions.Width && existingSize.Height == pixelDimensions.Height)
                {
                    return;
                }
            }

            _surfaceRenderer = new XamlSurfaceRenderer(Context, Direct2DRenderer, pixelDimensions, false);

            SurfaceCreated(_surfaceRenderer, effectLogicalDimensions);
        }

        protected virtual void SurfaceCreated(XamlSurfaceRenderer surfaceRenderer, Windows.Foundation.Size dipsSize)
        {
            throw new NotImplementedException("Surface Creation must be implemented.");
        }

        private async Task Render()
        {
            CreateRenderSurface();

            if (_surfaceRenderer != null && Direct2DRenderer != null)
            {
                await PrepareForRender();

                await _surfaceRenderer.Draw();

                RenderCompleted();
            }
        }

        protected virtual void RenderCompleted()
        {
            return;
        }

        protected async virtual Task PrepareForRender()
        {
            return;
        }

        protected virtual bool ShouldRender()
        {
            return false;
        }


        protected virtual Windows.Foundation.Size GetSurfaceDimensions()
        {
            return new Windows.Foundation.Size();
        }

        private void CompositionTarget_SurfaceContentsLost(object sender, object e)
        {
        }

        void displayInformation_DpiChanged(DisplayInformation sender, object args)
        {
            Context.SetDpi(new Windows.Foundation.Size(sender.LogicalDpi, sender.LogicalDpi));
        }
    }
}
