//
// System.Runtime.Remoting.TypeInfo.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;

namespace System.Runtime.Remoting
{
	[Serializable]
	internal class TypeInfo : IRemotingTypeInfo
	{
		string serverType;
		string[] serverHierarchy;
		string[] interfacesImplemented;

		public TypeInfo(Type type)
		{
			if (type.IsInterface)
			{
				serverType = typeof (MarshalByRefObject).AssemblyQualifiedName;
				serverHierarchy = new string[0];
				interfacesImplemented = new string[] { type.AssemblyQualifiedName };
			}
			else
			{
				serverType = type.AssemblyQualifiedName;

				// base class info

				int baseCount = 0;
				Type baseType = type.BaseType;
				while (baseType != typeof (MarshalByRefObject) && baseType != typeof(object))
				{
					baseType = baseType.BaseType;
					baseCount++;
				}

				serverHierarchy = new string[baseCount];
				baseType = type.BaseType;
				for (int n=0; n<baseCount; n++) 
				{
					serverHierarchy[n] = baseType.AssemblyQualifiedName;
					baseType = baseType.BaseType;
				}

				// Interfaces info

				Type[] interfaces = type.GetInterfaces();
				interfacesImplemented = new string[interfaces.Length];
				for (int n=0; n<interfaces.Length; n++)
					interfacesImplemented[n] = interfaces[n].AssemblyQualifiedName;
			}
		}

		public string TypeName 
		{
			get { return serverType; }
			set { serverType = value; }
		}

		public bool CanCastTo (Type fromType, object o)
		{
			if (fromType == typeof (object)) return true;
			if (fromType == typeof (MarshalByRefObject)) return true;

			string fromName = fromType.AssemblyQualifiedName;

			// Find the type comparing the name of the type and the name of the assembly,
			// excluding version and other assembly info

			int i = fromName.IndexOf (",");
			if (i != -1) i = fromName.IndexOf (",", i+1);
			if (i != -1) fromName = fromName.Substring (0,i+1);
			else fromName += ",";

			if ( (serverType + ",").StartsWith (fromName)) return true;

			foreach (string basec in serverHierarchy)
				if ( (basec + ",").StartsWith (fromName)) return true;

			foreach (string basec in interfacesImplemented)
				if ( (basec + ",").StartsWith (fromName)) return true;

			return false;
		}
	}
}
