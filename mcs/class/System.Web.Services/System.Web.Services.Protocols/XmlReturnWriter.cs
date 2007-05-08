// 
// System.Web.Services.Protocols.XmlReturnWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

using System.IO;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.Text;

namespace System.Web.Services.Protocols {
	internal class XmlReturnWriter : MimeReturnWriter {

		XmlSerializer serializer;
		
		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			LogicalTypeInfo sti = TypeStubManager.GetLogicalTypeInfo (methodInfo.DeclaringType);
			object[] ats = methodInfo.ReturnTypeCustomAttributeProvider.GetCustomAttributes (typeof(XmlRootAttribute), true);
			XmlRootAttribute root = ats.Length > 0 ? ats[0] as XmlRootAttribute : null; 
			
			XmlReflectionImporter importer = new XmlReflectionImporter ();
			importer.IncludeTypes (methodInfo.CustomAttributeProvider);
			XmlTypeMapping map = importer.ImportTypeMapping (methodInfo.ReturnType, root, sti.GetWebServiceLiteralNamespace (sti.WebServiceNamespace));
			return new XmlSerializer (map);
		}
		
		public override object[] GetInitializers (LogicalMethodInfo[] methodInfos)
		{
			XmlReflectionImporter importer = new XmlReflectionImporter ();
			XmlMapping[] sers = new XmlMapping [methodInfos.Length];
			for (int n=0; n<sers.Length; n++)
			{
				LogicalMethodInfo metinfo = methodInfos[n];
				if (metinfo.IsVoid) 
					sers[n] = null;
				else
				{
					importer.IncludeTypes (metinfo.CustomAttributeProvider);
					LogicalTypeInfo sti = TypeStubManager.GetLogicalTypeInfo (metinfo.DeclaringType);
					object[] ats = methodInfos[n].ReturnTypeCustomAttributeProvider.GetCustomAttributes (typeof(XmlRootAttribute), true);
					XmlRootAttribute root = ats.Length > 0 ? ats[0] as XmlRootAttribute : null; 
					sers[n] = importer.ImportTypeMapping (methodInfos[n].ReturnType, root, sti.GetWebServiceLiteralNamespace (sti.WebServiceNamespace));
				}
			}
			return XmlSerializer.FromMappings (sers);
		}

		public override void Initialize (object initializer) 
		{
			serializer = (XmlSerializer) initializer;
		}

		public override void Write (HttpResponse response, Stream outputStream, object returnValue)
		{
			if (serializer != null)
			{
				response.ContentType = "text/xml; charset=utf-8";
				XmlTextWriter xtw = new XmlTextWriter (outputStream, new UTF8Encoding (false));
				xtw.Formatting = Formatting.Indented;
				serializer.Serialize (xtw, returnValue);
			}
			outputStream.Close ();
		}

		#endregion // Methods
	}
}
