//
// System.Web.UI.HtmlControls.HtmlGenericControl.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//
	
using System;
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
			base(tag)
		{
		}
		
		public override string TagName
		{
			get { return base.TagName; }
		}
	}
}

