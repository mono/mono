// Type: System.Object
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System.Globalization;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Security;

namespace System
{
  [ComVisible(true)]
  [ClassInterface(ClassInterfaceType.AutoDual)]
  [__DynamicallyInvokable]
  [Serializable]
  public class Object
  {
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public Object()
    {
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [__DynamicallyInvokable]
    ~Object()
    {
    }

    [__DynamicallyInvokable]
    public virtual string ToString()
    {
      return this.GetType().ToString();
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public virtual bool Equals(object obj)
    {
      return RuntimeHelpers.Equals(this, obj);
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public static bool Equals(object objA, object objB)
    {
      if (objA == objB)
        return true;
      if (objA == null || objB == null)
        return false;
      else
        return objA.Equals(objB);
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public static bool ReferenceEquals(object objA, object objB)
    {
      return objA == objB;
    }

    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    [__DynamicallyInvokable]
    public virtual int GetHashCode()
    {
      return RuntimeHelpers.GetHashCode(this);
    }

    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    [MethodImpl(MethodImplOptions.InternalCall)]
    public Type GetType();

    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    [MethodImpl(MethodImplOptions.InternalCall)]
    protected object MemberwiseClone();

    [SecurityCritical]
    private void FieldSetter(string typeName, string fieldName, object val)
    {
      FieldInfo fieldInfo = this.GetFieldInfo(typeName, fieldName);
      if (fieldInfo.IsInitOnly)
        throw new FieldAccessException(Environment.GetResourceString("FieldAccess_InitOnly"));
      Message.CoerceArg(val, fieldInfo.FieldType);
      fieldInfo.SetValue(this, val);
    }

    private void FieldGetter(string typeName, string fieldName, ref object val)
    {
      FieldInfo fieldInfo = this.GetFieldInfo(typeName, fieldName);
      val = fieldInfo.GetValue(this);
    }

    private FieldInfo GetFieldInfo(string typeName, string fieldName)
    {
      Type type = this.GetType();
      while ((Type) null != type && !type.FullName.Equals(typeName))
        type = type.BaseType;
      if ((Type) null == type)
      {
        throw new RemotingException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), new object[1]
        {
          (object) typeName
        }));
      }
      else
      {
        FieldInfo field = type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
        if (!((FieldInfo) null == field))
          return field;
        throw new RemotingException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadField"), new object[2]
        {
          (object) fieldName,
          (object) typeName
        }));
      }
    }
  }
}
