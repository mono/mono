using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DbLinq.Data.Linq.Database
{
#if MONO_STRICT
    internal
#else
    public
#endif
 interface IDbLinqCommand : IDisposable
    {
        IDbCommand Command { get; }
        /// <summary>
        /// Commits the current transaction.
        /// throws NRE if _transaction is null. Behavior is intentional.
        /// </summary>
        void Commit();
    }
}
