/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       RegexEditorDialog
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
	public class RegexEditorDialog : Form
	{
		private ISite site;
		private string regularExpression = String.Empty;

		[MonoTODO]
		public RegexEditorDialog(ISite site)
		{
			this.site = site;
			throw new NotImplementedException();
		}
		
		public string RegularExpression
		{
			get
			{
				return regularExpression;
			}
			set
			{
				regularExpression = value;
			}
		}
	}
}
