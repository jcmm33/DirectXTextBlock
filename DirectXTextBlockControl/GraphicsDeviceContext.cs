using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using Factory = SharpDX.DirectWrite.Factory;

namespace DirectXTextBlockControl
{
    public class GraphicsDeviceContext : Component, IDisposable
    {
        private SharpDX.Direct3D11.Device _d3DDevice;
        private SharpDX.Direct2D1.Device _d2DDevice;
        private SharpDX.Direct2D1.DeviceContext _d2DContext;
        private Factory _dwriteFactory;


        public GraphicsDeviceContext()
        {
            CreateDeviceIndepedentResources();
            CreateDeviceResources();
        }

        public SharpDX.DirectWrite.Factory FactoryDirectWrite { get { return _dwriteFactory; } }

        static readonly FeatureLevel[] FeatureLevels = 
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
                FeatureLevel.Level_9_3,
                FeatureLevel.Level_9_2,
                FeatureLevel.Level_9_1,
            };


        public void CreateDeviceIndepedentResources()
        {
            _dwriteFactory = ToDispose(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared));
        }

        public void CreateDeviceResources()
        {
            Utilities.Dispose(ref _d3DDevice);
            Utilities.Dispose(ref _d2DDevice);
            Utilities.Dispose(ref _d2DContext);

            var creationFlags = DeviceCreationFlags.BgraSupport;

#if DEBUG
            // If the project is in a debug build, enable debugging via SDK Layers.
            creationFlags |= DeviceCreationFlags.Debug;
#endif

            // This array defines the set of DirectX hardware feature levels this app will support.
            // Note the ordering should be preserved.
            // Don't forget to declare your application's minimum required feature level in its
            // description.  All applications are assumed to support 9.1 unless otherwise stated.

            // Create the Direct3D 11 API device object.
            _d3DDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, FeatureLevels);

            // Get the Direct3D 11.1 API device.
            using (var dxgiDevice = _d3DDevice.QueryInterface<SharpDX.DXGI.Device>())
            {
                // Create the Direct2D device object and a corresponding context.
                _d2DDevice = new SharpDX.Direct2D1.Device(dxgiDevice);

                _d2DContext = new SharpDX.Direct2D1.DeviceContext(_d2DDevice, DeviceContextOptions.EnableMultithreadedOptimizations);

                if (DesignMode.DesignModeEnabled) return;


                _d2DContext.DotsPerInch = new Size2F((float)LogicalDpi, (float)LogicalDpi);
                _d2DContext.UnitMode = UnitMode.Dips;
            }
        }

        public void Initialize()
        {
            CreateDeviceResources();
        }

        public SharpDX.Direct2D1.Device D2DDevice
        {
            get { return _d2DDevice; }
            set { _d2DDevice = value; }
        }

        public SharpDX.Direct3D11.Device D3DDevice
        {
            get { return _d3DDevice; }
            set { _d3DDevice = value; }
        }

        public SharpDX.Direct2D1.DeviceContext D2DContext
        {
            get { return _d2DContext; }
            set { _d2DContext = value; }
        }

        private DisplayInformation _displayInformation = null;


        public DisplayInformation DisplayInformation
        {
            get
            {
                if (!DesignMode.DesignModeEnabled)
                {
                    return _displayInformation ?? (_displayInformation = DisplayInformation.GetForCurrentView());
                }

                else
                {
                    return null;
                }
            }
        }


        public double LogicalDpi
        {
            get
            {
                if (!DesignMode.DesignModeEnabled)
                {
                    return DisplayInformation.LogicalDpi;
                }
                else
                {
                    return 96.0;
                }
            }
        }


        public void SetDpi(Windows.Foundation.Size dpi)
        {
            _d2DContext.DotsPerInch = new Size2F((float)dpi.Width, (float)dpi.Height);
        }

        private Boolean _disposed = false;

        protected override void Dispose(bool disposing)
        {

            if (_disposed) return;

            if (disposing)
            {
                Utilities.Dispose(ref _d3DDevice);
                Utilities.Dispose(ref _d2DDevice);
                Utilities.Dispose(ref _d2DContext);
            }

            _disposed = true;
        }

        ~GraphicsDeviceContext()
        {
            Dispose(false);
        }


        public int ConvertPixelsToDips(int pixels)
        {
            const float dipsPerInch = 96.0f;
            return (int)(pixels * dipsPerInch / this.LogicalDpi);
        }

        public int ConvertDipsToPixels(double dips)
        {
            const float dipsPerInch = 96.0f;

            return (int)((dips * this.LogicalDpi) / dipsPerInch);
        }


    }
}
