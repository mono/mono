//
// System.Web.UI.HtmlControls.HtmlGenericControl.cs
//
// Authors:
//   Bob Smith <bob@thestuff.net>
//   Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) Bob Smith
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//
	
using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	[ConstructorNeedsTag]
	public class HtmlGenericControl : HtmlContainerControl {
		public HtmlGenericControl() :
			this ("span")
		{
		}
		
		public HtmlGenericControl (string tag) :
			base ()
		{
			if (tag == null)
				tag = "";
			_tagName = tag;
		}
		
		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Appearance")]
		public new string TagName
		{
			get { return _tagName; }
			set { _tagName = value; }
		}
	}
}

