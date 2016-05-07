using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A ManualResetEventSlim that also remembers a value that was stored at the last Set().
    /// </summary>
    internal class IntValueEvent : ManualResetEventSlim
    {
        internal int Value;

        internal IntValueEvent()
            : base(false)
        {
            Value = 0;
        }

        internal void Set(int index)
        {
            Value = index;
            base.Set();
        }
    }
}
