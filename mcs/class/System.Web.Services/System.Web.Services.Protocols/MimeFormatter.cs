// 
// System.Web.Services.Protocols.MimeFormatter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Protocols {
	public abstract class MimeFormatter {

		#region Constructors

		protected MimeFormatter () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public static MimeFormatter CreateInstance (Type type, object initializer)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			ob.Initialize (initializer);
			return ob;
		}

		public abstract object GetInitializer (LogicalMethodInfo methodInfo);

		public static object GetInitializer (Type type, LogicalMethodInfo methodInfo)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			return ob.GetInitializer (methodInfo);
		}

		public virtual object[] GetInitializers (LogicalMethodInfo[] methodInfos)
		{
			object[] initializers = new object [methodInfos.Length];
			for (int n=0; n<methodInfos.Length; n++)
				initializers [n] = GetInitializer (methodInfos[n]);
				
			return initializers;
		}

		public static object[] GetInitializers (Type type, LogicalMethodInfo[] methodInfos)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			return ob.GetInitializers (methodInfos);
		}

		public abstract void Initialize (object initializer);	

		internal static object StringToObj (Type type, string value)
		{
			if (type.IsEnum)
				return Enum.Parse (type, value);
			
			switch (Type.GetTypeCode (type))
			{
				case TypeCode.Boolean: return XmlConvert.ToBoolean (value);
				case TypeCode.Byte: return XmlConvert.ToByte (value);
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value);
				case TypeCode.Decimal: return XmlConvert.ToDecimal (value);
				case TypeCode.Double: return XmlConvert.ToDouble (value);
				case TypeCode.Int16: return XmlConvert.ToInt16 (value);
				case TypeCode.Int32: return XmlConvert.ToInt32 (value);
				case TypeCode.Int64: return XmlConvert.ToInt64 (value);
				case TypeCode.SByte: return XmlConvert.ToSByte (value);
				case TypeCode.Single: return XmlConvert.ToSingle (value);
				case TypeCode.UInt16: return XmlConvert.ToUInt16 (value);
				case TypeCode.UInt32: return XmlConvert.ToUInt32 (value);
				case TypeCode.UInt64: return XmlConvert.ToUInt64 (value);
				case TypeCode.String: return value;
				case TypeCode.Char:
					if (value.Length != 1) throw new InvalidOperationException ("Invalid char value");
					return value [0];
			}
			throw new InvalidOperationException ("Type not supported");
		}

		internal static string ObjToString (object value)
		{
			if (value == null) return "";
			switch (Type.GetTypeCode (value.GetType ()))
			{
				case TypeCode.Boolean: return XmlConvert.ToString ((bool)value);
				case TypeCode.Byte: return XmlConvert.ToString ((byte)value);
				case TypeCode.Char: return XmlConvert.ToString ((char)value);
				case TypeCode.DateTime: return XmlConvert.ToString ((DateTime)value);
				case TypeCode.Decimal: return XmlConvert.ToString ((decimal)value);
				case TypeCode.Double: return XmlConvert.ToString ((double)value);
				case TypeCode.Int16: return XmlConvert.ToString ((Int16)value);
				case TypeCode.Int32: return XmlConvert.ToString ((Int32)value);
				case TypeCode.Int64: return XmlConvert.ToString ((Int64)value);
				case TypeCode.SByte: return XmlConvert.ToString ((sbyte)value);
				case TypeCode.Single: return XmlConvert.ToString ((Single)value);
				case TypeCode.UInt16: return XmlConvert.ToString ((UInt16)value);
				case TypeCode.UInt32: return XmlConvert.ToString ((UInt32)value);
				case TypeCode.UInt64: return XmlConvert.ToString ((UInt64)value);
				case TypeCode.String: return value as string;
			}
			if (value.GetType().IsEnum)
				return value.ToString ();

			throw new InvalidOperationException ("Type not supported");
		}
		
		#endregion // Methods
	}
}
