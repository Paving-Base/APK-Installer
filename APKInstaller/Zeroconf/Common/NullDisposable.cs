using System;

namespace Zeroconf.Common
{
    internal sealed class NullDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
