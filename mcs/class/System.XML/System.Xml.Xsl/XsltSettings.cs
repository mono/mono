//
// XsltSettings.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell Inc,
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

namespace System.Xml.Xsl
{
	public sealed class XsltSettings
	{
		static readonly XsltSettings defaultSettings;
		static readonly XsltSettings trustedXslt;

		static XsltSettings ()
		{
			defaultSettings = new XsltSettings (true);
			trustedXslt = new XsltSettings (true);
			trustedXslt.enableDocument = true;
			trustedXslt.enableScript = true;
		}

		public static XsltSettings Default {
			get { return defaultSettings; }
		}

		public static XsltSettings TrustedXslt {
			get { return trustedXslt; }
		}

		bool readOnly;
		bool enableDocument;
		bool enableScript;

		public XsltSettings ()
		{
		}

		public XsltSettings (bool enableDocumentFunction,
			bool enableScript)
		{
			this.enableDocument = enableDocumentFunction;
			this.enableScript = enableScript;
		}

		private XsltSettings (bool readOnly)
		{
			this.readOnly = readOnly;
		}

		public bool EnableDocumentFunction {
			get { return enableDocument; }
			set {
				if (!readOnly)
					enableDocument = value;
				// otherwise silently ignored.
			}
		}

		public bool EnableScript {
			get { return enableScript; }
			set {
				if (!readOnly)
					enableScript = value;
				// otherwise silently ignored.
			}
		}
	}
}
