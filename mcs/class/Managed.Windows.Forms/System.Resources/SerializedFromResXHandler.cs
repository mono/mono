using System;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

namespace System.Resources {
	internal class SerializedFromResXHandler : ResXDataNodeHandler {

		string dataString;
		string mime_type;
		string typeString;
		object retrievedObject;

		public SerializedFromResXHandler (string data, string _mime_type, string _typeString)
		{
			dataString = data;
			mime_type = _mime_type;
			typeString = _typeString;
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			if (retrievedObject == null)
				retrievedObject = DeserializeObject (typeResolver);

			return retrievedObject;
		}

		public override object GetValue (AssemblyName[] assemblyNames)
		{
			if (retrievedObject == null)
				retrievedObject = DeserializeObject ((ITypeResolutionService) null);

			return retrievedObject;
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			if (retrievedObject == null)
				retrievedObject = DeserializeObject (typeResolver);

			return retrievedObject.GetType ().AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName[] assemblyNames)
		{
			if (retrievedObject == null)
				retrievedObject = DeserializeObject ((ITypeResolutionService) null);

			return retrievedObject.GetType ().AssemblyQualifiedName;
		}
		#endregion

		public override object GetValueForResX ()
		{
			return DeserializeObject ((ITypeResolutionService) null);
		}

		object DeserializeObject (ITypeResolutionService typeResolver)
		{
			object obj;
			if (mime_type == ResXResourceWriter.SoapSerializedObjectMimeType) {
				// FIXME: implement support and test
				throw new NotImplementedException();
			} else if (mime_type == ResXResourceWriter.BinSerializedObjectMimeType) {

				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Binder = new CustomBinder (typeResolver);

				byte [] data = Convert.FromBase64String (dataString);
				using (MemoryStream s = new MemoryStream (data)) {
					obj = formatter.Deserialize (s);
				}

				return obj;

			} else
				throw new Exception("shouldnt get here"); //FIXME:
		}

		sealed class CustomBinder : SerializationBinder 
		{
			ITypeResolutionService typeResolver;

			public CustomBinder (ITypeResolutionService _typeResolver)
			{
				// nulls ok
				typeResolver = _typeResolver;
			}

			public override Type BindToType(string assemblyName, string typeName) 
			{
				Type typeToUse = null;

				string typeString = String.Format("{0}, {1}", typeName, assemblyName);

				if (typeResolver != null)
					typeToUse = typeResolver.GetType (typeString);

				if (typeToUse == null)
					typeToUse = Type.GetType(typeString);

				return typeToUse;
			}
		}
	}
}

