// Type: System.Int32
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;

namespace System
{
  [ComVisible(true)]
  [__DynamicallyInvokable]
  [Serializable]
  public struct Int32 : IComparable, IFormattable, IConvertible, IComparable<int>, IEquatable<int>
  {
    [__DynamicallyInvokable]
    public const int MaxValue = 2147483647;
    [__DynamicallyInvokable]
    public const int MinValue = -2147483648;
    internal int m_value;

    public int CompareTo(object value)
    {
      if (value == null)
        return 1;
      if (!(value is int))
        throw new ArgumentException(Environment.GetResourceString("Arg_MustBeInt32"));
      int num = (int) value;
      if (this < num)
        return -1;
      return this > num ? 1 : 0;
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public int CompareTo(int value)
    {
      if (this < value)
        return -1;
      return this > value ? 1 : 0;
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public override bool Equals(object obj)
    {
      if (!(obj is int))
        return false;
      else
        return this == (int) obj;
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public bool Equals(int obj)
    {
      return this == obj;
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public override int GetHashCode()
    {
      return this;
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public override string ToString()
    {
      return Number.FormatInt32(this, (string) null, NumberFormatInfo.CurrentInfo);
    }

    [SecuritySafeCritical]
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public string ToString(string format)
    {
      return Number.FormatInt32(this, format, NumberFormatInfo.CurrentInfo);
    }

    [SecuritySafeCritical]
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public string ToString(IFormatProvider provider)
    {
      return Number.FormatInt32(this, (string) null, NumberFormatInfo.GetInstance(provider));
    }

    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public string ToString(string format, IFormatProvider provider)
    {
      return Number.FormatInt32(this, format, NumberFormatInfo.GetInstance(provider));
    }

    [__DynamicallyInvokable]
    public static int Parse(string s)
    {
      return Number.ParseInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
    }

    [__DynamicallyInvokable]
    public static int Parse(string s, NumberStyles style)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.ParseInt32(s, style, NumberFormatInfo.CurrentInfo);
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public static int Parse(string s, IFormatProvider provider)
    {
      return Number.ParseInt32(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public static int Parse(string s, NumberStyles style, IFormatProvider provider)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.ParseInt32(s, style, NumberFormatInfo.GetInstance(provider));
    }

    [__DynamicallyInvokable]
    public static bool TryParse(string s, out int result)
    {
      return Number.TryParseInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out int result)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.TryParseInt32(s, style, NumberFormatInfo.GetInstance(provider), out result);
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    public TypeCode GetTypeCode()
    {
      return TypeCode.Int32;
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

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return this;
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32(this);
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return Convert.ToInt64(this);
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
      throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", (object) "Int32", (object) "DateTime"));
    }

    object IConvertible.ToType(Type type, IFormatProvider provider)
    {
      return Convert.DefaultToType((IConvertible) this, type, provider);
    }
  }
}
