using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    [global::System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class DBLinqExtendedAttribute : Attribute
    {
        public DBLinqExtendedAttribute()
        {

        }
    }
}
