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
			if (ReturnReaderType == null) ReturnReaderType = new MimeFormatterInfo (typeof(XmlReturnReader));
		}
	}
	
	internal class HttpPostTypeStubInfo : HttpSimpleTypeStubInfo
	{
		public HttpPostTypeStubInfo (Type t): base (t)
		{
		}

		protected override MethodStubInfo CreateMethodStubInfo (TypeStubInfo typeInfo, LogicalMethodInfo methodInfo, bool isClientProxy)
		{
			return new HttpPostMethodStubInfo (typeInfo, methodInfo);
		}
	}
}
