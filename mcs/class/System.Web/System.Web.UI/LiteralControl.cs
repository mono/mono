//
// System.Web.UI.LiteralControl.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.ComponentModel;
using System.Web;

namespace System.Web.UI
{
	[ToolboxItem(false)]
        public class LiteralControl : Control
        {
                private string _text = String.Empty;
                public LiteralControl() {}
                public LiteralControl(string text)
                {
                        _text = text;
                }
                public virtual string Text
                {
                        get
                        {
                                return _text;
                        }
                        set
                        {
                                _text = value;
                        }
                }
                protected override void Render(HtmlTextWriter writer)
                {
                        writer.Write(_text);
                }

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}
        }
}
