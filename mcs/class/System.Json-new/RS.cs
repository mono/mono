using System;
using System.Globalization;

internal static class RS
{
    public static string Format(string format, params object[] args)
    {
        return String.Format(CultureInfo.CurrentCulture, format, args);
    }
}
