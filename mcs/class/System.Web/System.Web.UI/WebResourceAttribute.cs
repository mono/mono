//
// System.Web.UI.WebResourceAttribute
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//
#if NET_1_2
namespace System.Web.UI {
	public sealed class WebResourceAttribute : Attribute {
		public WebResourceAttribute (string webResource, string contentType) : this (webResource, contentType, false) {}
		public WebResourceAttribute (string webResource, string contentType, bool performSubstitution)
		{
			this.webResource = webResource;
			this.contentType = contentType;
			this.performSubstitution = performSubstitution;
		}
		
		public string ContentType { get { return contentType; } }
		public bool PerformSubstitution { get { return performSubstitution; } }
		public string WebResource { get { return webResource; } }
		
		bool performSubstitution;
		string webResource, contentType;
	}
}
#endif