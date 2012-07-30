// Type: System.Int64
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System
{
  [ComVisible(true)]
  [__DynamicallyInvokable]
  [Serializable]
  public struct Int64 : IComparable, IFormattable, IConvertible, IComparable<long>, IEquatable<long>
  {
    [__DynamicallyInvokable]
    public const long MaxValue = 9223372036854775807L;
    [__DynamicallyInvokable]
    public const long MinValue = -9223372036854775808L;
    internal long m_value;

    public int CompareTo(object value)
    {
      if (value == null)
        return 1;
      if (!(value is long))
        throw new ArgumentException(Environment.GetResourceString("Arg_MustBeInt64"));
      long num = (long) value;
      if (this < num)
        return -1;
      return this > num ? 1 : 0;
    }

    [__DynamicallyInvokable]
    public int CompareTo(long value)
    {
      if (this < value)
        return -1;
      return this > value ? 1 : 0;
    }

    [__DynamicallyInvokable]
    public override bool Equals(object obj)
    {
      if (!(obj is long))
        return false;
      else
        return this == (long) obj;
    }

    [__DynamicallyInvokable]
    public bool Equals(long obj)
    {
      return this == obj;
    }

    [__DynamicallyInvokable]
    public override int GetHashCode()
    {
      return (int) this ^ (int) (this >> 32);
    }

    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public override string ToString()
    {
      return Number.FormatInt64(this, (string) null, NumberFormatInfo.CurrentInfo);
    }

    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public string ToString(IFormatProvider provider)
    {
      return Number.FormatInt64(this, (string) null, NumberFormatInfo.GetInstance(provider));
    }

    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public string ToString(string format)
    {
      return Number.FormatInt64(this, format, NumberFormatInfo.CurrentInfo);
    }

    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public string ToString(string format, IFormatProvider provider)
    {
      return Number.FormatInt64(this, format, NumberFormatInfo.GetInstance(provider));
    }

    [__DynamicallyInvokable]
    public static long Parse(string s)
    {
      return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
    }

    [__DynamicallyInvokable]
    public static long Parse(string s, NumberStyles style)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.ParseInt64(s, style, NumberFormatInfo.CurrentInfo);
    }

    [__DynamicallyInvokable]
    public static long Parse(string s, IFormatProvider provider)
    {
      return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
    }

    [__DynamicallyInvokable]
    public static long Parse(string s, NumberStyles style, IFormatProvider provider)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.ParseInt64(s, style, NumberFormatInfo.GetInstance(provider));
    }

    [__DynamicallyInvokable]
    public static bool TryParse(string s, out long result)
    {
      return Number.TryParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
    }

    [__DynamicallyInvokable]
    public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out long result)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.TryParseInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
    }

    public TypeCode GetTypeCode()
    {
      return TypeCode.Int64;
    }

    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
      return Convert.ToBoolean(this);
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
      return Convert.ToChar(this);
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      return Convert.ToSByte(this);
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return Convert.ToByte(this);
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return Convert.ToInt16(this);
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return Convert.ToUInt16(this);
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return Convert.ToInt32(this);
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32(this);
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return this;
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return Convert.ToUInt64(this);
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return Convert.ToSingle(this);
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return Convert.ToDouble(this);
    }

    Decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      return Convert.ToDecimal(this);
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", (object) "Int64", (object) "DateTime"));
    }

    object IConvertible.ToType(Type type, IFormatProvider provider)
    {
      return Convert.DefaultToType((IConvertible) this, type, provider);
    }
  }
}
