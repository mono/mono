//
// System.Web.UI.WebControls.ContentPlaceHolderBuilder.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc.
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
using System.Web.UI.WebControls;

namespace System.Web.UI.WebControls
{	
	internal class ContentPlaceHolderBuilder: ControlBuilder
	{
		public override void Init (TemplateParser parser, ControlBuilder parentBuilder, Type type,
					   string tagName, string ID, IDictionary attribs)
		{
			string id = null, s;
			foreach (object k in attribs.Keys) {
				s = k as string;
				if (String.IsNullOrEmpty (s))
					continue;
				
				if (String.Compare (s, "id", StringComparison.OrdinalIgnoreCase) == 0) {
					id = attribs [s] as string;
					break;
				}
			}
			base.Init (parser, parentBuilder, type, tagName, ID, attribs);
			MasterPageParser mpp = parser as MasterPageParser;
			if (mpp == null || String.IsNullOrEmpty (id))
				return;
			
			mpp.AddContentPlaceHolderId (id);
		}
	}
}

