// 
// SoapHeaderMapping.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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

using System.Reflection;

namespace System.Web.Services.Protocols
{
#if NET_2_0
	public
#else
	internal
#endif
	sealed class SoapHeaderMapping // It used to be HeaderInfo class until Mono 1.2
	{
		MemberInfo member;
		Type header_type;
		bool is_unknown_header;
		SoapHeaderDirection direction;

		internal SoapHeaderMapping (MemberInfo member, SoapHeaderAttribute attributeInfo)
		{
			this.member = member;
			direction = attributeInfo.Direction;
			if (member is PropertyInfo)
				header_type = ((PropertyInfo) member).PropertyType;
			else
				header_type = ((FieldInfo) member).FieldType;
			
			if (HeaderType == typeof(SoapHeader) || HeaderType == typeof(SoapUnknownHeader) ||
				HeaderType == typeof(SoapHeader[]) || HeaderType == typeof(SoapUnknownHeader[]))
			{
				is_unknown_header = true;
			}
			else if (!typeof(SoapHeader).IsAssignableFrom (HeaderType))
				throw new InvalidOperationException (string.Format ("Header members type must be a SoapHeader subclass"));
		}
		
		internal object GetHeaderValue (object ob)
		{
			if (member is PropertyInfo)
				return ((PropertyInfo) member).GetValue (ob, null);
			else
				return ((FieldInfo) member).GetValue (ob);
		}

		internal void SetHeaderValue (object ob, SoapHeader header)
		{
			object value = header;
			if (Custom && HeaderType.IsArray)
			{
				SoapUnknownHeader uheader = header as SoapUnknownHeader;
				SoapUnknownHeader[] array = (SoapUnknownHeader[]) GetHeaderValue (ob);
				if (array == null || array.Length == 0) {
					value = new SoapUnknownHeader[] { uheader };
				}
				else {
					SoapUnknownHeader[] newArray = new SoapUnknownHeader [array.Length+1];
					Array.Copy (array, newArray, array.Length);
					newArray [array.Length] = uheader;
					value = newArray;
				}
			}
			
			if (member is PropertyInfo)
				((PropertyInfo) member).SetValue (ob, value, null);
			else
				((FieldInfo) member).SetValue (ob, value);
		}
		
		public SoapHeaderDirection Direction
		{
			get { return direction; }
		}

		public MemberInfo MemberInfo {
			get { return member; }
		}

		public Type HeaderType {
			get { return header_type; }
		}

		public bool Custom {
			get { return is_unknown_header; }
		}

		[MonoTODO]
		public bool Repeats {
			get { throw new NotImplementedException (); }
		}
	}

}
