
//
// System/AppDomain.cs
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

namespace System {

	public interface AppDomain_Intf {
	}

	public sealed class AppDomain /* : MarshalByRefObject , _AppDomain, IEvidenceFactory */ {

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain
		{
			get {
				return getCurDomain ();
			}
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access)
		{
			AssemblyBuilder ab = new AssemblyBuilder (name, access);
			return ab;
		}
	}
}
