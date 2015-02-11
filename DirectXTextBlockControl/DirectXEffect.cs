using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectXTextBlockControl
{
    public class DirectXEffect : IDisposable
    {
        protected Boolean IsDisposed = false;

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
            }
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }

        ~DirectXEffect()
        {
            if (!IsDisposed)
            {
                Dispose(false);
            }
        }
    }
}
