using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel.Composition.Primitives;

namespace Microsoft.Internal
{
    internal class ContractServices
    {
        public static T Cast<T>(object o)
        {
            return (T)o;
        }

        public static bool TryCast(Type contractType, object value, out object result)
        {
            if (value == null)
            {
                result = null;
                return true;
            }
            if (contractType.IsInstanceOfType(value))
            {
                result = value;
                return true;
            }

            // We couldn't cast see if a delegate works for us.
            if (typeof(Delegate).IsAssignableFrom(contractType))
            {
                ExportedDelegate exportedDelegate = value as ExportedDelegate;
                if (exportedDelegate != null)
                {
                    result = exportedDelegate.CreateDelegate(contractType);
                    return (result != null);
                }
            }

            result = null;
            return false;
        }
    }
}

