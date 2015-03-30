using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Point = SharpDX.Point;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace DirectXTextBlockControl
{
    public class XamlSurfaceRenderer : SurfaceImageSource
    {
        private readonly IDirect2DRenderer _renderer;
        private Size2 _pixelSize;

        public XamlSurfaceRenderer(GraphicsDeviceContext context, IDirect2DRenderer renderer, Size2 pixelSize,
            Boolean isOpaque)
            : base(pixelSize.Width, pixelSize.Height, isOpaque)
        {
            Context = context;
            _renderer = renderer;
            _pixelSize = pixelSize;

            CreateDeviceResources();
        }

        public IDirect2DRenderer Renderer
        {
            get { return _renderer; }
        }

        public Size2 SurfaceSize
        {
            get { return _pixelSize; }
            set { _pixelSize = value; }
        }

        public GraphicsDeviceContext Context { get; private set; }

        private void CreateDeviceResources()
        {
            using (var dxgiDevice = Context.D3DDevice.QueryInterface<SharpDX.DXGI.Device>())
            {
                // Query for ISurfaceImageSourceNative interface.
                using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
                {
                    sisNative.Device = dxgiDevice;
                }
            }

            _targetProperties = new BitmapProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied), (float)Context.LogicalDpi, (float)Context.LogicalDpi);
        }

        private void FreeDeviceResources()
        {

        }

        public void Draw()
        {
            Draw(new Rectangle(0, 0, _pixelSize.Width, _pixelSize.Height));
        }

        private BitmapProperties _targetProperties;

        public void Draw(Rectangle updateRect)
        {
            //_renderer.BeginDraw(Context, updateRect);

            // 

            //var updateRectNative = new Rectangle
            //{
            //    Left = Context.ConvertDipsToPixels(updateRect.Left),
            //    Top = Context.ConvertDipsToPixels(updateRect.Top),
            //    Right = Context.ConvertDipsToPixels(updateRect.Right),
            //    Bottom = Context.ConvertDipsToPixels(updateRect.Bottom)
            //};

            var updateRectNative = new Rectangle
            {
                Left = 0,
                Top = 0,
                Right = _pixelSize.Width,
                Bottom = _pixelSize.Height
            };

            // Query for ISurfaceImageSourceNative interface.
            using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
            {
                while (true)
                {
                    // Begin drawing - returns a target surface and an offset to use as the top left origin when drawing.
                    try
                    {
                        Point pixelOffset;

                        using (var surface = sisNative.BeginDraw(updateRectNative, out pixelOffset))
                        {
                            using (var bitmap = new Bitmap(Context.D2DContext, surface, _targetProperties))
                            {
                                // Set context's render target.
                                Context.D2DContext.Target = bitmap;
                            }

                            // Apply a clip and transform to constrain updates to the target update area.
                            // This is required to ensure coordinates within the target surface remain
                            // consistent by taking into account the offset returned by BeginDraw, and
                            // can also improve performance by optimizing the area that is drawn by D2D.
                            // Apps should always account for the offset output parameter returned by 
                            // BeginDraw, since it may not match the passed updateRect input parameter's location.

                            //_gdc.D2DContext.PushAxisAlignedClip(
                            //    new RectangleF(
                            //        (offset.X),
                            //        (offset.Y),
                            //        (offset.X + (float)updateRect.Width),
                            //        (offset.Y + (float)updateRect.Height)
                            //        ),
                            //    AntialiasMode.Aliased
                            //    

                            //_renderer.BeginDraw(Context, updateRect);

                            // 23/03/2015 - need to adjust the pixel offset to Dips

                            Point dipsOffset = new Point(Context.ConvertPixelsToDips(pixelOffset.X),Context.ConvertPixelsToDips(pixelOffset.Y));

                            var renderContext = new RenderContext(Context, dipsOffset, updateRect);

                            _renderer.DrawAsync(renderContext);

                            //_renderer.EndDraw(Context);

                            Context.D2DContext.Target = null;

                            sisNative.EndDraw();
                        }

                        return;
                    }
                    catch
                        (SharpDXException ex)
                    {
                        if (ex.ResultCode == ResultCode.DeviceRemoved ||
                            ex.ResultCode == ResultCode.DeviceReset)
                        {
                            // reset the gdc
                            Context.Initialize();

                            // reset the renderer
                            _renderer.Reset(Context);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private int ConvertPixelsToDips(float pixels, float dpi)
        {
            const float dipsPerInch = 96.0f;
            return (int)(pixels * dipsPerInch / dpi);
        }


        protected virtual void BeginDraw()
        {
        }

        protected virtual void EndDraw()
        {
        }
    }
}
