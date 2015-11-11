using System;
using System.Reflection;

namespace System.Runtime.Serialization
{
	static class CodeInterpreter
	{
		internal static object ConvertValue(object arg, Type source, Type target)
		{
			return InternalConvert(arg, source, target, false);
		}


        static bool CanConvert (TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
					return true;
            }
            return false;
        }

		static object InternalConvert(object arg, Type source, Type target, bool isAddress)
		{

            if (target == source)
                return arg;
            if (target.IsValueType)
            {
                if (source.IsValueType)
                {
                    if (!CanConvert (Type.GetTypeCode (target)))
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.NoConversionPossibleTo, DataContract.GetClrTypeFullName(target))));
                    else
						return target;
                }
                else if (source.IsAssignableFrom(target))
					return arg;
                else
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.IsNotAssignableFrom, DataContract.GetClrTypeFullName(target), DataContract.GetClrTypeFullName(source))));
            }
            else if (target.IsAssignableFrom(source))
				return arg;
            else if (source.IsAssignableFrom(target))
				return arg;
            else if (target.IsInterface || source.IsInterface)
				return arg;
            else
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.IsNotAssignableFrom, DataContract.GetClrTypeFullName(target), DataContract.GetClrTypeFullName(source))));
		}

		public static object GetMember (MemberInfo memberInfo, object instance)
		{
			var pi = memberInfo as PropertyInfo;
			if (pi != null)
				return pi.GetValue (instance);
			else
				return ((FieldInfo) memberInfo).GetValue (instance);
		}

		public static void SetMember (MemberInfo memberInfo, object instance, object value)
		{
			var pi = memberInfo as PropertyInfo;
			if (pi != null)
				pi.SetValue (instance, value);
			else
				((FieldInfo) memberInfo).SetValue (instance, value);
		}
	}
}

