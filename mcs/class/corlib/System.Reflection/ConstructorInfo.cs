//
// System.Reflection/ConstructorInfo.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class ConstructorInfo : MethodBase {
		public static readonly string ConstructorName = ".ctor";
		public static readonly string TypeConstructorName = ".cctor";

		protected ConstructorInfo() {
		}
		
		public override MemberTypes MemberType {
			get {return MemberTypes.Constructor;}
		}

		public object Invoke (object[] parameters)
		{
			if (parameters == null)
				parameters = new object [0];

			return Invoke (BindingFlags.CreateInstance, null, parameters, null);
		}

		public abstract object Invoke (BindingFlags invokeAttr, Binder binder, object[] parameters,
					       CultureInfo culture);
		
	}
}
