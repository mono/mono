//
// HttpPostTypeStubInfo.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
//

namespace System.Web.Services.Protocols 
{
	internal class HttpPostMethodStubInfo : HttpSimpleMethodStubInfo
	{
		public HttpPostMethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source): base (parent, source)
		{
			ParameterReaderType = new MimeFormatterInfo (typeof(HtmlFormParameterReader));
			ReturnWriterType = new MimeFormatterInfo (typeof(XmlReturnWriter));

			if (ParameterWriterType == null) ParameterWriterType = new MimeFormatterInfo (typeof(HtmlFormParameterWriter));
		}
	}
	
	internal class HttpPostTypeStubInfo : HttpSimpleTypeStubInfo
	{
		public HttpPostTypeStubInfo (LogicalTypeInfo logicalTypeInfo): base (logicalTypeInfo)
		{
		}

		public override string ProtocolName
		{
			get { return "HttpPost"; }
		}
		
		protected override MethodStubInfo CreateMethodStubInfo (TypeStubInfo typeInfo, LogicalMethodInfo methodInfo, bool isClientProxy)
		{
			if (isClientProxy && methodInfo.MethodInfo.GetCustomAttributes (typeof(HttpMethodAttribute),true).Length == 0) return null;
			if (!ValueCollectionParameterReader.IsSupported (methodInfo)) return null;
			return new HttpPostMethodStubInfo (typeInfo, methodInfo);
		}
	}
}
