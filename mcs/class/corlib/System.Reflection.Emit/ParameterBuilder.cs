

//
// System.Reflection.Emit/ParameterBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	public class ParameterBuilder {
		private MethodBase methodb; /* MethodBuilder or ConstructorBuilder */
		private string name;
		private CustomAttributeBuilder[] cattrs;
		private UnmanagedMarshal marshal_info;
		private ParameterAttributes attrs;
		private int position;
		private int table_idx;
		
		internal ParameterBuilder (MethodBase mb, int pos, ParameterAttributes attributes, string strParamName) {
			name = strParamName;
			position = pos;
			attrs = attributes;
			methodb = mb;
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

		[MonoTODO]
		public virtual void SetConstant( object defaultValue) {
			/* FIXME */
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
				marshal_info = CustomAttributeBuilder.get_umarshal (customBuilder, true);
				/* FIXME: check for errors */
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
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		public virtual void SetMarshal( UnmanagedMarshal unmanagedMarshal) {
			marshal_info = unmanagedMarshal;
			attrs |= ParameterAttributes.HasFieldMarshal;
		}





	}
}

