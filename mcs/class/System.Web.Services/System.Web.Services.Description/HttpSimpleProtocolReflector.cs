// 
// System.Web.Services.Description.HttpSimpleProtocolReflector.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
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

using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

namespace System.Web.Services.Description {

	internal abstract class HttpSimpleProtocolReflector : ProtocolReflector 
	{
		#region Fields

		SoapBinding soapBinding;

		#endregion // Fields

		#region Constructors

		public HttpSimpleProtocolReflector ()
		{
		}
		
		#endregion // Constructors

		#region Methods

		protected override void BeginClass ()
		{
			HttpAddressBinding abind = new HttpAddressBinding ();
			abind.Location = ServiceUrl;
			Port.Extensions.Add (abind);
		}

		protected override void EndClass ()
		{
		}

		protected override bool ReflectMethod ()
		{
			LogicalTypeInfo ti = TypeStubManager.GetLogicalTypeInfo (ServiceType);
			HttpOperationBinding sob = new HttpOperationBinding();
			sob.Location = "/" + MethodStubInfo.Name;
			OperationBinding.Extensions.Add (sob);
			
			if (!Method.IsVoid)
			{
				MimeXmlBinding mxb = new MimeXmlBinding ();
				mxb.Part = "Body";
				OperationBinding.Output.Extensions.Add (mxb);
			
				MessagePart part = new MessagePart ();
				part.Name = "Body";	
				
				XmlTypeMapping map = ReflectionImporter.ImportTypeMapping (Method.ReturnType, ti.GetWebServiceLiteralNamespace (ServiceDescription.TargetNamespace));
				XmlQualifiedName qname = new XmlQualifiedName (map.ElementName, map.Namespace);
				part.Element = qname;
				OutputMessage.Parts.Add (part);
				SchemaExporter.ExportTypeMapping (map);
			}
			
			XmlReflectionMember[] mems = new XmlReflectionMember [Method.Parameters.Length];
			for (int n=0; n<Method.Parameters.Length; n++)
			{
				ParameterInfo param = Method.Parameters [n];
				XmlReflectionMember mem = new XmlReflectionMember ();
				mem.MemberName = param.Name;
				Type ptype = param.ParameterType;
				if (ptype.IsByRef) ptype = ptype.GetElementType ();
				mem.MemberType = ptype;
				mems [n] = mem;
			}
			
			XmlMembersMapping memap = ReflectionImporter.ImportMembersMapping ("", ti.WebServiceAbstractNamespace, mems, false);
			bool allPrimitives = true;
			
			for (int n=0; n<memap.Count; n++)
			{
				XmlMemberMapping mem = memap[n];
				MessagePart part = new MessagePart ();
				XmlQualifiedName pqname;
				
				if (mem.TypeNamespace == "") 
					pqname = new XmlQualifiedName (mem.TypeName, XmlSchema.Namespace);
				else {
					pqname = new XmlQualifiedName (mem.TypeName, mem.TypeNamespace); 
					allPrimitives = false; 
				}

				part.Type = pqname;
				part.Name = mem.ElementName;
				InputMessage.Parts.Add (part);
			}

			if (!allPrimitives)
				SoapSchemaExporter.ExportMembersMapping (memap);
			
			return true;
		}
		
		XmlQualifiedName GenerateStringArray ()
		{
			return null;
		}

		protected override string ReflectMethodBinding ()
		{
			return null;
		}

		#endregion
	}
}
