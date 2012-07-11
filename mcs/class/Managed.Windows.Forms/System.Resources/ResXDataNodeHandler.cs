using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace System.Resources {
	internal abstract class ResXDataNodeHandler {
		protected ResXDataNodeHandler ()
		{
		}

		public abstract object GetValue (ITypeResolutionService typeResolver);
		
		public abstract object GetValue (AssemblyName[] assemblyNames);

		public abstract string GetValueTypeName (ITypeResolutionService typeResolver);

		public abstract string GetValueTypeName (AssemblyName[] assemblyNames);

		//override by any inheritor that doesnt want to send the default output of GetValue to be written to ResXFile
		public virtual object GetValueForResX ()
		{
			return GetValue ((AssemblyName[]) null);
		}

		public Type ResolveType (string typeString) 
		{
			// FIXME: check the test that shows you cant load a type with just a fullname from current assembly is valid
			return Type.GetType (typeString);
		}

		protected Type ResolveType (string typeString, AssemblyName[] assemblyNames) 
		{
			Type result = null;

			//FIXME: should I unload the assemblies again if type not found?
			if (assemblyNames != null) {
				foreach (AssemblyName assem in assemblyNames) {
						Assembly myAssembly = Assembly.Load (assem);
						result = myAssembly.GetType (typeString, false);
						if (result != null)
							return result;
					}
			}
			if (result == null)
				result = ResolveType (typeString);

			return result;
		}

		protected Type ResolveType (string typeString, ITypeResolutionService typeResolver) 
		{
			// mono implementation previously didnt fallback to type check
			Type result = null;

			if (typeResolver != null)
				result = typeResolver.GetType (typeString);

			if (result == null)
				result = ResolveType (typeString);

			return result;
		}
	}
}
