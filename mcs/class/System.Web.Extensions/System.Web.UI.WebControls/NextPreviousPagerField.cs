//
// System.Web.UI.WebControls.NextPreviousPagerField
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2008 Novell, Inc
//

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
#if NET_3_5
using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class NextPreviousPagerField : DataPagerField
	{
		int _startRowIndex;
		int _maximumRows;
		int _totalRowCount;
		int _fieldIndex;
		
		public NextPreviousPagerField ()
		{
		}

		protected override void CopyProperties (DataPagerField newField)
		{
			base.CopyProperties (newField);

			NextPreviousPagerField field = newField as NextPreviousPagerField;
			if (field == null)
				return;
			
			field.ButtonCssClass = ButtonCssClass;
			field.ButtonType = ButtonType;
			field.FirstPageImageUrl = FirstPageImageUrl;
			field.FirstPageText = FirstPageText;
			field.LastPageImageUrl = LastPageImageUrl;
			field.LastPageText = LastPageText;
			field.NextPageImageUrl = NextPageImageUrl;
			field.NextPageText = NextPageText;
			field.PreviousPageImageUrl = PreviousPageImageUrl;
			field.PreviousPageText = PreviousPageText;
			field.ShowFirstPageButton = ShowFirstPageButton;
			field.ShowLastPageButton = ShowLastPageButton;
			field.ShowNextPageButton = ShowNextPageButton;
			field.ShowPreviousPageButton = ShowPreviousPageButton;
		}

		// What's the fieldIndex used for?
		public override void CreateDataPagers (DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex)
		{
			_startRowIndex = startRowIndex;
			_maximumRows = maximumRows;
			_totalRowCount = totalRowCount;
			_fieldIndex = fieldIndex;

			bool setPagePropertiesNeeded = false;
			bool queryMode = GetQueryModeStartRowIndex (_totalRowCount, _maximumRows, ref _startRowIndex, ref setPagePropertiesNeeded);
			bool enablePrevFirst = _startRowIndex >= _maximumRows;
			bool enableNextLast = (_startRowIndex + _maximumRows) < _totalRowCount;
			bool addNonBreakingSpace = RenderNonBreakingSpacesBetweenControls;

			if (ShowFirstPageButton)
				CreateButton (container, DataControlCommands.FirstPageCommandArgument, FirstPageText, FirstPageImageUrl, 0,
					      queryMode, enablePrevFirst, addNonBreakingSpace);
			
			int newPageNum = -1;
			if (ShowPreviousPageButton) {
				if (queryMode)
					newPageNum = (_startRowIndex / _maximumRows) - 1;
				
				CreateButton (container, DataControlCommands.PreviousPageCommandArgument, PreviousPageText, PreviousPageImageUrl, newPageNum,
					      queryMode, enablePrevFirst, addNonBreakingSpace);
			}
			
			if (ShowNextPageButton) {
				if (queryMode)
					newPageNum = (_startRowIndex + _maximumRows) / _maximumRows;
				
				CreateButton (container, DataControlCommands.NextPageCommandArgument, NextPageText, NextPageImageUrl, newPageNum,
					      queryMode, enableNextLast, addNonBreakingSpace);
			}
			
			if (ShowLastPageButton) {
				if (queryMode) {
					newPageNum = _totalRowCount / _maximumRows;
					if ((_totalRowCount % _maximumRows) == 0)
						newPageNum--;
				}
				
				CreateButton (container, DataControlCommands.LastPageCommandArgument, LastPageText, LastPageImageUrl, newPageNum,
					      queryMode, enableNextLast, addNonBreakingSpace);
			}
			
			if (setPagePropertiesNeeded)
				DataPager.SetPageProperties (_startRowIndex, _maximumRows, true);
		}

		void CreateButton (DataPagerFieldItem container, string commandName, string text, string imageUrl, int pageNum,
				   bool queryMode, bool enabled, bool addNonBreakingSpace)
		{
			WebControl ctl = null;
			
			if (queryMode) {
				pageNum++;
				HyperLink h = new HyperLink ();
				h.Text = text;
				h.ImageUrl = imageUrl;
				h.Enabled = enabled;
				h.NavigateUrl = GetQueryStringNavigateUrl (pageNum);
				h.CssClass = ButtonCssClass;
				ctl = h;
			} else {
				if (!enabled && RenderDisabledButtonsAsLabels) {
					Label l = new Label ();
					l.Text = text;
					ctl = l;
				} else {
					switch (ButtonType) {
						case ButtonType.Button:
							Button btn = new Button ();
							btn.CommandName = commandName;
							btn.Text = text;
							ctl = btn;
							break;

						case ButtonType.Link:
							LinkButton lbtn = new LinkButton ();
							lbtn.CommandName = commandName;
							lbtn.Text = text;
							ctl = lbtn;
							break;

						case ButtonType.Image:
							ImageButton ibtn = new ImageButton ();
							ibtn.CommandName = commandName;
							ibtn.ImageUrl = imageUrl;
							ibtn.AlternateText = text;
							ctl = ibtn;
							break;
					}

					if (ctl != null) {
						ctl.Enabled = enabled;
						ctl.CssClass = ButtonCssClass;
					}
				}
			}

			if (ctl != null) {
				container.Controls.Add (ctl);
				if (addNonBreakingSpace)
					container.Controls.Add (new LiteralControl ("&nbsp;"));
			}
		}
		
		protected override DataPagerField CreateField ()
		{
			return new NextPreviousPagerField ();
		}

		public override bool Equals (object o)
		{
			NextPreviousPagerField field = o as NextPreviousPagerField;
			if (field == null)
				return false;
			
			// Compare using the properties that are copied in CopyProperties
			if (field.ButtonCssClass != ButtonCssClass)
				return false;

			if (field.ButtonType != ButtonType)
				return false;

			if (field.FirstPageImageUrl != FirstPageImageUrl)
				return false;
			
			if (field.FirstPageText != FirstPageText)
				return false;
			
			if (field.LastPageImageUrl != LastPageImageUrl)
				return false;
			
			if (field.LastPageText != LastPageText)
				return false;
			
			if (field.NextPageImageUrl != NextPageImageUrl)
				return false;
			
			if (field.NextPageText != NextPageText)
				return false;
			
			if (field.PreviousPageImageUrl != PreviousPageImageUrl)
				return false;
			
			if (field.PreviousPageText != PreviousPageText)
				return false;
			
			if (field.ShowFirstPageButton != ShowFirstPageButton)
				return false;
			
			if (field.ShowLastPageButton != ShowLastPageButton)
				return false;
			
			if (field.ShowNextPageButton != ShowNextPageButton)
				return false;
			
			if (field.ShowPreviousPageButton != ShowPreviousPageButton)
				return false;

			return true;
		}

		public override int GetHashCode ()
		{
			int ret = 0;

			// Base the calculation on the properties that are copied in CopyProperties
			ret |= ButtonCssClass.GetHashCode ();
			ret |= ButtonType.GetHashCode ();
			ret |= FirstPageImageUrl.GetHashCode ();
			ret |= FirstPageText.GetHashCode ();
			ret |= LastPageImageUrl.GetHashCode ();
			ret |= LastPageText.GetHashCode ();
			ret |= NextPageImageUrl.GetHashCode ();
			ret |= NextPageText.GetHashCode ();
			ret |= PreviousPageImageUrl.GetHashCode ();
			ret |= PreviousPageText.GetHashCode ();
			ret |= ShowFirstPageButton.GetHashCode ();
			ret |= ShowLastPageButton.GetHashCode ();
			ret |= ShowNextPageButton.GetHashCode ();
			ret |= ShowPreviousPageButton.GetHashCode ();

			return ret;
		}

		public override void HandleEvent (CommandEventArgs e)
		{
			string commandName = e.CommandName;
			int newStartIndex = -1;
			int pageSize = DataPager.PageSize;
			
			if (String.Compare (commandName, DataControlCommands.FirstPageCommandArgument, StringComparison.OrdinalIgnoreCase) == 0)
				newStartIndex = 0;
			else if (String.Compare (commandName, DataControlCommands.LastPageCommandArgument, StringComparison.OrdinalIgnoreCase) == 0) {
				int lastPageMod = _totalRowCount % pageSize;
				if (lastPageMod == 0)
					newStartIndex = _totalRowCount - pageSize;
				else
					newStartIndex = _totalRowCount - lastPageMod;
			} else if (String.Compare (commandName, DataControlCommands.NextPageCommandArgument, StringComparison.OrdinalIgnoreCase) == 0) {
				newStartIndex = _startRowIndex + pageSize;
				if (_totalRowCount >= 0 && newStartIndex > _totalRowCount)
					newStartIndex = _totalRowCount - pageSize;
			} else if (String.Compare (commandName, DataControlCommands.PreviousPageCommandArgument, StringComparison.OrdinalIgnoreCase) == 0) {
				newStartIndex = _startRowIndex - pageSize;
				if (newStartIndex < 0)
					newStartIndex = 0;
			}

			if (newStartIndex >= 0)
				DataPager.SetPageProperties (newStartIndex, pageSize, true);
		}

		public string ButtonCssClass {
			get {
				string s = ViewState ["ButtonCssClass"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set { ViewState ["ButtonCssClass"] = value; }
		}

		public ButtonType ButtonType {
			get {
				object o = ViewState ["ButtonType"];
				if (o != null)
					return (ButtonType) o;

				return ButtonType.Link;
			}
			
			set { ViewState ["ButtonType"] = value; }
		}

		public string FirstPageImageUrl {
			get {
				string s = ViewState ["FirstPageImageUrl"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set { ViewState ["FirstPageImageUrl"] = value; }
		}

		public string FirstPageText {
			get {
				string s = ViewState ["FirstPageText"] as string;
				if (s != null)
					return s;

				return "First";
			}
			
			set { ViewState ["FirstPageText"] = value; }
		}

		public string LastPageImageUrl {
			get {
				string s = ViewState ["LastPageImageUrl"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set { ViewState ["LastPageImageUrl"] = value; }
		}

		public string LastPageText {
			get {
				string s = ViewState ["LastPageText"] as string;
				if (s != null)
					return s;

				return "Last";
			}
			
			set { ViewState ["LastPageText"] = value; }
		}

		public string NextPageImageUrl {
			get {
				string s = ViewState ["NextPageImageUrl"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set { ViewState ["NextPageImageUrl"] = value; }
		}

		public string NextPageText {
			get {
				string s = ViewState ["NextPageText"] as string;
				if (s != null)
					return s;

				return "Next";
			}
			
			set { ViewState ["NextPageText"] = value; }
		}

		public string PreviousPageImageUrl {
			get {
				string s = ViewState ["PreviousPageImageUrl"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set { ViewState ["PreviousPageImageUrl"] = value; }
		}

		public string PreviousPageText {
			get {
				string s = ViewState ["PreviousPageText"] as string;
				if (s != null)
					return s;

				return "Previous";
			}
			
			set { ViewState ["PreviousPageText"] = value; }
		}

		public bool RenderDisabledButtonsAsLabels {
			get {
				object o = ViewState ["RenderDisabledButtonsAsLabels"];
				if (o != null)
					return (bool) o;

				return false;
			}
			
			set { ViewState ["RenderDisabledButtonsAsLabels"] = value; }
		}

		public bool RenderNonBreakingSpacesBetweenControls {
			get {
				object o = ViewState ["RenderNonBreakingSpacesBetweenControls"];
				if (o != null)
					return (bool) o;

				return true;
			}
			
			set { ViewState ["RenderNonBreakingSpacesBetweenControls"] = value; }
		}

		public bool ShowFirstPageButton {
			get {
				object o = ViewState ["ShowFirstPageButton"];
				if (o != null)
					return (bool) o;

				return false;
			}
			
			set { ViewState ["ShowFirstPageButton"] = value; }
		}

		public bool ShowLastPageButton {
			get {
				object o = ViewState ["ShowLastPageButton"];
				if (o != null)
					return (bool) o;

				return false;
			}
			
			set { ViewState ["ShowLastPageButton"] = value; }
		}

		public bool ShowNextPageButton {
			get {
				object o = ViewState ["ShowNextPageButton"];
				if (o != null)
					return (bool) o;

				return true;
			}
			
			set { ViewState ["ShowNextPageButton"] = value; }
		}

		public bool ShowPreviousPageButton {
			get {
				object o = ViewState ["ShowPreviousPageButton"];
				if (o != null)
					return (bool) o;

				return true;
			}
			
			set { ViewState ["ShowPreviousPageButton"] = value; }
		}
	}
}
#endif
