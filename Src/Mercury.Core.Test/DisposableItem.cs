using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Core.Test
{
    /// <summary>
    /// A test class for disposable items.
    /// </summary>
    public class DisposableItem : IDisposable
    {
        public DisposableItem(int value)
        {
            this.Value = value;
            this.IsDisposed = false;
        }

        public int Value { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Value = 0;
                this.IsDisposed = true;
            }
        }
    }
}
