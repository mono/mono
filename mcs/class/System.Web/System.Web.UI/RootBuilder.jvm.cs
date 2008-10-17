//
// System.Web.UI.RootBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.UI.HtmlControls;

namespace System.Web.UI {

#if NET_2_0
	public class RootBuilder : TemplateBuilder {

		public RootBuilder ()
		{
		}
#else
	public sealed class RootBuilder : TemplateBuilder {
#endif
		public RootBuilder (TemplateParser parser)
		{
			throw new NotImplementedException ();
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs) 
		{
			throw new NotImplementedException ();

		}

#if NET_2_0
		// FIXME: it's empty (but not null) when using the new default ctor
		// but I'm not sure when something should gets in...
		public IDictionary BuiltObjects {
			get {
							throw new NotImplementedException ();
			}
		}
#endif
	}
}
