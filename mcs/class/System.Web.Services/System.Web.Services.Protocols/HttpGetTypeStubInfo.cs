//
// HttpGetTypeStubInfo.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
//

namespace System.Web.Services.Protocols 
{
	internal class HttpGetMethodStubInfo : HttpSimpleMethodStubInfo
	{
		public HttpGetMethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source): base (parent, source)
		{
			ParameterReaderType = new MimeFormatterInfo (typeof(UrlParameterReader));
			ReturnWriterType = new MimeFormatterInfo (typeof(XmlReturnWriter));
			
			if (ParameterWriterType == null) ParameterWriterType = new MimeFormatterInfo (typeof(UrlParameterWriter));
		}
	}

	internal class HttpGetTypeStubInfo : HttpSimpleTypeStubInfo
	{
		public HttpGetTypeStubInfo (LogicalTypeInfo logicalTypeInfo): base (logicalTypeInfo)
		{
		}

		public override string ProtocolName
		{
			get { return "HttpGet"; }
		}
		
		protected override MethodStubInfo CreateMethodStubInfo (TypeStubInfo typeInfo, LogicalMethodInfo methodInfo, bool isClientProxy)
		{
			if (isClientProxy && methodInfo.MethodInfo.GetCustomAttributes (typeof(HttpMethodAttribute),true).Length == 0) return null;
			if (!ValueCollectionParameterReader.IsSupported (methodInfo)) return null;
			return new HttpGetMethodStubInfo (typeInfo, methodInfo);
		}
	}
}
