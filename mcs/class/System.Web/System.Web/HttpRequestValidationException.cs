//
// System.Web.HttpRequestValidationException
//
// Authors:
//   	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Novell, Inc. (http://www.novell.com)
//
#if NET_1_1
namespace System.Web
{
	public sealed class HttpRequestValidationException : HttpException
	{
		internal HttpRequestValidationException (string msg) : base (msg)
		{
		}

		internal override string Description {
			get {
				return  "Request validation detected a potentially dangerous input value " +
					"from the client and aborted the request. This might be an attemp of " +
					"using cross-site scripting to compromise the security of your site. " +
					"You can disable request validation using the 'validateRequest=false' " +
					"attribute in your page or setting it in your machine.config or web.config " +
					"configuration files. If you disable it, you're encouraged to properly " +
					"check the input values you get from the client.<br>\r\n" +
					"You can get more information on input validation " +
					"<a href=\"http://www.cert.org/tech_tips/malicious_code_mitigation.html\">" +
					"here</a>.";
			}
		}
	}
}
#endif
