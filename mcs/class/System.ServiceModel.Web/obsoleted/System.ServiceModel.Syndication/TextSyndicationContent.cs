//
// System.ServiceModel.Syndication.TextSyndicationContent
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
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

using System;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class TextSyndicationContent : SyndicationContent
	{
		private string text;
		private TextSyndicationContentKind type;

		public TextSyndicationContent (string text)
			{
				this.text = text;
				this.type = TextSyndicationContentKind.Plaintext;
			}

		public TextSyndicationContent (string text, TextSyndicationContentKind type)
			{
				this.text = text;
				this.type = type;
			}

		public string Text { 
			get { return text; }
		}

		public override string Type { 
			get { 
				string tname = "text";
				if (type == TextSyndicationContentKind.Html) tname = "html";
				else if (type == TextSyndicationContentKind.XHtml) tname = "xhtml";
				return tname;
			}
		}

		protected override void WriteContentsTo (XmlWriter writer)
		{
			writer.WriteString(Text);
		}

	}
}

