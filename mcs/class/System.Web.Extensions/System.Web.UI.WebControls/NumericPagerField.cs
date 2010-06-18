//
// System.Web.UI.WebControls.NumericPagerField
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2010 Novell, Inc
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
using System.Globalization;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class NumericPagerField : DataPagerField
	{
		const string Default_NextPageText = "...";
		const string Default_PreviousPageText = "...";

		int _startRowIndex;
		int _maximumRows;
		int _totalRowCount;
		int _fieldIndex;
		bool _renderNonBreakingSpacesBetweenControls = true;
		
		public NumericPagerField ()
		{
		}

		protected override void CopyProperties (DataPagerField newField)
		{
			base.CopyProperties (newField);

			NumericPagerField field = newField as NumericPagerField;
			if (field == null)
				return;
			
			field.ButtonCount = ButtonCount;
			field.ButtonType = ButtonType;
			field.CurrentPageLabelCssClass = CurrentPageLabelCssClass;
			field.NextPageImageUrl = NextPageImageUrl;
			field.NextPageText = NextPageText;
			field.NextPreviousButtonCssClass = NextPreviousButtonCssClass;
			field.NumericButtonCssClass = NumericButtonCssClass;
			field.PreviousPageImageUrl = PreviousPageImageUrl;
			field.PreviousPageText = PreviousPageText;
			field.RenderNonBreakingSpacesBetweenControls = RenderNonBreakingSpacesBetweenControls;
		}

		public override void CreateDataPagers (DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex)
		{
			_startRowIndex = startRowIndex;
			_maximumRows = maximumRows;
			_totalRowCount = totalRowCount;
			_fieldIndex = fieldIndex;

			bool setPagePropertiesNeeded = false;
			bool queryMode = GetQueryModeStartRowIndex (_totalRowCount, _maximumRows, ref _startRowIndex, ref setPagePropertiesNeeded);
			bool addNonBreakingSpace = RenderNonBreakingSpacesBetweenControls;
			int buttonCount = ButtonCount;
			int totalPages = totalRowCount / maximumRows + (totalRowCount % maximumRows > 0 ? 1 : 0);
			int currentPage = startRowIndex == 0 ? 1 : (startRowIndex / maximumRows) + 1;
			int activePage = ((startRowIndex / (maximumRows * buttonCount)) * buttonCount) + 1;
			int lastPage = activePage + buttonCount - 1;
			
			bool showPreviousPage = activePage > buttonCount;
			bool showNextPage = totalPages - activePage >= buttonCount;

			if (lastPage > totalPages)
				lastPage = totalPages;

			int newPageNum;
			if (showPreviousPage) {
				newPageNum = activePage - 1;
				if (newPageNum < 1)
					newPageNum = 1;

				CreateButton (container, DataControlCommands.PreviousPageCommandArgument, PreviousPageText, PreviousPageImageUrl,
					      NextPreviousButtonCssClass, newPageNum, queryMode, true, addNonBreakingSpace, false);
			}

			string numericButtonCssClass = NumericButtonCssClass;
			bool enabled;
			string pageString;
			while (activePage <= lastPage) {
				enabled = activePage != currentPage;
				pageString = activePage.ToString (CultureInfo.InvariantCulture);
				CreateButton (container, pageString, pageString, String.Empty,
					      enabled ? numericButtonCssClass : CurrentPageLabelCssClass, activePage,
					      queryMode, enabled, addNonBreakingSpace, true);
				activePage++;
			}
			if (showNextPage && addNonBreakingSpace)
					container.Controls.Add (new LiteralControl ("&nbsp;"));
			
			if (showNextPage)
				CreateButton (container, DataControlCommands.NextPageCommandArgument, NextPageText, NextPageImageUrl,
					      NextPreviousButtonCssClass, activePage, queryMode, true, addNonBreakingSpace, false);

			if (setPagePropertiesNeeded)
				DataPager.SetPageProperties (_startRowIndex, _maximumRows, true);
		}

		void CreateButton (DataPagerFieldItem container, string commandName, string text, string imageUrl, string cssClass, int pageNum,
				   bool queryMode, bool enabled, bool addNonBreakingSpace, bool isPageNumber)
		{
			WebControl ctl = null;
			
			if (queryMode) {
				if (isPageNumber && !enabled) {
					var span = new Label ();
					span.Text = text;
					span.CssClass = cssClass;
					ctl = span;
				} else {
					HyperLink h = new HyperLink ();
					h.Text = text;
					h.ImageUrl = imageUrl;
					h.Enabled = enabled;
					h.NavigateUrl = GetQueryStringNavigateUrl (pageNum);
					h.CssClass = cssClass;
					ctl = h;
				}
			} else {
				if (!enabled) {
					Label l = new Label ();
					l.Text = text;
					l.CssClass = cssClass;
					ctl = l;
				} else {
					switch (ButtonType) {
						case ButtonType.Button:
							Button btn = new Button ();
							btn.CommandName = commandName;
							btn.CommandArgument = pageNum.ToString ();
							btn.Text = text;
							break;

						case ButtonType.Link:
							LinkButton lbtn = new LinkButton ();
							lbtn.CommandName = commandName;
							lbtn.CommandArgument = pageNum.ToString ();
							lbtn.Text = text;
							ctl = lbtn;
							break;

						case ButtonType.Image:
							ImageButton ibtn = new ImageButton ();
							ibtn.CommandName = commandName;
							ibtn.CommandArgument = pageNum.ToString ();
							ibtn.ImageUrl = imageUrl;
							ibtn.AlternateText = text;
							ctl = ibtn;
							break;
					}

					if (ctl != null)
						ctl.CssClass = cssClass;
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
			return new NumericPagerField ();
		}

		public override bool Equals (object o)
		{
			NumericPagerField field = o as NumericPagerField;

			if (field == null)
				return false;

			if (field.ButtonCount != ButtonCount)
				return false;
			
			if (field.ButtonType != ButtonType)
				return false;
			
			if (String.Compare (field.CurrentPageLabelCssClass, CurrentPageLabelCssClass, StringComparison.Ordinal) != 0)
				return false;
			
			if (String.Compare (field.NextPageImageUrl, NextPageImageUrl, StringComparison.Ordinal) != 0)
				return false;
			
			if (String.Compare (field.NextPageText, NextPageText, StringComparison.Ordinal) != 0)
				return false;
			
			if (String.Compare (field.NextPreviousButtonCssClass, NextPreviousButtonCssClass, StringComparison.Ordinal) != 0)
				return false;
			
			if (String.Compare (field.NumericButtonCssClass, NumericButtonCssClass, StringComparison.Ordinal) != 0)
				return false;
			
			if (String.Compare (field.PreviousPageImageUrl, PreviousPageImageUrl, StringComparison.Ordinal) != 0)
				return false;
			
			if (String.Compare (field.PreviousPageText, PreviousPageText, StringComparison.Ordinal) != 0)
				return false;
			
			if (field.RenderNonBreakingSpacesBetweenControls != RenderNonBreakingSpacesBetweenControls)
				return false;

			return true;
		}

		public override int GetHashCode ()
		{
			int ret = 0;

			// Base the calculation on the properties that are copied in CopyProperties
			ret |= ButtonCount.GetHashCode ();
			ret |= ButtonType.GetHashCode ();
			ret |= CurrentPageLabelCssClass.GetHashCode ();
			ret |= NextPageImageUrl.GetHashCode ();
			ret |= NextPageText.GetHashCode ();
			ret |= NextPreviousButtonCssClass.GetHashCode ();
			ret |= NumericButtonCssClass.GetHashCode ();
			ret |= PreviousPageImageUrl.GetHashCode ();
			ret |= PreviousPageText.GetHashCode ();
			ret |= RenderNonBreakingSpacesBetweenControls.GetHashCode ();

			return ret;
		}

		public override void HandleEvent (CommandEventArgs e)
		{
			string commandName = e.CommandName;
			int pageNum;

			if (!Int32.TryParse (e.CommandArgument as string, out pageNum))
				pageNum = 0;
			else if (pageNum >= 1)
				pageNum--;
			else if (pageNum < 0)
				pageNum = 0;
			
			int newStartIndex = -1;
			int pageSize = DataPager.PageSize;
			int offset = pageSize * pageNum;
			
			if (String.Compare (commandName, DataControlCommands.NextPageCommandArgument, StringComparison.OrdinalIgnoreCase) == 0 ||
			    String.Compare (commandName, DataControlCommands.PreviousPageCommandArgument, StringComparison.OrdinalIgnoreCase) == 0) {
				newStartIndex = offset;
			} else
				newStartIndex = (Int32.Parse (commandName) - 1) * pageSize;

			if (newStartIndex != -1)
				DataPager.SetPageProperties (newStartIndex, pageSize, true);
		}

		public int ButtonCount {
			get {
				object o = ViewState ["ButtonCount"];
				if (o != null)
					return (int)o;
				
				return 5;
			}
			
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("value", "The ButtonCount property is set to a value less than 1.");
				
				ViewState ["ButtonCount"] = value;
			}
		}

		public ButtonType ButtonType {
			get {
				object o = ViewState ["ButtonType"];
				if (o != null)
					return (ButtonType)o;

				return ButtonType.Link;
			}
			
			set {
				if (!Enum.IsDefined (typeof (ButtonType), value))
					throw new ArgumentOutOfRangeException ("value", "The value for the ButtonType property is not one of the ButtonType values.");

				ViewState ["ButtonType"] = value;
			}
		}

		public string CurrentPageLabelCssClass {
			get {
				string s = ViewState ["CurrentPageLabelCssClass"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ViewState ["CurrentPageLabelCssClass"] = value;
			}
		}

		public string NextPageImageUrl {
			get {
				string s = ViewState ["NextPageImageUrl"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ViewState ["NextPageImageUrl"] = value;
			}
		}

		public string NextPageText {
			get {
				string s = ViewState ["NextPageText"] as string;
				if (s != null)
					return s;

				return Default_NextPageText;
			}
			
			set {
				if (String.IsNullOrEmpty (value) || String.Compare (Default_NextPageText, value, StringComparison.Ordinal) == 0)
					return;

				ViewState ["NextPageText"] = value;
			}
		}

		public string NextPreviousButtonCssClass {
			get {
				string s = ViewState ["NextPreviousButtonCssClass"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ViewState ["NextPreviousButtonCssClass"] = value;
			}
		}

		public string NumericButtonCssClass {
			get {
				string s = ViewState ["NumericButtonCssClass"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ViewState ["NumericButtonCssClass"] = value;
			}
		}

		public string PreviousPageImageUrl {
			get {
				string s = ViewState ["PreviousPageImageUrl"] as string;
				if (s != null)
					return s;

				return String.Empty;
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ViewState ["PreviousPageImageUrl"] = value;
			}
		}

		public string PreviousPageText {
			get {
				string s = ViewState ["PreviousPageText"] as string;
				if (s != null)
					return s;

				return Default_PreviousPageText;
			}
			
			set {
				if (String.IsNullOrEmpty (value) || String.Compare (Default_PreviousPageText, value, StringComparison.Ordinal) == 0)
					return;

				ViewState ["PreviousPageText"] = value;
			}
		}

		public bool RenderNonBreakingSpacesBetweenControls {
			get { return _renderNonBreakingSpacesBetweenControls; }
			set { _renderNonBreakingSpacesBetweenControls = value; }
		}
	}
}
#endif
