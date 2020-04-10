using System.Collections;
using System.Diagnostics;
using System.Reflection;

internal static class CountPropertyHelper
{
    public static bool TryGetCount(object value, out int count)
    {
        Debug.Assert(value != null);

        if (value is ICollection collection)
        {
            count = collection.Count;
            return true;
        }

        PropertyInfo property = value.GetType().GetRuntimeProperty("Count");
        if (property != null && property.CanRead && property.PropertyType == typeof(int))
        {
            count = (int)property.GetValue(value);
            return true;
        }

        count = -1;
        return false;
    }
}