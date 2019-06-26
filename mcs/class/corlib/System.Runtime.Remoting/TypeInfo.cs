//
// System.Runtime.Remoting.TypeInfo.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
				Type[] interfaces = type.GetInterfaces();
				interfacesImplemented = new string[interfaces.Length + 1];
				for(int n=0; n<interfaces.Length; n++)
					interfacesImplemented[n] = interfaces[n].AssemblyQualifiedName;
				interfacesImplemented[interfaces.Length] = type.AssemblyQualifiedName;
			}
			else
			{
				serverType = type.AssemblyQualifiedName;

				// base class info

				int baseCount = 0;
				Type baseType = type.BaseType;
				while (baseType != typeof (MarshalByRefObject) && baseType != null)
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

			int i = fromName.IndexOf (',');
			if (i != -1) i = fromName.IndexOf (',', i+1);
			if (i != -1) fromName = fromName.Substring (0,i+1);
			else fromName += ",";

			if ( (serverType + ",").StartsWith (fromName)) return true;

			if (serverHierarchy != null)
				foreach (string basec in serverHierarchy)
					if ( (basec + ",").StartsWith (fromName)) return true;

			if (interfacesImplemented != null)
				foreach (string basec in interfacesImplemented)
					if ( (basec + ",").StartsWith (fromName)) return true;
			
			return false;
		}
	}
}
