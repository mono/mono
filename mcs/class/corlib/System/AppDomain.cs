using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System {
	public interface _AppDomain {
	}
	public sealed class AppDomain /* : MarshalByRefObject , _AppDomain, IEvidenceFactory */ {

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain {
			get { return getCurDomain ();}
		}
		public AssemblyBuilder DefineDynamicAssembly( AssemblyName name, AssemblyBuilderAccess access) {
			return null;
		}


	
	}


}
