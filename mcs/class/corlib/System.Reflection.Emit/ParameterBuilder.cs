

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

namespace System.Reflection.Emit {
	public class ParameterBuilder {
		private MethodBuilder methodb;
		private string name;
		private ParameterAttributes attrs;
		private int position;
		private int table_idx;
		
		internal ParameterBuilder (MethodBuilder mb, int pos, ParameterAttributes attributes, string strParamName) {
			name = strParamName;
			position = pos;
			attrs = attributes;
			methodb = mb;
			table_idx = mb.type.module.assemblyb.get_next_table_index (0x08, true);
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

		public virtual void SetConstant( object defaultValue) {
			/* FIXME */
		}
		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}
		public virtual void SetMarshal( UnmanagedMarshal unmanagedMarshal) {
		}





	}
}

