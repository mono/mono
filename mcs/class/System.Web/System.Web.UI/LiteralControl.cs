//
// System.Web.UI.LiteralControl.cs
//
// Author:
// 	Bob Smith <bob@thestuff.net>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Bob Smith
// Copyright (C) 2002-2010 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem(false)]
        public class LiteralControl : Control, ITextControl
        {
                string _text;

			public LiteralControl ()
			{
				EnableViewState = false;
				AutoID = false;

			}

			public LiteralControl (string text)
				: this ()
			{
				Text = text;
			}

                public virtual string Text {
                        get { return _text; }
                        set {
                                _text = (value == null) ? String.Empty : value;
                        }
                }

		protected internal override void Render (HtmlTextWriter output)
                {
                        output.Write (_text);
                }

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}
        }
}

