//
// Methods.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Miguel de Icaza
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

using HeaderInfo = System.Web.Services.Protocols.SoapHeaderMapping;

using System.Reflection;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols {

	//
	// This class represents all the information we extract from a MethodInfo
	// in the SoapHttpClientProtocol derivative stub class
	//
	internal class SoapMethodStubInfo : MethodStubInfo
	{
		internal readonly string Action;
		internal readonly string Binding;

		// The name/namespace of the request 
		internal readonly string RequestName;
		internal readonly string RequestNamespace;

		// The name/namespace of the response.
		internal readonly string ResponseName;
		internal readonly string ResponseNamespace;

		internal readonly bool OneWay;
		internal readonly SoapParameterStyle ParameterStyle;
		internal readonly SoapBindingStyle SoapBindingStyle;
		internal readonly SoapBindingUse Use;

		internal readonly HeaderInfo [] Headers;
		internal readonly HeaderInfo [] InHeaders;
		internal readonly HeaderInfo [] OutHeaders;
		internal readonly HeaderInfo [] FaultHeaders;
		internal readonly SoapExtensionRuntimeConfig [] SoapExtensions;

		internal readonly XmlMembersMapping InputMembersMapping;
		internal readonly XmlMembersMapping OutputMembersMapping;
		internal readonly XmlMembersMapping InputHeaderMembersMapping;
		internal readonly XmlMembersMapping OutputHeaderMembersMapping;
		internal readonly XmlMembersMapping FaultHeaderMembersMapping;

		private readonly int requestSerializerId;
		private readonly int responseSerializerId;
		private readonly int requestHeadersSerializerId = -1;
		private readonly int responseHeadersSerializerId = -1;
		private readonly int faultHeadersSerializerId = -1;
		
		internal XmlSerializer RequestSerializer
		{
			get { return TypeStub.GetSerializer (requestSerializerId); }
		}
		
		internal XmlSerializer ResponseSerializer
		{
			get { return TypeStub.GetSerializer (responseSerializerId); }
		}
		
		internal XmlSerializer RequestHeadersSerializer
		{
			get { return requestHeadersSerializerId != -1 ? TypeStub.GetSerializer (requestHeadersSerializerId) : null; }
		}
		
		internal XmlSerializer ResponseHeadersSerializer
		{
			get { return responseHeadersSerializerId != -1 ? TypeStub.GetSerializer (responseHeadersSerializerId) : null; }
		}
		
		internal XmlSerializer FaultHeadersSerializer
		{
			get { return faultHeadersSerializerId != -1 ? TypeStub.GetSerializer (faultHeadersSerializerId) : null; }
		}
		

		//
		// Constructor
		//
		public SoapMethodStubInfo (TypeStubInfo typeStub, LogicalMethodInfo source, object kind, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter)
		: base (typeStub, source)
		{
			SoapTypeStubInfo parent = (SoapTypeStubInfo) typeStub;
			XmlElementAttribute optional_ns = null;

			if (kind == null) {
				Use = parent.LogicalType.BindingUse;
				RequestName = "";
				RequestNamespace = "";
				ResponseName = "";
				ResponseNamespace = "";
				ParameterStyle = parent.ParameterStyle;
				SoapBindingStyle = parent.SoapBindingStyle;
				OneWay = false;
// disabled (see bug #332150)
//#if NET_2_0
//				if (parent.Type != source.DeclaringType)
//					Binding = source.DeclaringType.Name + parent.ProtocolName;
//#endif
			}
			else if (kind is SoapDocumentMethodAttribute){
				SoapDocumentMethodAttribute dma = (SoapDocumentMethodAttribute) kind;
				
				Use = dma.Use;
				if (Use == SoapBindingUse.Default) {
					if (parent.SoapBindingStyle == SoapBindingStyle.Document)
						Use = parent.LogicalType.BindingUse;
					else
						Use = SoapBindingUse.Literal;
				}
				
				Action = dma.Action;
				Binding = dma.Binding;
				RequestName = dma.RequestElementName;
				RequestNamespace = dma.RequestNamespace;
				ResponseName = dma.ResponseElementName;
				ResponseNamespace = dma.ResponseNamespace;
				ParameterStyle = dma.ParameterStyle;
				if (ParameterStyle == SoapParameterStyle.Default)
					ParameterStyle = parent.ParameterStyle;
				OneWay = dma.OneWay;
				SoapBindingStyle = SoapBindingStyle.Document;
			} else {
				SoapRpcMethodAttribute rma = (SoapRpcMethodAttribute) kind;
				Use = SoapBindingUse.Encoded;	// RPC always use encoded

				Action = rma.Action;
				if (Action != null && Action.Length == 0)
					Action = null;
				Binding = rma.Binding;
				
				// When using RPC, MS.NET seems to ignore RequestElementName and
				// MessageName, and it always uses the method name
				RequestName = source.Name;
				ResponseName = source.Name + "Response";
//				RequestName = rma.RequestElementName;
//				ResponseName = rma.ResponseElementName;
				RequestNamespace = rma.RequestNamespace;
				ResponseNamespace = rma.ResponseNamespace;
				ParameterStyle = SoapParameterStyle.Wrapped;
				OneWay = rma.OneWay;
				SoapBindingStyle = SoapBindingStyle.Rpc;

				// For RPC calls, make all arguments be part of the empty namespace
				optional_ns = new XmlElementAttribute ();
				optional_ns.Namespace = "";
			}

			if (OneWay){
				if (source.ReturnType != typeof (void))
					throw new Exception ("OneWay methods should not have a return value.");
				if (source.OutParameters.Length != 0)
					throw new Exception ("OneWay methods should not have out/ref parameters.");
			}
			
			BindingInfo binfo = parent.GetBinding (Binding);
			if (binfo == null) throw new InvalidOperationException ("Type '" + parent.Type + "' is missing WebServiceBinding attribute that defines a binding named '" + Binding + "'.");
			
			string serviceNamespace = binfo.Namespace;
				
			if (RequestNamespace == "") RequestNamespace = parent.LogicalType.GetWebServiceNamespace (serviceNamespace, Use);
			if (ResponseNamespace == "") ResponseNamespace = parent.LogicalType.GetWebServiceNamespace (serviceNamespace, Use);
			if (RequestName == "") RequestName = Name;
			if (ResponseName == "")	ResponseName = Name + "Response";
			if (Action == null)
				Action = serviceNamespace.EndsWith("/") ? (serviceNamespace + Name) : (serviceNamespace + "/" + Name);
			
			bool hasWrappingElem = (ParameterStyle == SoapParameterStyle.Wrapped);
			bool writeAccessors = (SoapBindingStyle == SoapBindingStyle.Rpc);
			
			XmlReflectionMember [] in_members = BuildRequestReflectionMembers (optional_ns);
			XmlReflectionMember [] out_members = BuildResponseReflectionMembers (optional_ns);

			if (Use == SoapBindingUse.Literal) {
				xmlImporter.IncludeTypes (source.CustomAttributeProvider);
				InputMembersMapping = xmlImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, hasWrappingElem);
				OutputMembersMapping = xmlImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, hasWrappingElem);
			}
			else {
				soapImporter.IncludeTypes (source.CustomAttributeProvider);
				InputMembersMapping = soapImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, hasWrappingElem, writeAccessors);
				OutputMembersMapping = soapImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, hasWrappingElem, writeAccessors);
			}

			requestSerializerId = parent.RegisterSerializer (InputMembersMapping);
			responseSerializerId = parent.RegisterSerializer (OutputMembersMapping);

			object[] o = source.GetCustomAttributes (typeof (SoapHeaderAttribute));
			ArrayList allHeaderList = new ArrayList (o.Length);
			ArrayList inHeaderList = new ArrayList (o.Length);
			ArrayList outHeaderList = new ArrayList (o.Length);
			ArrayList faultHeaderList = new ArrayList ();
			
			SoapHeaderDirection unknownHeaderDirections = (SoapHeaderDirection)0;
			
			for (int i = 0; i < o.Length; i++) {
				SoapHeaderAttribute att = (SoapHeaderAttribute) o[i];
				MemberInfo[] mems = source.DeclaringType.GetMember (att.MemberName);
				if (mems.Length == 0) throw new InvalidOperationException ("Member " + att.MemberName + " not found in class " + source.DeclaringType.FullName + ".");
				
				HeaderInfo header = new HeaderInfo (mems[0], att);
				allHeaderList.Add (header);
				if (!header.Custom) {
					if ((header.Direction & SoapHeaderDirection.In) != 0)
						inHeaderList.Add (header);
					if ((header.Direction & SoapHeaderDirection.Out) != 0)
						outHeaderList.Add (header);
					if ((header.Direction & SoapHeaderDirection.Fault) != 0)
						faultHeaderList.Add (header);
				} else
					unknownHeaderDirections |= header.Direction;
			}
			
			Headers = (HeaderInfo[]) allHeaderList.ToArray (typeof(HeaderInfo));

			if (inHeaderList.Count > 0 || (unknownHeaderDirections & SoapHeaderDirection.In) != 0) {
				InHeaders = (HeaderInfo[]) inHeaderList.ToArray (typeof(HeaderInfo));
				XmlReflectionMember[] members = BuildHeadersReflectionMembers (InHeaders);
				
				if (Use == SoapBindingUse.Literal)
					InputHeaderMembersMapping = xmlImporter.ImportMembersMapping ("", RequestNamespace, members, false);
				else
					InputHeaderMembersMapping = soapImporter.ImportMembersMapping ("", RequestNamespace, members, false, false);
				
				requestHeadersSerializerId = parent.RegisterSerializer (InputHeaderMembersMapping);
			}
			
			if (outHeaderList.Count > 0 || (unknownHeaderDirections & SoapHeaderDirection.Out) != 0) {
				OutHeaders = (HeaderInfo[]) outHeaderList.ToArray (typeof(HeaderInfo));
				XmlReflectionMember[] members = BuildHeadersReflectionMembers (OutHeaders);
				
				if (Use == SoapBindingUse.Literal)
					OutputHeaderMembersMapping = xmlImporter.ImportMembersMapping ("", RequestNamespace, members, false);
				else
					OutputHeaderMembersMapping = soapImporter.ImportMembersMapping ("", RequestNamespace, members, false, false);
				
				responseHeadersSerializerId = parent.RegisterSerializer (OutputHeaderMembersMapping);
			}
			
			if (faultHeaderList.Count > 0 || (unknownHeaderDirections & SoapHeaderDirection.Fault) != 0) {
				FaultHeaders = (HeaderInfo[]) faultHeaderList.ToArray (typeof(HeaderInfo));
				XmlReflectionMember[] members = BuildHeadersReflectionMembers (FaultHeaders);
				
				if (Use == SoapBindingUse.Literal)
					FaultHeaderMembersMapping = xmlImporter.ImportMembersMapping ("", RequestNamespace, members, false);
				else
					FaultHeaderMembersMapping = soapImporter.ImportMembersMapping ("", RequestNamespace, members, false, false);
				
				faultHeadersSerializerId = parent.RegisterSerializer (FaultHeaderMembersMapping);
			}
			
			SoapExtensions = SoapExtension.GetMethodExtensions (source);
		}

		XmlReflectionMember [] BuildRequestReflectionMembers (XmlElementAttribute optional_ns)
		{
			ParameterInfo [] input = MethodInfo.InParameters;
			XmlReflectionMember [] in_members = new XmlReflectionMember [input.Length];

			for (int i = 0; i < input.Length; i++)
			{
				XmlReflectionMember m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = input [i].Name;
				m.MemberType = input [i].ParameterType;

				m.XmlAttributes = new XmlAttributes (input[i]);
				m.SoapAttributes = new SoapAttributes (input[i]);

				if (m.MemberType.IsByRef)
					m.MemberType = m.MemberType.GetElementType ();
				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				in_members [i] = m;
			}
			return in_members;
		}
		
		XmlReflectionMember [] BuildResponseReflectionMembers (XmlElementAttribute optional_ns)
		{
			ParameterInfo [] output = MethodInfo.OutParameters;
			bool has_return_value = !(OneWay || MethodInfo.ReturnType == typeof (void));
			XmlReflectionMember [] out_members = new XmlReflectionMember [(has_return_value ? 1 : 0) + output.Length];
			XmlReflectionMember m;
			int idx = 0;

			if (has_return_value)
			{
				m = new XmlReflectionMember ();
				m.IsReturnValue = true;
				m.MemberName = Name + "Result";
				m.MemberType = MethodInfo.ReturnType;

				m.XmlAttributes = new XmlAttributes (MethodInfo.ReturnTypeCustomAttributeProvider);
				m.SoapAttributes = new SoapAttributes (MethodInfo.ReturnTypeCustomAttributeProvider);

				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				idx++;
				out_members [0] = m;
			}
			
			for (int i = 0; i < output.Length; i++)
			{
				m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = output [i].Name;
				m.MemberType = output [i].ParameterType;
				m.XmlAttributes = new XmlAttributes (output[i]);
				m.SoapAttributes = new SoapAttributes (output[i]);

				if (m.MemberType.IsByRef)
					m.MemberType = m.MemberType.GetElementType ();
				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				out_members [i + idx] = m;
			}
			return out_members;
		}

		XmlReflectionMember [] BuildHeadersReflectionMembers (HeaderInfo[] headers)
		{
			XmlReflectionMember [] mems = new XmlReflectionMember [headers.Length];

			for (int n=0; n<headers.Length; n++)
			{
				HeaderInfo header = headers [n];
				
				XmlReflectionMember m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = header.HeaderType.Name;
				m.MemberType = header.HeaderType;

				// MS.NET reflects header classes in a weird way. The root element
				// name is the CLR class name unless it is specified in an XmlRootAttribute.
				// The usual is to use the xml type name by default, but not in this case.
				
				XmlAttributes ats = new XmlAttributes (header.HeaderType);
				if (ats.XmlRoot != null) {
					XmlElementAttribute xe = new XmlElementAttribute ();
					xe.ElementName = ats.XmlRoot.ElementName;
					xe.Namespace = ats.XmlRoot.Namespace;
					m.XmlAttributes = new XmlAttributes ();
					m.XmlAttributes.XmlElements.Add (xe);
				}
				
				mems [n] = m;
			}
			return mems;
		}

		public HeaderInfo GetHeaderInfo (Type headerType)
		{
			foreach (HeaderInfo headerInfo in Headers)
				if (headerInfo.HeaderType == headerType) return headerInfo;
			return null;
		}
		
		public XmlSerializer GetBodySerializer (SoapHeaderDirection dir, bool soap12)
		{
			switch (dir) {
				case SoapHeaderDirection.In: return RequestSerializer;
				case SoapHeaderDirection.Out: return ResponseSerializer;
				case SoapHeaderDirection.Fault: return soap12 ? Soap12Fault.Serializer : Fault.Serializer;
				default: return null;
			}
		}
		
		public XmlSerializer GetHeaderSerializer (SoapHeaderDirection dir)
		{
			switch (dir) {
				case SoapHeaderDirection.In: return RequestHeadersSerializer;
				case SoapHeaderDirection.Out: return ResponseHeadersSerializer;
				case SoapHeaderDirection.Fault: return FaultHeadersSerializer;
				default: return null;
			}
		}
		
		HeaderInfo[] GetHeaders (SoapHeaderDirection dir)
		{
			switch (dir) {
				case SoapHeaderDirection.In: return InHeaders;
				case SoapHeaderDirection.Out: return OutHeaders;
				case SoapHeaderDirection.Fault: return FaultHeaders;
				default: return null;
			}
		}
		
		public object[] GetHeaderValueArray (SoapHeaderDirection dir, SoapHeaderCollection headers)
		{
			HeaderInfo[] headerInfos = GetHeaders (dir);
			if (headerInfos == null) return null;

			object[] hs = new object [headerInfos.Length];
			
			for (int n=0; n<headers.Count; n++) {
				SoapHeader h = headers[n];
				Type t = h.GetType();
				for (int i=0; i<headerInfos.Length; i++)
					if (headerInfos [i].HeaderType == t)
						hs [i] = h;
			}
			return hs;
		}
	}

	//
	// Holds the metadata loaded from the type stub, as well as
	// the metadata for all the methods in the type
	//
	internal class SoapTypeStubInfo : TypeStubInfo
	{
		Hashtable methods_byaction = new Hashtable (); 

		// Precomputed
		internal SoapParameterStyle      ParameterStyle;
		internal SoapExtensionRuntimeConfig[][] SoapExtensions;
		internal SoapBindingStyle SoapBindingStyle;
		internal XmlReflectionImporter 	xmlImporter;
		internal SoapReflectionImporter soapImporter;

		public SoapTypeStubInfo (LogicalTypeInfo logicalTypeInfo)
		: base (logicalTypeInfo)
		{
			xmlImporter = new XmlReflectionImporter ();
			soapImporter = new SoapReflectionImporter ();
				
			if (typeof (SoapHttpClientProtocol).IsAssignableFrom (Type))
			{
				if (Bindings.Count == 0 || ((BindingInfo)Bindings[0]).WebServiceBindingAttribute == null)
					throw new InvalidOperationException ("WebServiceBindingAttribute is required on proxy class '" + Type + "'.");
				if (Bindings.Count > 1)
					throw new InvalidOperationException ("Only one WebServiceBinding attribute may be specified on type '" + Type + "'.");
			}

			object [] o = Type.GetCustomAttributes (typeof (SoapDocumentServiceAttribute), false);
			if (o.Length == 1){
				SoapDocumentServiceAttribute a = (SoapDocumentServiceAttribute) o [0];

				ParameterStyle = a.ParameterStyle;
				SoapBindingStyle = SoapBindingStyle.Document;
			} else {
				o = Type.GetCustomAttributes (typeof (SoapRpcServiceAttribute), false);
				if (o.Length == 1){
					ParameterStyle = SoapParameterStyle.Wrapped;
					SoapBindingStyle = SoapBindingStyle.Rpc;
				} else {
					ParameterStyle = SoapParameterStyle.Wrapped;
					SoapBindingStyle = SoapBindingStyle.Document;
				}
			}
			
			if (ParameterStyle == SoapParameterStyle.Default) ParameterStyle = SoapParameterStyle.Wrapped;
			
			xmlImporter.IncludeTypes (Type);
			soapImporter.IncludeTypes (Type);

#if MOBILE
			SoapExtensions = new SoapExtensionRuntimeConfig [2][];
#else
			SoapExtensions = SoapExtension.GetTypeExtensions (Type);
#endif
		}

		internal SoapServiceRoutingStyle RoutingStyle {
			get { return LogicalType.RoutingStyle; }
		}

		public override XmlReflectionImporter XmlImporter 
		{
			get { return xmlImporter; }
		}

		public override SoapReflectionImporter SoapImporter 
		{
			get { return soapImporter; }
		}
		
		public override string ProtocolName
		{
			get { return "Soap"; }
		}
		
		protected override MethodStubInfo CreateMethodStubInfo (TypeStubInfo parent, LogicalMethodInfo lmi, bool isClientProxy)
		{
			SoapMethodStubInfo res = null;
			object [] ats = lmi.GetCustomAttributes (typeof (SoapDocumentMethodAttribute));
			if (ats.Length == 0) ats = lmi.GetCustomAttributes (typeof (SoapRpcMethodAttribute));

			if (ats.Length == 0 && isClientProxy)
				return null;
			else if (ats.Length == 0)
				res = new SoapMethodStubInfo (parent, lmi, null, xmlImporter, soapImporter);
			else
				res = new SoapMethodStubInfo (parent, lmi, ats[0], xmlImporter, soapImporter);
				
			methods_byaction [res.Action] = res;
			return res;
		}
		
		public SoapMethodStubInfo GetMethodForSoapAction (string name)
		{
			return (SoapMethodStubInfo) methods_byaction [name.Trim ('"',' ')];
		}
	}

	internal class Soap12TypeStubInfo : SoapTypeStubInfo
	{
		public Soap12TypeStubInfo (LogicalTypeInfo logicalTypeInfo)
		: base (logicalTypeInfo)
		{
		}

		public override string ProtocolName
		{
			get { return "Soap12"; }
		}
	}
}
