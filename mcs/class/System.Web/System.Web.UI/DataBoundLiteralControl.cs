//
// System.Web.UI.DataBoundLiteralCOntrol.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Web.UI;

namespace System.Web.UI {

	public sealed class DataBoundLiteralControl : Control
	{
		public DataBoundLiteralControl (int staticLiteralsCount,
						int dataBOundLiteralCount)
		{
		}

		[MonoTODO]
		public string Text {
			get { return String.Empty; }
		}

		[MonoTODO]
		protected override ControlCollection CreateControlCollection ()
		{
			throw new NotImplementedException ();
		}

		protected override void LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}

		protected override void Render (HtmlTextWriter output)
		{
			throw new NotImplementedException ();
		}

		protected override object SaveViewState ()
		{
			throw new NotImplementedException ();
		}

		public void SetDataBoundString (int index, string s)
		{
			throw new NotImplementedException ();
		}

		public void SetStaticString (int index, string s)
		{
			throw new NotImplementedException ();
		}
	}
}
