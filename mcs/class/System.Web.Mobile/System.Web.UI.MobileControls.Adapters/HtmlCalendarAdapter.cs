
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
/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : HtmlCalendarAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.IO;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

namespace System.Web.UI.MobileControls.Adapters
{
	public class HtmlCalendarAdapter : HtmlControlAdapter
	{
		private const int bgColDistThresh = 1000;
		private const int bgColInsert     = 4;
		private const string selDateAttr = "background-color:Silver;";
		private const string selDateCellTag = "<td ";
		private const string selDateTableTag = "<table ";
		
		public HtmlCalendarAdapter()
		{
		}
		
		protected new Calendar Control
		{
			get
			{
				return (Calendar)base.Control;
			}
		}
		
		public override void Render(HtmlMobileTextWriter writer)
		{
			System.Web.UI.WebControls.WebControl cal = Control.WebCalendar;
			Style.ApplyTo(cal);
			cal.Visible = true;
			writer.EnterStyle(new Style());
			writer.EnsureStyle();
			if(Control.Alignment != Alignment.NotSet)
			{
				cal.Attributes["align"] = Control.Alignment.ToString();
			}
			if(Device.SupportsCss)
			{
				cal.RenderControl(writer);
			} else
			{
				StringWriter sw = new StringWriter();
				HtmlTextWriter htw = new HtmlTextWriter(sw);
				cal.RenderControl(htw);
				string res = sw.ToString();
				//TODO: Add styles manually (with things like
				// <table bgcolor="something">
				// and foreach selectedDates =>
				// "<td bgcolor="new Color(BackColorKey.R/G/B)"> etc
				throw new NotImplementedException();
			}
			if(Control.BreakAfter)
			{
				writer.WriteBreak();
			}
			writer.ExitStyle(new Style());
		}
		
		[MonoTODO("Helper_For_Render")]
		private int GetNextSelectedDate(string webCalHtml, int start)
		{
			throw new NotImplementedException();
		}
	}
}
