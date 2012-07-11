using System;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel;

namespace System.Resources {
	internal class TypeConverterFromResXHandler : ResXDataNodeHandler {

		string dataString;
		string mime_type;
		string typeString;

		public TypeConverterFromResXHandler (string data, string _mime_type, string _typeString)
		{
			dataString = data;
			mime_type = _mime_type;
			typeString = _typeString;
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			Type type = ResolveType (typeString, typeResolver);

			TypeConverter c = TypeDescriptor.GetConverter (type);
			return ConvertData (c);
		}

		public override object GetValue (AssemblyName[] assemblyNames)
		{
			Type type = ResolveType (typeString, assemblyNames);

			TypeConverter c = TypeDescriptor.GetConverter (type);
			return ConvertData (c);
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			Type type = ResolveType (typeString, typeResolver);
			return type.AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName[] assemblyNames)
		{
			Type type = ResolveType (typeString, assemblyNames);
			return type.AssemblyQualifiedName;
		}
		#endregion

		object ConvertData (TypeConverter c)
		{
			//FIXME: throw errors when converter not found?
			if (mime_type == ResXResourceWriter.ByteArraySerializedObjectMimeType) {
				if (c.CanConvertFrom (typeof (byte [])))
					return c.ConvertFrom (Convert.FromBase64String (dataString));
			} else if (String.IsNullOrEmpty (mime_type)) {
				if (c.CanConvertFrom (typeof (string)))
					return c.ConvertFromInvariantString (dataString);
			}
			//else
			return null;
		}

	}
}

