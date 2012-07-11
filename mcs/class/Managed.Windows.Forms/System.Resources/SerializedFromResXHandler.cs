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
// Copyright (c) 2012 Gary Barnett
//
// Authors:
//	Gary Barnett

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

