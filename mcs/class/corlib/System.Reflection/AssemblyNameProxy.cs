//
// System.Reflection.AssemblyNameProxy.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Reflection
{
	public class AssemblyNameProxy : MarshalByRefObject
	{
		// Constructor
		public AssemblyNameProxy ()
		{
		}

		// Method
		public AssemblyName GetAssemblyName (string assemblyFile)
		{
			return AssemblyName.GetAssemblyName (assemblyFile);
		}
	}
}
