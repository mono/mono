

//
// System.Reflection.Emit/FieldBuilder.cs
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
	public sealed class FieldBuilder : FieldInfo {
		private FieldAttributes attrs;
		private Type type;
		private String name;
		private object def_value;
		private int offset;
		private int table_idx;
		internal TypeBuilder typeb;
		private byte[] rva_data;

		internal FieldBuilder (TypeBuilder tb, string fieldName, Type type, FieldAttributes attributes) {
			attrs = attributes;
			name = fieldName;
			this.type = type;
			offset = -1;
			typeb = tb;
			table_idx = tb.get_next_table_index (0x04, true);
		}

		public override FieldAttributes Attributes {
			get {return attrs;}
		}
		public override Type DeclaringType {
			get {return typeb;}
		}
		public override RuntimeFieldHandle FieldHandle {
			get {return new RuntimeFieldHandle();}
		}
		public override Type FieldType {
			get {return type;}
		}
		public override string Name {
			get {return name;}
		}
		public override Type ReflectedType {
			get {return null;}
		}

		public override object[] GetCustomAttributes(bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return null;
		}
		public FieldToken GetToken() {
			return new FieldToken (0x04000000 | table_idx);
		}
		public override object GetValue(object obj) {
			return null;
		}
		public override bool IsDefined( Type attributeType, bool inherit) {
			return false;
		}
		internal void SetRVAData (byte[] data) {
			rva_data = (byte[])data.Clone ();
		}
		public void SetConstant( object defaultValue) {
			/*if (defaultValue.GetType() != type)
				throw new ArgumentException ("Constant doesn't match field type");*/
			def_value = defaultValue;
		}
		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}
		public void SetMarshal( UnmanagedMarshal unmanagedMarshal) {
		}
		public void SetOffset( int iOffset) {
			offset = iOffset;
		}
		public override void SetValue( object obj, object val, BindingFlags invokeAttr, Binder binder, CultureInfo culture) {
		}

	}
}

