/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       CalendarAutoFormatDialog
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
	public class CalendarAutoFormatDialog : Form
	{
		private Calendar calendar;
		private bool     activated;

		public CalendarAutoFormatDialog(Calendar calendar) : base()
		{
			this.calendar  = calendar;
			this.activated = false;
			this.InitializeCalendar();
		}

		[MonoTODO]
		private void InitializeCalendar()
		{
			// Load various WC-schemes.
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected void OnActivated(object source, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
