using System;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.IO;

namespace System.Resources {
	internal class FileRefHandler : ResXDataNodeHandler {

		ResXFileRef resXFileRef; // may not be the same as that referenced in ResXDataNode
		ResXFileRef originalResXFileRef; // will be same

		public FileRefHandler (ResXFileRef fileRef, string basePath)
		{

			originalResXFileRef = fileRef;
			// recreate object to apply basePath if present
			if (basePath == null) {
				resXFileRef = fileRef;
			} else {
				string pathToUse = Path.Combine (basePath, fileRef.FileName);
				resXFileRef = new ResXFileRef (pathToUse,fileRef.TypeName);
			}
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			TypeConverter c = TypeDescriptor.GetConverter (typeof (ResXFileRef));
			return c.ConvertFromInvariantString (resXFileRef.ToString ());				
		}

		public override object GetValue (AssemblyName[] assemblyNames)
		{
			TypeConverter c = TypeDescriptor.GetConverter (typeof (ResXFileRef));
			return c.ConvertFromInvariantString (resXFileRef.ToString ());	
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			// although params ignored by GetValue. .NET resolves the type for GetValueTypeName
			return ResolveType (resXFileRef.TypeName, typeResolver).AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName[] assemblyNames)
		{
			// although params ignored by GetValue. .NET resolves the type for GetValueTypeName
			return ResolveType (resXFileRef.TypeName, assemblyNames).AssemblyQualifiedName;
		}
		#endregion

		public override object GetValueForResX ()
		{
			return originalResXFileRef;
		}
	}
}

