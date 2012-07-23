using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    public static class ObjectExtensions
    {
        public static long? ConvertToLong(this object value)
        {
            var convertible = value as IConvertible;
            if (convertible != null)
            {
                try
                {
                    convertible.ToInt64 (null);
                }
                catch
                {
                }
            }

            return null;
        }
    }
}