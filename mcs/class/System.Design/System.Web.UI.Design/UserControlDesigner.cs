//
// System.Web.UI.Design.UserControlDesigner
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System;
using System.ComponentModel.Design;

namespace System.Web.UI.Design
{
	public class UserControlDesigner : ControlDesigner
	{
		public UserControlDesigner ()
		{
		}

		public override string GetDesignTimeHtml ()
		{
			return base.CreatePlaceHolderDesignTimeHtml ();
		}

		public override string GetPersistInnerHtml ()
		{
			return null;
		}

		public override bool AllowResize {
			get {
				return false;
			}
		}

		public override bool ShouldCodeSerialize {
			get {
				return false;
			}
			set {
			}
		}
	}
}
