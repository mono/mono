//
// System.Web.UI.WebResourceAttribute
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
#if NET_2_0
namespace System.Web.UI {
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple = true)]
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
