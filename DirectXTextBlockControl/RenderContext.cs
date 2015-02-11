using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using SharpDX;
using Point = SharpDX.Point;

namespace DirectXTextBlockControl
{
    public class RenderContext
    {
        private readonly GraphicsDeviceContext _deviceContext;
        private readonly Point _pixelOffset;
        private readonly Rectangle _pixelUpdateRect;

        public RenderContext(GraphicsDeviceContext deviceContext, Point pixelOffset, Rectangle pixelUpdateRect)
        {
            _deviceContext = deviceContext;
            _pixelOffset = pixelOffset;
            _pixelUpdateRect = pixelUpdateRect;
        }

        public GraphicsDeviceContext DeviceContext
        {
            get { return _deviceContext; }
        }


        private Boolean _offsetSet = true;

        public void SetupOffset()
        {

            DeviceContext.D2DContext.Transform = Matrix3x2.Translation(_pixelOffset.X, _pixelOffset.Y);
            _offsetSet = true;
        }

        public void ResetOffset()
        {
            if (_offsetSet)
            {
                DeviceContext.D2DContext.Transform = Matrix3x2.Identity;
                _offsetSet = false;
            }
        }

        private Boolean _clipSet = false;
        public void SetupClip()
        {
            var clipRect = new RectangleF(_pixelOffset.X, _pixelOffset.Y, _pixelUpdateRect.Width, _pixelUpdateRect.Height);


            //            DeviceContext.D2DContext.PushAxisAlignedClip(clipRect,AntialiasMode.Aliased);

            _clipSet = true;
        }

        public void ClearClip()
        {
            if (_clipSet)
            {
                //DeviceContext.D2DContext.PopAxisAlignedClip();
                _clipSet = false;
            }
        }

        public Rect LogicalDimensions
        {
            get
            {
                return new Rect(_deviceContext.ConvertPixelsToDips(_pixelUpdateRect.Left), _deviceContext.ConvertPixelsToDips(_pixelUpdateRect.Y),
                                _deviceContext.ConvertPixelsToDips(_pixelUpdateRect.Width), _deviceContext.ConvertPixelsToDips(_pixelUpdateRect.Height));
            }
        }


        public void BeginDraw(Boolean setOffset = true, Boolean setClip = true, Boolean clear = true)
        {

            DeviceContext.D2DContext.BeginDraw();

            if (setOffset)
            {
                SetupOffset();
            }

            if (clear)
            {
                DeviceContext.D2DContext.Clear(null);
            }


            if (setClip)
            {
                SetupClip();
            }

        }

        public void EndDraw()
        {
            ClearClip();
            ResetOffset();

            DeviceContext.D2DContext.EndDraw();
        }
    }
}
