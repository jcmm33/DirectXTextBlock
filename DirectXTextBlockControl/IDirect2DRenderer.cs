using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectXTextBlockControl
{
    public interface IDirect2DRenderer
    {
        void Reset(GraphicsDeviceContext gdc);
        void DrawAsync(RenderContext renderContext);
    }
}
