
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

namespace System.Reflection.Emit {
	public class CustomAttributeBuilder {
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
		}

	}
}
