using System;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel;

namespace System.Resources {
	internal class ByteArrayFromResXHandler : ResXDataNodeHandler {

		string dataString;

		public ByteArrayFromResXHandler (string data)
		{
			dataString = data;
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			return Convert.FromBase64String (dataString);
		}

		public override object GetValue (AssemblyName[] assemblyNames)
		{
			return Convert.FromBase64String (dataString);
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			//FIXME: what if only fullname for byte[] present?
			Type type = ResolveType (typeof (byte[]).AssemblyQualifiedName, typeResolver);
			return type.AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName[] assemblyNames)
		{
			//FIXME: what if only fullname for byte[] present?
			return typeof (byte[]).AssemblyQualifiedName;
		}
		#endregion

	}
}

