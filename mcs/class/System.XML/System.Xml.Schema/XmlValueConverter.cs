//
// XmlValueConverter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

#if NET_2_0

using System;

namespace System.Xml.Schema
{
	public abstract class XmlValueConverter
	{
		[MonoTODO]
		protected XmlValueConverter ()
		{
		}

		public abstract object ChangeType (bool value, Type type);

		public abstract object ChangeType (DateTime value, Type type);

		public abstract object ChangeType (decimal value, Type type);

		public abstract object ChangeType (double value, Type type);

		public abstract object ChangeType (int value, Type type);

		public abstract object ChangeType (long value, Type type);

		public abstract object ChangeType (object value, Type type);

		public abstract object ChangeType (float value, Type type);

		public abstract object ChangeType (string value, Type type);

		public abstract object ChangeType (object value, Type type, IXmlNamespaceResolver nsResolver);

		public abstract object ChangeType (string value, Type type, IXmlNamespaceResolver nsResolver);

		public abstract bool ToBoolean (bool value);

		public abstract bool ToBoolean (DateTime value);

		public abstract bool ToBoolean (decimal value);

		public abstract bool ToBoolean (double value);

		public abstract bool ToBoolean (int value);

		public abstract bool ToBoolean (long value);

		public abstract bool ToBoolean (object value);

		public abstract bool ToBoolean (float value);

		public abstract bool ToBoolean (string value);

		public abstract bool ToDateTime (bool value);

		public abstract bool ToDateTime (DateTime value);

		public abstract bool ToDateTime (decimal value);

		public abstract bool ToDateTime (double value);

		public abstract bool ToDateTime (int value);

		public abstract bool ToDateTime (long value);

		public abstract bool ToDateTime (object value);

		public abstract bool ToDateTime (float value);

		public abstract bool ToDateTime (string value);

		public abstract bool ToDecimal (bool value);

		public abstract bool ToDecimal (DateTime value);

		public abstract bool ToDecimal (decimal value);

		public abstract bool ToDecimal (double value);

		public abstract bool ToDecimal (int value);

		public abstract bool ToDecimal (long value);

		public abstract bool ToDecimal (object value);

		public abstract bool ToDecimal (float value);

		public abstract bool ToDecimal (string value);

		public abstract bool ToDouble (bool value);

		public abstract bool ToDouble (DateTime value);

		public abstract bool ToDouble (decimal value);

		public abstract bool ToDouble (double value);

		public abstract bool ToDouble (int value);

		public abstract bool ToDouble (long value);

		public abstract bool ToDouble (object value);

		public abstract bool ToDouble (float value);

		public abstract bool ToDouble (string value);

		public abstract bool ToInt32 (bool value);

		public abstract bool ToInt32 (DateTime value);

		public abstract bool ToInt32 (decimal value);

		public abstract bool ToInt32 (double value);

		public abstract bool ToInt32 (int value);

		public abstract bool ToInt32 (long value);

		public abstract bool ToInt32 (object value);

		public abstract bool ToInt32 (float value);

		public abstract bool ToInt32 (string value);

		public abstract bool ToInt64 (bool value);

		public abstract bool ToInt64 (DateTime value);

		public abstract bool ToInt64 (decimal value);

		public abstract bool ToInt64 (double value);

		public abstract bool ToInt64 (int value);

		public abstract bool ToInt64 (long value);

		public abstract bool ToInt64 (object value);

		public abstract bool ToInt64 (float value);

		public abstract bool ToInt64 (string value);

		public abstract bool ToSingle (bool value);

		public abstract bool ToSingle (DateTime value);

		public abstract bool ToSingle (decimal value);

		public abstract bool ToSingle (double value);

		public abstract bool ToSingle (int value);

		public abstract bool ToSingle (long value);

		public abstract bool ToSingle (object value);

		public abstract bool ToSingle (float value);

		public abstract bool ToSingle (string value);

		public abstract bool ToString (bool value);

		public abstract bool ToString (DateTime value);

		public abstract bool ToString (decimal value);

		public abstract bool ToString (double value);

		public abstract bool ToString (int value);

		public abstract bool ToString (long value);

		public abstract bool ToString (object value);

		public abstract bool ToString (float value);

		public abstract bool ToString (string value);
	}
}

#endif
