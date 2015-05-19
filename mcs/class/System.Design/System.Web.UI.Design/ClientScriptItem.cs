//
// System.Web.UI.Design.ClientScriptItem
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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


using System.ComponentModel;

namespace System.Web.UI.Design
{
	public sealed class ClientScriptItem
	{
		string text, source, language, type, id;

		public ClientScriptItem (string text, string source, string language, string type, string id)
		{
			this.text = text;
			this.source = source;
			this.language = language;
			this.type = type;
			this.id = id;
		}

		public string Id {
			get { return id; }
		}

		public string Language {
			get { return language; }
		}

		public string Source {
			get { return source; }
		}

		public string Text {
			get { return text; }
		}

		public string Type{
			get { return type; }
		}
	}
}

