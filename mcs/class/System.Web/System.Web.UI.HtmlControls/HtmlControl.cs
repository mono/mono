//
// System.Web.UI.HtmlControls.HtmlControl.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls
{
        public abstract class HtmlControl : Control, IAttributeAccessor
        {
                private string _tagName = "span";
//TODO: Is this correct, or is the StateBag really the ViewState?
                private AttributeCollection _attributes = new AttributeCollection(new StateBag(true));
                private bool _disabled = false;
                public HtmlControl() {}
                public HtmlControl(string tag)
                {
                        if(tag != null && tag != "") _tagName = tag;
                }
                public AttributeCollection Attributes
                {
                        get
                        {
                                return _attributes;
                        }
                }
                public bool Disabled
                {
                        get
                        {
                                return _disabled;
                        }
                        set
                        {
                                _disabled = value;
                        }
                }
                public CssStyleCollection Style
                {
                        get
                        {
                                return _attributes.CssStyle;
                        }
                }
                public virtual string TagName
                {
                        get
                        {
                                return _tagName;
                        }
                }
        }
}
