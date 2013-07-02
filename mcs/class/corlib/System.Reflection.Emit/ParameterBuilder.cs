
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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


//
// System.Reflection.Emit/ParameterBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

#if !FULL_AOT_RUNTIME
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_ParameterBuilder))]
	[ClassInterface (ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
	public class ParameterBuilder : _ParameterBuilder {

#pragma warning disable 169, 414
		private MethodBase methodb; /* MethodBuilder, ConstructorBuilder or DynamicMethod */
		private string name;
		private CustomAttributeBuilder[] cattrs;
		private UnmanagedMarshal marshal_info;
		private ParameterAttributes attrs;
		private int position;
		private int table_idx;
		object def_value;
#pragma warning restore 169, 414
		
		internal ParameterBuilder (MethodBase mb, int pos, ParameterAttributes attributes, string strParamName) {
			name = strParamName;
			position = pos;
			attrs = attributes;
			methodb = mb;
			if (mb is DynamicMethod)
				table_idx = 0;
			else
				table_idx = mb.get_next_table_index (this, 0x08, true);
		}

		public virtual int Attributes {
			get {return (int)attrs;}
		}
		public bool IsIn {
			get {return ((int)attrs & (int)ParameterAttributes.In) != 0;}
		}
		public bool IsOut {
			get {return ((int)attrs & (int)ParameterAttributes.Out) != 0;}
		}
		public bool IsOptional {
			get {return ((int)attrs & (int)ParameterAttributes.Optional) != 0;}
		}
		public virtual string Name {
			get {return name;}
		}
		public virtual int Position {
			get {return position;}
		}

		public virtual ParameterToken GetToken() {
			return new ParameterToken (0x08 | table_idx);
		}

		public virtual void SetConstant (object defaultValue)
		{
			if (position > 0) {
				Type t = methodb.GetParameterType (position - 1);
				if (defaultValue != null && t != defaultValue.GetType ()) {
					if(!t.IsEnum || t.UnderlyingSystemType != defaultValue.GetType ())
						throw new ArgumentException ("Constant does not match the defined type.");
				}
				if (t.IsValueType && !t.IsPrimitive && !t.IsEnum && t != typeof (DateTime))
					throw new ArgumentException ("" + t + " is not a supported constant type.");
			}

			def_value = defaultValue;
			attrs |= ParameterAttributes.HasDefault;
		}
		
		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
			string attrname = customBuilder.Ctor.ReflectedType.FullName;
			if (attrname == "System.Runtime.InteropServices.InAttribute") {
				attrs |= ParameterAttributes.In;
				return;
			} else if (attrname == "System.Runtime.InteropServices.OutAttribute") {
				attrs |= ParameterAttributes.Out;
				return;
			} else if (attrname == "System.Runtime.InteropServices.OptionalAttribute") {
				attrs |= ParameterAttributes.Optional;
				return;
			} else if (attrname == "System.Runtime.InteropServices.MarshalAsAttribute") {
				attrs |= ParameterAttributes.HasFieldMarshal;
				marshal_info = CustomAttributeBuilder.get_umarshal (customBuilder, false);
				/* FIXME: check for errors */
				return;
			} else if (attrname == "System.Runtime.InteropServices.DefaultParameterValueAttribute") {
				/* MS.NET doesn't handle this attribute but we handle it for consistency */
				CustomAttributeBuilder.CustomAttributeInfo cinfo = CustomAttributeBuilder.decode_cattr (customBuilder);
				/* FIXME: check for type compatibility */
				SetConstant (cinfo.ctorArgs [0]);
				return;
			}

			if (cattrs != null) {
				CustomAttributeBuilder[] new_array = new CustomAttributeBuilder [cattrs.Length + 1];
				cattrs.CopyTo (new_array, 0);
				new_array [cattrs.Length] = customBuilder;
				cattrs = new_array;
			} else {
				cattrs = new CustomAttributeBuilder [1];
				cattrs [0] = customBuilder;
			}
		}

		[ComVisible (true)]
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		[Obsolete ("An alternate API is available: Emit the MarshalAs custom attribute instead.")]
		public virtual void SetMarshal( UnmanagedMarshal unmanagedMarshal) {
			marshal_info = unmanagedMarshal;
			attrs |= ParameterAttributes.HasFieldMarshal;
		}

                void _ParameterBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
                {
                        throw new NotImplementedException ();
                }

                void _ParameterBuilder.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
                {
                        throw new NotImplementedException ();
                }

                void _ParameterBuilder.GetTypeInfoCount (out uint pcTInfo)
                {
                        throw new NotImplementedException ();
                }

                void _ParameterBuilder.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
                {
                        throw new NotImplementedException ();
                }
	}
}

#endif
