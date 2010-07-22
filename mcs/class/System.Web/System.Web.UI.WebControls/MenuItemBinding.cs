//
// System.Web.UI.WebControls.MenuItemBinding.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004-2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty ("TextField")]
	public sealed class MenuItemBinding: IStateManager, ICloneable, IDataSourceViewSchemaAccessor
	{
		StateBag ViewState = new StateBag ();
		
		[DefaultValue ("")]
		public string DataMember {
			get { return ViewState.GetString ("DataMember", String.Empty); }
			set { ViewState ["DataMember"] = value; }
		}

		[DefaultValue (-1)]
		public int Depth {
			get { return ViewState.GetInt ("Depth", -1); }
			set { ViewState ["Depth"] = value; }
		}

		[DefaultValue (true)]
		public bool Enabled {
			get { return ViewState.GetBool ("Enabled", true); }
			set { ViewState ["Enabled"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string EnabledField {
			get { return ViewState.GetString ("EnabledField", String.Empty); }
			set { ViewState ["EnabledField"] = value; }
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string FormatString {
			get { return ViewState.GetString ("FormatString", String.Empty); }
			set { ViewState ["FormatString"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string ImageUrl {
			get { return ViewState.GetString ("ImageUrl", String.Empty); }
			set { ViewState ["ImageUrl"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string ImageUrlField {
			get { return ViewState.GetString ("ImageUrlField", String.Empty); }
			set { ViewState ["ImageUrlField"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string NavigateUrl {
			get { return ViewState.GetString ("NavigateUrl", String.Empty); }
			set { ViewState ["NavigateUrl"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string NavigateUrlField {
			get { return ViewState.GetString ("NavigateUrlField", String.Empty); }
			set { ViewState ["NavigateUrlField"] = value; }
		}

		[DefaultValue (true)]
		public bool Selectable {
			get { return ViewState.GetBool ("Selectable", true); }
			set { ViewState ["Selectable"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string SelectableField {
			get { return ViewState.GetString ("SelectableField", String.Empty); }
			set { ViewState ["SelectableField"] = value; }
		}

		[DefaultValue ("")]
		public string Target {
			get { return ViewState.GetString ("Target", String.Empty); }
			set { ViewState ["Target"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string TargetField {
			get { return ViewState.GetString ("TargetField", String.Empty); }
			set { ViewState ["TargetField"] = value; }
		}

		[Localizable (true)]
		[DefaultValue ("")]
		[WebSysDescription ("The display text of the menu item.")]
		public string Text {
			get { return ViewState.GetString ("Text", String.Empty); }
			set { ViewState ["Text"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string TextField {
			get { return ViewState.GetString ("TextField", String.Empty); }
			set { ViewState ["TextField"] = value; }
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public string ToolTip {
			get { return ViewState.GetString ("ToolTip", String.Empty); }
			set { ViewState ["ToolTip"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string ToolTipField {
			get { return ViewState.GetString ("ToolTipField", String.Empty); }
			set { ViewState ["ToolTipField"] = value; }
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public string Value {
			get { return ViewState.GetString ("Value", String.Empty); }
			set { ViewState ["Value"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string ValueField {
			get { return ViewState.GetString ("ValueField", String.Empty); }
			set { ViewState ["ValueField"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string PopOutImageUrl {
			get { return ViewState.GetString ("PopOutImageUrl", String.Empty); }
			set { ViewState ["PopOutImageUrl"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string PopOutImageUrlField {
			get { return ViewState.GetString ("PopOutImageUrlField", String.Empty); }
			set { ViewState ["PopOutImageUrlField"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string SeparatorImageUrl {
			get { return ViewState.GetString ("SeparatorImageUrl", String.Empty); }
			set { ViewState ["SeparatorImageUrl"] = value; }
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string SeparatorImageUrlField {
			get { return ViewState.GetString ("SeparatorImageUrlField", String.Empty); }
			set { ViewState ["SeparatorImageUrlField"] = value; }
		}
#if NET_4_0
		public override string ToString ()
		{
			string dm = DataMember;
			if (String.IsNullOrEmpty (dm))
				return "(Empty)";

			return dm;
		}
#endif
		void IStateManager.LoadViewState (object savedState)
		{
			ViewState.LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return ViewState.SaveViewState();
		}
		
		void IStateManager.TrackViewState ()
		{
			ViewState.TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return ViewState.IsTrackingViewState; }
		}
		
		[MonoTODO ("Not implemented")]
		object IDataSourceViewSchemaAccessor.DataSourceViewSchema {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		object ICloneable.Clone ()
		{
			MenuItemBinding bin = new MenuItemBinding ();
			foreach (DictionaryEntry e in ViewState)
				bin.ViewState [(string)e.Key] = e.Value;
			return bin;
		}

		internal void SetDirty ()
		{
			StateBag vs = ViewState;
			foreach (string key in vs.Keys)
				vs.SetItemDirty (key, true);
		}
	}
}

