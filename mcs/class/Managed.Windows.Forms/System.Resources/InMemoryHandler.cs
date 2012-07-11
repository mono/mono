using System;
using System.Reflection;
using System.ComponentModel.Design;

namespace System.Resources {
	internal class InMemoryHandler : ResXDataNodeHandler {

		object value;

		public InMemoryHandler (object valueObject)
		{
			value = valueObject;
		}

		#region implemented abstract members of System.Windows.Formsnet_2_0.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			return value;
		}

		public override object GetValue (AssemblyName[] assemblyNames)
		{
			return value;
		}
		//FIXME: what if value null??
		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			return value.GetType ().AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName[] assemblyNames)
		{
			return value.GetType ().AssemblyQualifiedName;
		}
		#endregion

	}
}

