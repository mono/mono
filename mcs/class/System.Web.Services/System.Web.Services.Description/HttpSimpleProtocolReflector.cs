// 
// System.Web.Services.Description.HttpSimpleProtocolReflector.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
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
			sob.Location = "/" + OperationBinding.Name;
			OperationBinding.Extensions.Add (sob);
			
			if (!Method.IsVoid)
			{
				MimeXmlBinding mxb = new MimeXmlBinding ();
				mxb.Part = "Body";
				OperationBinding.Output.Extensions.Add (mxb);
			
				MessagePart part = new MessagePart ();
				part.Name = "Body";	
				
				XmlTypeMapping map = ReflectionImporter.ImportTypeMapping (Method.ReturnType, ti.WebServiceLiteralNamespace);
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
			return TypeInfo.DefaultBinding;
		}

		#endregion
	}
}
