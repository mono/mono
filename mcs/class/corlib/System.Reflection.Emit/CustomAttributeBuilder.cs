
//
// System.Reflection.Emit/CustomAttributeBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public class CustomAttributeBuilder {
		ConstructorInfo ctor;
		byte[] data;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern byte[] GetBlob(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues);
		
		internal CustomAttributeBuilder( ConstructorInfo con, byte[] cdata) {
			ctor = con;
			data = (byte[])cdata.Clone ();
			/* should we check that the user supplied data is correct? */
		}
		
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs)
			: this (con, constructorArgs, null, null, null, null) {
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs, FieldInfo[] namedFields, object[] fieldValues)
			: this (con, constructorArgs, null, null, namedFields, fieldValues) {
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
			: this (con, constructorArgs, namedProperties, propertyValues, null, null) {
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues) {
			ctor = con;
			data = GetBlob (con, constructorArgs, namedProperties, propertyValues, namedFields, fieldValues);
		}

	}
}
