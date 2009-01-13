//
// System.Web.UI.WebControls.XmlBuilder.cs
//
// Author:
// 	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.Web.Compilation;
using System.Web.UI;
using System.Xml;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	public
#endif
	class XmlBuilder : ControlBuilder
	{
		public override void AppendLiteralString (string s)
		{
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs)
		{
			return null;
		}

		public override bool NeedsTagInnerText ()
		{
			return true;
		}

		public override void SetTagInnerText (string text)
		{
			string trimmed = text.Trim ();
			if (trimmed == "")
				return;

			XmlDocument doc = new XmlDocument ();
			try {
				doc.LoadXml (text);
			} catch (XmlException xmle) {
				Location newloc = new Location (Location);
				if (xmle.LineNumber >= 0)
					newloc.BeginLine += xmle.LineNumber - 1;

				Location = newloc;
				throw;
			}

			base.AppendLiteralString (trimmed);
		}
	}
}

