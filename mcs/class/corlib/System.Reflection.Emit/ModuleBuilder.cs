
//
// System.Reflection.Emit/ModuleBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public class ModuleBuilder : Module {
		private IntPtr _impl;
	
		public override string FullyQualifiedName {get { return "FIXME: bah";}}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeBuilder defineType (ModuleBuilder mb, string name, TypeAttributes attr);
		public TypeBuilder DefineType( string name, TypeAttributes attr) {
			return defineType (this, name, attr);
		}

	}
}
