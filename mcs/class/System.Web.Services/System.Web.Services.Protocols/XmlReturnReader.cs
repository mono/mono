// 
// System.Web.Services.Protocols.XmlReturnReader.cs
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
using System.Net;
using System.Xml.Serialization;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class XmlReturnReader : MimeReturnReader {

		XmlSerializer serializer;
		
		#region Constructors

		public XmlReturnReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			LogicalTypeInfo sti = TypeStubManager.GetLogicalTypeInfo (methodInfo.DeclaringType);
			object[] ats = methodInfo.ReturnTypeCustomAttributeProvider.GetCustomAttributes (typeof(XmlRootAttribute), true);
			XmlRootAttribute root = ats.Length > 0 ? ats[0] as XmlRootAttribute : null; 
			return new XmlSerializer (methodInfo.ReturnType, null, null, root, sti.GetWebServiceLiteralNamespace (sti.WebServiceNamespace));
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
					LogicalTypeInfo sti = TypeStubManager.GetLogicalTypeInfo (metinfo.DeclaringType);
					object[] ats = methodInfos[n].ReturnTypeCustomAttributeProvider.GetCustomAttributes (typeof(XmlRootAttribute), true);
					XmlRootAttribute root = ats.Length > 0 ? ats[0] as XmlRootAttribute : null; 
					sers[n] = importer.ImportTypeMapping (methodInfos[n].ReturnType, root, sti.GetWebServiceLiteralNamespace (sti.WebServiceNamespace));
				}
			}
			return XmlSerializer.FromMappings (sers);
		}
		
		public override void Initialize (object o)
		{
			serializer = (XmlSerializer)o;
		}

		public override object Read (WebResponse response, Stream responseStream)
		{
			object result = null;
			if (serializer != null)
			{
				if (response.ContentType.IndexOf ("text/xml") == -1)
					throw new InvalidOperationException ("Result was not XML");
				
				result = serializer.Deserialize (responseStream);
			}
			responseStream.Close ();
			return result;
		}

		#endregion // Methods
	}
}
