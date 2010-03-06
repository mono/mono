#if NET_4_0
using System;
using System.Collections.Generic;

using StandAloneTests.RequestValidator;

namespace StandAloneTests.RequestValidator.Generated
{
	class RequestValidatorCallSet000 : RequestValidatorCallSet
	{
		Dictionary <string, object> callSet000 = new Dictionary <string, object> ();
		void CreateCallSet000 ()
		{
			callSet000.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_RawUrl()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 31\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_PathInfo()\n   at System.Web.Script.Services.RestHandlerFactory.IsRestMethodCall(HttpRequest request)\n   at System.Web.Handlers.ScriptModule.OnPostAcquireRequestState(Object sender, EventArgs eventArgs)\n   at System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet000.Add ("rawUrl", "/Default.aspx");
			callSet000.Add ("context", true);
			callSet000.Add ("value", "/Default.aspx");
			callSet000.Add ("requestValidationSource", 4);
			callSet000.Add ("collectionKey", null);
			callSet000.Add ("returnValue", true);
			callSet000.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet001 = new Dictionary <string, object> ();
		void CreateCallSet001 ()
		{
			callSet001.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_PathInfo()\n   at System.Web.Script.Services.RestHandlerFactory.IsRestMethodCall(HttpRequest request)\n   at System.Web.Handlers.ScriptModule.OnPostAcquireRequestState(Object sender, EventArgs eventArgs)\n   at System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet001.Add ("rawUrl", "/Default.aspx");
			callSet001.Add ("context", true);
			callSet001.Add ("value", "");
			callSet001.Add ("requestValidationSource", 6);
			callSet001.Add ("collectionKey", null);
			callSet001.Add ("returnValue", true);
			callSet001.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet002 = new Dictionary <string, object> ();
		void CreateCallSet002 ()
		{
			callSet002.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet002.Add ("rawUrl", "/Default.aspx");
			callSet002.Add ("context", true);
			callSet002.Add ("value", "keep-alive");
			callSet002.Add ("requestValidationSource", 7);
			callSet002.Add ("collectionKey", "Connection");
			callSet002.Add ("returnValue", true);
			callSet002.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet003 = new Dictionary <string, object> ();
		void CreateCallSet003 ()
		{
			callSet003.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet003.Add ("rawUrl", "/Default.aspx");
			callSet003.Add ("context", true);
			callSet003.Add ("value", "300");
			callSet003.Add ("requestValidationSource", 7);
			callSet003.Add ("collectionKey", "Keep-Alive");
			callSet003.Add ("returnValue", true);
			callSet003.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet004 = new Dictionary <string, object> ();
		void CreateCallSet004 ()
		{
			callSet004.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet004.Add ("rawUrl", "/Default.aspx");
			callSet004.Add ("context", true);
			callSet004.Add ("value", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			callSet004.Add ("requestValidationSource", 7);
			callSet004.Add ("collectionKey", "Accept");
			callSet004.Add ("returnValue", true);
			callSet004.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet005 = new Dictionary <string, object> ();
		void CreateCallSet005 ()
		{
			callSet005.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet005.Add ("rawUrl", "/Default.aspx");
			callSet005.Add ("context", true);
			callSet005.Add ("value", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
			callSet005.Add ("requestValidationSource", 7);
			callSet005.Add ("collectionKey", "Accept-Charset");
			callSet005.Add ("returnValue", true);
			callSet005.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet006 = new Dictionary <string, object> ();
		void CreateCallSet006 ()
		{
			callSet006.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet006.Add ("rawUrl", "/Default.aspx");
			callSet006.Add ("context", true);
			callSet006.Add ("value", "gzip,deflate");
			callSet006.Add ("requestValidationSource", 7);
			callSet006.Add ("collectionKey", "Accept-Encoding");
			callSet006.Add ("returnValue", true);
			callSet006.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet007 = new Dictionary <string, object> ();
		void CreateCallSet007 ()
		{
			callSet007.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet007.Add ("rawUrl", "/Default.aspx");
			callSet007.Add ("context", true);
			callSet007.Add ("value", "en-us,en;q=0.5");
			callSet007.Add ("requestValidationSource", 7);
			callSet007.Add ("collectionKey", "Accept-Language");
			callSet007.Add ("returnValue", true);
			callSet007.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet008 = new Dictionary <string, object> ();
		void CreateCallSet008 ()
		{
			callSet008.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet008.Add ("rawUrl", "/Default.aspx");
			callSet008.Add ("context", true);
			callSet008.Add ("value", "localhost:1383");
			callSet008.Add ("requestValidationSource", 7);
			callSet008.Add ("collectionKey", "Host");
			callSet008.Add ("returnValue", true);
			callSet008.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet009 = new Dictionary <string, object> ();
		void CreateCallSet009 ()
		{
			callSet009.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Headers()\n   at System.Web.Configuration.BrowserCapabilitiesFactoryBase.GetHttpBrowserCapabilities(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.EvaluateFinal(HttpRequest request, Boolean onlyEvaluateUserAgent)\n   at System.Web.Configuration.HttpCapabilitiesDefaultProvider.Evaluate(HttpRequest request)\n   at System.Web.Configuration.HttpCapabilitiesBase.GetBrowserCapabilities(HttpRequest request)\n   at System.Web.HttpRequest.get_Browser()\n   at System.Web.UI.Page.SetIntrinsics(HttpContext context, Boolean allowAsync)\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet009.Add ("rawUrl", "/Default.aspx");
			callSet009.Add ("context", true);
			callSet009.Add ("value", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.8) Gecko/20100202 Firefox/3.5.8");
			callSet009.Add ("requestValidationSource", 7);
			callSet009.Add ("collectionKey", "User-Agent");
			callSet009.Add ("returnValue", true);
			callSet009.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet010 = new Dictionary <string, object> ();
		void CreateCallSet010 ()
		{
			callSet010.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Path()\n   at System.Web.HttpResponse.AddVirtualPathDependencies(String[] virtualPaths)\n   at System.Web.UI.Page.AddWrappedFileDependencies(Object virtualFileDependencies)\n   at ASP.default_aspx.FrameworkInitialize() in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\Default.aspx.cs:line 912308\n   at System.Web.UI.Page.ProcessRequest(Boolean includeStagesBeforeAsyncPoint, Boolean includeStagesAfterAsyncPoint)\n   at System.Web.UI.Page.ProcessRequest()\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet010.Add ("rawUrl", "/Default.aspx");
			callSet010.Add ("context", true);
			callSet010.Add ("value", "/Default.aspx");
			callSet010.Add ("requestValidationSource", 5);
			callSet010.Add ("collectionKey", null);
			callSet010.Add ("returnValue", true);
			callSet010.Add ("validationFailureIndex", 0);
		}

		public RequestValidatorCallSet000 ()
		{
			CreateCallSet000 ();
			RegisterCallSet (callSet000);
			CreateCallSet001 ();
			RegisterCallSet (callSet001);
			CreateCallSet002 ();
			RegisterCallSet (callSet002);
			CreateCallSet003 ();
			RegisterCallSet (callSet003);
			CreateCallSet004 ();
			RegisterCallSet (callSet004);
			CreateCallSet005 ();
			RegisterCallSet (callSet005);
			CreateCallSet006 ();
			RegisterCallSet (callSet006);
			CreateCallSet007 ();
			RegisterCallSet (callSet007);
			CreateCallSet008 ();
			RegisterCallSet (callSet008);
			CreateCallSet009 ();
			RegisterCallSet (callSet009);
			CreateCallSet010 ();
			RegisterCallSet (callSet010);
			Name = "000";
			RequestValidatorCallSetContainer.Register (this);
		}
	}
	class RequestValidatorCallSet001 : RequestValidatorCallSet
	{
		Dictionary <string, object> callSet000 = new Dictionary <string, object> ();
		void CreateCallSet000 ()
		{
			callSet000.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_RawUrl()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 31\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_PathInfo()\n   at System.Web.Script.Services.RestHandlerFactory.IsRestMethodCall(HttpRequest request)\n   at System.Web.Handlers.ScriptModule.OnPostAcquireRequestState(Object sender, EventArgs eventArgs)\n   at System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet000.Add ("rawUrl", "/Default.aspx?key=invalid%3Cscript%3Evalue%3C/script%3E");
			callSet000.Add ("context", true);
			callSet000.Add ("value", "/Default.aspx?key=invalid%3Cscript%3Evalue%3C/script%3E");
			callSet000.Add ("requestValidationSource", 4);
			callSet000.Add ("collectionKey", null);
			callSet000.Add ("returnValue", true);
			callSet000.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet001 = new Dictionary <string, object> ();
		void CreateCallSet001 ()
		{
			callSet001.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_PathInfo()\n   at System.Web.Script.Services.RestHandlerFactory.IsRestMethodCall(HttpRequest request)\n   at System.Web.Handlers.ScriptModule.OnPostAcquireRequestState(Object sender, EventArgs eventArgs)\n   at System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet001.Add ("rawUrl", "/Default.aspx?key=invalid%3Cscript%3Evalue%3C/script%3E");
			callSet001.Add ("context", true);
			callSet001.Add ("value", "");
			callSet001.Add ("requestValidationSource", 6);
			callSet001.Add ("collectionKey", null);
			callSet001.Add ("returnValue", true);
			callSet001.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet002 = new Dictionary <string, object> ();
		void CreateCallSet002 ()
		{
			callSet002.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_Path()\n   at System.Web.HttpResponse.AddVirtualPathDependencies(String[] virtualPaths)\n   at System.Web.UI.Page.AddWrappedFileDependencies(Object virtualFileDependencies)\n   at ASP.default_aspx.FrameworkInitialize() in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\Default.aspx.cs:line 912308\n   at System.Web.UI.Page.ProcessRequest(Boolean includeStagesBeforeAsyncPoint, Boolean includeStagesAfterAsyncPoint)\n   at System.Web.UI.Page.ProcessRequest()\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet002.Add ("rawUrl", "/Default.aspx?key=invalid%3Cscript%3Evalue%3C/script%3E");
			callSet002.Add ("context", true);
			callSet002.Add ("value", "/Default.aspx");
			callSet002.Add ("requestValidationSource", 5);
			callSet002.Add ("collectionKey", null);
			callSet002.Add ("returnValue", true);
			callSet002.Add ("validationFailureIndex", 0);
		}

		Dictionary <string, object> callSet003 = new Dictionary <string, object> ();
		void CreateCallSet003 ()
		{
			callSet003.Add ("calledFrom", "   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\n   at System.Environment.get_StackTrace()\n   at Tests.TestRequestValidator.IsValidRequestString(HttpContext context, String value, RequestValidationSource requestValidationSource, String collectionKey, Int32& validationFailureIndex) in c:\\Users\\grendel\\Documents\\Visual Studio 2010\\Websites\\ExtensibleRequestValidation_01\\App_Code\\TestRequestValidator.cs:line 30\n   at System.Web.HttpRequest.ValidateString(String value, String collectionKey, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.ValidateNameValueCollection(NameValueCollection nvc, RequestValidationSource requestCollection)\n   at System.Web.HttpRequest.get_QueryString()\n   at System.Web.UI.Page.GetCollectionBasedOnMethod(Boolean dontReturnNull)\n   at System.Web.UI.Page.DeterminePostBackMode()\n   at System.Web.UI.Page.ProcessRequestMain(Boolean includeStagesBeforeAsyncPoint, Boolean includeStagesAfterAsyncPoint)\n   at System.Web.UI.Page.ProcessRequest(Boolean includeStagesBeforeAsyncPoint, Boolean includeStagesAfterAsyncPoint)\n   at System.Web.UI.Page.ProcessRequest()\n   at System.Web.UI.Page.ProcessRequestWithNoAssert(HttpContext context)\n   at System.Web.UI.Page.ProcessRequest(HttpContext context)\n   at ASP.default_aspx.ProcessRequest(HttpContext context) in c:\\Users\\grendel\\AppData\\Local\\Temp\\Temporary ASP.NET Files\\extensiblerequestvalidation_01\\e21ea603\\117282eb\\App_Web_vaqbxbsk.0.cs:line 0\n   at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()\n   at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)\n   at System.Web.HttpApplication.ApplicationStepManager.ResumeSteps(Exception error)\n   at System.Web.HttpApplication.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)\n   at System.Web.HttpRuntime.ProcessRequestInternal(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequestNoDemand(HttpWorkerRequest wr)\n   at System.Web.HttpRuntime.ProcessRequest(HttpWorkerRequest wr)\n   at Microsoft.VisualStudio.WebHost.Request.Process()\n   at Microsoft.VisualStudio.WebHost.Host.ProcessRequest(Connection conn)");
			callSet003.Add ("rawUrl", "/Default.aspx?key=invalid%3Cscript%3Evalue%3C/script%3E");
			callSet003.Add ("context", true);
			callSet003.Add ("value", "invalid<script>value</script>");
			callSet003.Add ("requestValidationSource", 0);
			callSet003.Add ("collectionKey", "key");
			callSet003.Add ("returnValue", false);
			callSet003.Add ("validationFailureIndex", 7);
		}

		public RequestValidatorCallSet001 ()
		{
			CreateCallSet000 ();
			RegisterCallSet (callSet000);
			CreateCallSet001 ();
			RegisterCallSet (callSet001);
			CreateCallSet002 ();
			RegisterCallSet (callSet002);
			CreateCallSet003 ();
			RegisterCallSet (callSet003);
			Name = "001";
			RequestValidatorCallSetContainer.Register (this);
		}
	}

	static class GeneratedCallSets
	{
		public static void Register ()
		{
			RequestValidatorCallSet cs;
			cs = new RequestValidatorCallSet000 ();
			cs = new RequestValidatorCallSet001 ();
		}
	}
}
#endif
