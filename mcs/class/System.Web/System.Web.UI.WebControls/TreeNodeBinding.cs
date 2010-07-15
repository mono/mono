//
// System.Web.UI.WebControls.TreeNodeBinding.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Collections;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty ("TextField")]
	public sealed class TreeNodeBinding: IStateManager, ICloneable, IDataSourceViewSchemaAccessor
	{
		StateBag ViewState = new StateBag ();
		
		[DefaultValue ("")]
		public string DataMember {
			get {
				object o = ViewState ["DataMember"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["DataMember"] = value;
			}
		}

		[DefaultValue (-1)]
		public int Depth {
			get {
				object o = ViewState ["Depth"];
				if (o != null) return (int) o;
				return -1;
			}
			set {
				ViewState ["Depth"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string FormatString {
			get {
				object o = ViewState ["FormatString"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["FormatString"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string ImageToolTip {
			get {
				object o = ViewState ["ImageToolTip"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["ImageToolTip"] = value;
			}
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string ImageToolTipField {
			get {
				object o = ViewState ["ImageToolTipField"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["ImageToolTipField"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string ImageUrl {
			get {
				object o = ViewState ["ImageUrl"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["ImageUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string ImageUrlField {
			get {
				object o = ViewState ["ImageUrlField"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["ImageUrlField"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string NavigateUrl {
			get {
				object o = ViewState ["NavigateUrl"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["NavigateUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string NavigateUrlField {
			get {
				object o = ViewState ["NavigateUrlField"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["NavigateUrlField"] = value;
			}
		}

		[DefaultValue (false)]
		public bool PopulateOnDemand {
			get {
				object o = ViewState ["PopulateOnDemand"];
				if (o != null) return (bool) o;
				return false;
			}
			set {
				ViewState ["PopulateOnDemand"] = value;
			}
		}

		[DefaultValue (TreeNodeSelectAction.Select)]
		public TreeNodeSelectAction SelectAction {
			get {
				object o = ViewState ["SelectAction"];
				if (o != null) return (TreeNodeSelectAction)o;
				return TreeNodeSelectAction.Select;
			}
			set {
				ViewState ["SelectAction"] = value;
			}
		}

		[DefaultValue (false)]
		public Nullable<bool> ShowCheckBox {
			get {
				return (Nullable<bool>) ViewState ["ShowCheckBox"];
			}
			set {
				ViewState ["ShowCheckBox"] = value;
			}
		}

		[DefaultValue ("")]
		public string Target {
			get {
				object o = ViewState ["Target"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Target"] = value;
			}
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string TargetField {
			get {
				object o = ViewState ["TargetField"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState ["TargetField"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		[WebSysDescription ("The display text of the tree node.")]
		public string Text {
			get {
				object o = ViewState ["Text"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Text"] = value;
			}
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string TextField {
			get {
				object o = ViewState ["TextField"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["TextField"] = value;
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public string ToolTip {
			get {
				object o = ViewState ["ToolTip"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["ToolTip"] = value;
			}
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string ToolTipField {
			get {
				object o = ViewState ["ToolTipField"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["ToolTipField"] = value;
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public string Value {
			get {
				object o = ViewState ["Value"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Value"] = value;
			}
		}
		
		internal bool HasPropertyValue (string propName)
		{
			return ViewState [propName] != null;
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public string ValueField {
			get {
				object o = ViewState ["ValueField"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["ValueField"] = value;
			}
		}

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
			TreeNodeBinding bin = new TreeNodeBinding ();
			foreach (DictionaryEntry e in ViewState)
				bin.ViewState [(string)e.Key] = e.Value;
			return bin;
		}

		internal void SetDirty ()
		{
			foreach (string key in ViewState.Keys)
				ViewState.SetItemDirty (key, true);
		}

		public override string ToString () {
			return DataMember.Length > 0 ? DataMember : "(Empty)";
		}
	}
}

#endif
