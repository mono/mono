using System.Globalization;

namespace System.Reflection
{
	partial class Binder
	{
#if MOBILE_LEGACY || MONO_COM
        // CanChangeType
        // This method checks whether the value can be converted into the property type.
        public virtual bool CanChangeType (object value, Type type, CultureInfo culture)
        {
            return false;
        }
#endif
	}
}