//
// System.Web.UI.WebControls.RadioButtonList.cs
//
// Authors:
//    Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//
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
//

using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls {

	[ValidationProperty ("SelectedItem")]
	public class RadioButtonList : ListControl, IRepeatInfoUser,
		INamingContainer, IPostBackDataHandler {

		public RadioButtonList ()
		{

		}

#if ONLY_1_1
		[Bindable (true)]
#endif		
		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int CellPadding {
			get {
				if (ControlStyleCreated == false)
					return -1; // default value

				return ((TableStyle) ControlStyle).CellPadding;
			}

			set {
				((TableStyle) ControlStyle).CellPadding = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif		
		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int CellSpacing {
			get {
				if (ControlStyleCreated == false)
					return -1; // default value

				return ((TableStyle) ControlStyle).CellSpacing;
			}

			set {
				((TableStyle) ControlStyle).CellSpacing = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif		
		[DefaultValue (0)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int RepeatColumns  {
			get {
				return ViewState.GetInt ("RepeatColumns", 0);
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("The number of columns is set to a negative value.");

				ViewState ["RepeatColumns"] = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif		
		[DefaultValue (RepeatDirection.Vertical)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatDirection RepeatDirection {
			get {
				return (RepeatDirection) ViewState.GetInt ("RepeatDirection", (int) RepeatDirection.Vertical);
			}

			set {
				if (value != RepeatDirection.Horizontal && value != RepeatDirection.Vertical)
					throw new ArgumentOutOfRangeException ("he display direction of the list is not one of the RepeatDirection values.");

				ViewState ["RepeatDirection"] = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif		
		[DefaultValue (RepeatLayout.Table)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatLayout RepeatLayout {
			get {
				return (RepeatLayout) ViewState.GetInt ("RepeatLayout", (int) RepeatLayout.Table);
			}

			set {
				if (value != RepeatLayout.Flow && value != RepeatLayout.Table)
					throw new ArgumentOutOfRangeException ("The radio buttons layout is not one of the RepeatLayout values.");

				ViewState ["RepeatLayout"] = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif		
		[DefaultValue (TextAlign.Right)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual TextAlign TextAlign {
			get {
				return (TextAlign )ViewState.GetInt ("TextAlign", (int) TextAlign.Right);
			}

			set {
				if (value != TextAlign.Left && value != TextAlign.Right)
					throw new ArgumentOutOfRangeException ("The label text alignment associated with the radio buttons is not one of the TextAlign values.");

				ViewState ["TextAlign"] = value;
			}
		}

		// Interface properties

#if NET_2_0
		[MonoTODO]
		protected virtual bool HasFooter
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected virtual bool HasHeader 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected virtual bool HasSeparators
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected virtual int RepeatedItemCount
		{
			get {
				throw new NotImplementedException ();
			}
		}
#endif
		
		bool IRepeatInfoUser.HasFooter {
			get { return false; }
		}

		bool IRepeatInfoUser.HasHeader {
			get { return false; }
		}

		bool IRepeatInfoUser.HasSeparators {
			get { return false; }
		}

		int IRepeatInfoUser.RepeatedItemCount {
			get { return Items.Count; }
		}

		protected override Style CreateControlStyle ()
		{
			return new TableStyle (ViewState);
		}

#if NET_2_0
		[MonoTODO]
		protected override Control FindControl (string id, int pathOffset)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_2_0
		protected virtual
#endif
		Style GetItemStyle (ListItemType itemType, int repeatIndex)
		{
			return null;
		}

#if NET_2_0
		protected virtual
#endif
		void RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			RadioButton radio = new RadioButton ();
			radio.Text = Items [repeatIndex].Text;
			radio.ID = ClientID + "_"  + repeatIndex;
			radio.TextAlign = TextAlign;
			radio.GroupName = UniqueID;
			radio.Page = Page;
			radio.Checked = Items [repeatIndex].Selected;
			radio.Attributes["Value"] = Items [repeatIndex].Value;
			radio.RenderControl (writer);
		}

		[MonoTODO]
#if NET_2_0
		protected virtual
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return true;
		}

		[MonoTODO]
#if NET_2_0
		protected virtual
#endif
		void RaisePostDataChangedEvent ()
		{
		}


		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

		Style IRepeatInfoUser.GetItemStyle (ListItemType itemType,  int repeatIndex)
		{
			return GetItemStyle (itemType, repeatIndex);
		}

		void IRepeatInfoUser.RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			RenderItem (itemType, repeatIndex, repeatInfo, writer);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter writer)
		{
			RepeatInfo repeat = new RepeatInfo ();
			repeat.RepeatColumns = RepeatColumns;
			repeat.RepeatDirection = RepeatDirection;
			repeat.RepeatLayout = RepeatLayout;
			repeat.RenderRepeater (writer, this, ControlStyle, this);
 		}
	}

}

