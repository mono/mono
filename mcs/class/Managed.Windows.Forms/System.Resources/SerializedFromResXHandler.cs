//
// SerializedFromResXHandler.cs : Handles a resource that was stored in a
// resx file by means of serialization.
// 
// Author:
//	Gary Barnett (gary.barnett.mono@gmail.com)
// 
// Copyright (C) Gary Barnett (2012)
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

using System;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Soap;

namespace System.Resources {
	internal class SerializedFromResXHandler : ResXDataNodeHandler, IWritableHandler {

		string dataString;
		string mime_type;
		CustomBinder binder; // so type set after first call

		public SerializedFromResXHandler (string data, string _mime_type)
		{
			dataString = data;
			mime_type = _mime_type;
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			return DeserializeObject (typeResolver);
		}

		public override object GetValue (AssemblyName [] assemblyNames)
		{
			return DeserializeObject ((ITypeResolutionService) null);
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			return InternalGetValueType (typeResolver);
		}

		public override string GetValueTypeName (AssemblyName [] assemblyNames)
		{
			return InternalGetValueType (null);
		}
		#endregion

		#region IWritableHandler implementation
		public string DataString {
			get {
				return dataString;
			}
		}
		#endregion

		string InternalGetValueType (ITypeResolutionService typeResolver)
		{
			object retrievedObject;
			try {
				retrievedObject = DeserializeObject (typeResolver);
			} catch {
				return typeof (object).AssemblyQualifiedName;
			}

			if (retrievedObject == null)
				return null;
			else
				return retrievedObject.GetType ().AssemblyQualifiedName;
		}

		object DeserializeObject (ITypeResolutionService typeResolver)
		{
			try {
				if (mime_type == ResXResourceWriter.SoapSerializedObjectMimeType) {
					//FIXME: theres a test in the suite to check that a type converter converts from invariant string
					//do i need to take the string culture into consideration here?
					SoapFormatter soapF = new SoapFormatter ();
					if (binder == null)
						binder = new CustomBinder (typeResolver);
					soapF.Binder = binder;
					byte [] data = Convert.FromBase64String (dataString);
					using (MemoryStream s = new MemoryStream (data)) {
						return soapF.Deserialize (s);
					}
				} else if (mime_type == ResXResourceWriter.BinSerializedObjectMimeType) {
					BinaryFormatter binF = new BinaryFormatter ();
					if (binder == null)
						binder = new CustomBinder (typeResolver);
					binF.Binder = binder;
					byte [] data = Convert.FromBase64String (dataString);
					using (MemoryStream s = new MemoryStream (data)) {
						return binF.Deserialize (s);
					}
				} else // invalid mime_type
					return null; 
			} catch (SerializationException ex) { 
				if (ex.Message.StartsWith ("Couldn't find assembly"))
					throw new ArgumentException (ex.Message);
				else
					throw ex;
			}
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

