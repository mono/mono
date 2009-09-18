//
// DynamicControl.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[ToolboxBitmap (typeof(DynamicControl), "DynamicControl.ico")]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DynamicControl : Control, IAttributeAccessor, IFieldTemplateHost, IFieldFormattingOptions
	{
		Dictionary <string, string> attributes;
		bool? applyFormatInEditMode;
		bool? convertEmptyStringToNull;
		bool? htmlEncode;
		string dataField = String.Empty;
		string dataFormatString;
		string nullDisplayText;
		string uiHint;
		
		public DynamicControl () : this (DataBoundControlMode.ReadOnly)
		{
		}

		public DynamicControl (DataBoundControlMode mode)
		{
			Mode = mode;
			CssClass = String.Empty;
			ValidationGroup = String.Empty;
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool ApplyFormatInEditMode {
			get {
				if (applyFormatInEditMode == null) {
					MetaColumn column = Column;
					applyFormatInEditMode = column != null ? column.ApplyFormatInEditMode : false;
				}
				
				return (bool)applyFormatInEditMode;
			}
			
			set { applyFormatInEditMode = value; }
		}

		[Browsable (false)]
		public MetaColumn Column { get; set; }

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool ConvertEmptyStringToNull {
			get {
				if (convertEmptyStringToNull == null) {
					MetaColumn column = Column;
					convertEmptyStringToNull = column != null ? column.ConvertEmptyStringToNull : false;
				}

				return (bool)convertEmptyStringToNull;
			}
			
			set { convertEmptyStringToNull = value; }
		}

		[MonoTODO]
		[Category ("Appearance")]
		[DefaultValue ("")]
		[CssClassProperty]
		public virtual string CssClass { get; set; }

		[Category ("Data")]
		[DefaultValue ("")]
		public string DataField {
			get { return dataField; }
			
			set { dataField = value == null ? String.Empty : value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string DataFormatString {
			get {
				if (dataFormatString == null) {
					MetaColumn column = Column;
					if (column != null) {
						dataFormatString = column.DataFormatString;
						if (dataFormatString == null)
							dataFormatString = String.Empty;
					} else
						dataFormatString = String.Empty;
				}

				return dataFormatString;
			}
			
			set { dataFormatString = value == null ? String.Empty : value; }
		}

		[MonoTODO]
		[Browsable (false)]
		public Control FieldTemplate { get; private set; }

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool HtmlEncode {
			get {
				if (htmlEncode == null) {
					MetaColumn column = Column;
					htmlEncode = column != null ? column.HtmlEncode : true;
				}

				return (bool)htmlEncode;
			}
			
			set { htmlEncode = value; }
		}

		IFieldFormattingOptions IFieldTemplateHost.FormattingOptions {
			get { return this; }
		}

		[MonoTODO]
		public DataBoundControlMode Mode { get; set; }

		[Category ("Behavior")]
		[DefaultValue ("")]
		public string NullDisplayText {
			get {
				if (nullDisplayText == null) {
					MetaColumn column = Column;
					if (column != null) {
						nullDisplayText = column.NullDisplayText;
						if (nullDisplayText == null)
							nullDisplayText = String.Empty;
					} else
						nullDisplayText = String.Empty;
				}

				return nullDisplayText;
			}
			
			set { nullDisplayText = value == null ? String.Empty : value; }
		}
		
		[Browsable (false)]
		public virtual MetaTable Table {
			get { return this.FindMetaTable (); }
		}

		[Category ("Behavior")]
		[DefaultValue ("")]
		public virtual string UIHint {
			get {
				if (uiHint == null) {
					MetaColumn column = Column;
					uiHint = column != null ? column.UIHint : String.Empty;
					if (uiHint == null)
						uiHint = String.Empty;
				}

				return uiHint;
			}
			
			set { uiHint = value != null ? value : String.Empty; }
			
		}

		[Themeable (false)]
		[Category ("Behavior")]
		[DefaultValue ("")]
		public virtual string ValidationGroup { get; set; }

		void CreateFieldTemplate ()
		{
			MetaColumn column = Column;
				
			// Safe as ResolveColumn won't return with a null Column
			MetaModel model = column.Model;
			IFieldTemplateFactory ftf = model != null ? model.FieldTemplateFactory : null;
			IFieldTemplate ft;
			
			if (ftf != null) {
				ft = ftf.CreateFieldTemplate (column, Mode, UIHint);
				if (ft == null)
					return;
			} else
				return;
			
			ft.SetHost (this);

			Control ctl = ft as Control;
			if (ctl == null)
				return;
			
			FieldTemplate = ctl;
			Controls.Add (ctl);
		}
		
		public string GetAttribute (string key)
		{
			if (attributes == null)
				return String.Empty;

			string ret;
			if (attributes.TryGetValue (key, out ret))
				return ret;
			else
				// "Compatibility"...
				throw new KeyNotFoundException ("NoSuchAttribute");
		}

		protected override void OnInit (EventArgs e)
		{
			// It seems _all_ the properties are initialized _only_ here. Further user's
			// actions to set the Column property don't affect the other properties
			// which derive their values from the associated MetaColumn.
			base.OnInit (e);
			if (Column == null) {
				ResolveColumn ();
				Controls.Clear ();
				CreateFieldTemplate ();
			}
		}

		protected override void Render (HtmlTextWriter writer)
		{
			string cssClass = CssClass;
			bool haveCssClass = !String.IsNullOrEmpty (cssClass);
			
			if (haveCssClass) {
				writer.AddAttribute (HtmlTextWriterAttribute.Class, cssClass);
				writer.RenderBeginTag (HtmlTextWriterTag.Span);
				writer.Write ("\n\n");
			}
			
			base.Render (writer);

			if (haveCssClass) {
				writer.RenderEndTag ();
			}
		}

		void ResolveColumn ()
		{
			string dataField = DataField;
			if (String.IsNullOrEmpty (dataField))
				throw new InvalidOperationException ("The '" + GetType ().Name + "' control '" + ID + "' must have a DataField attribute.");

			MetaTable table = Table;
			// And, as it is .NET DynamicData's tradition... no null check!!
			if (table == null)
				throw new NullReferenceException ();

			Column = table.GetColumn (dataField);
		}
		
		internal void InternalSetAttributes (Dictionary <string, string> attributes)
		{
			this.attributes = attributes;
		}
		
		public void SetAttribute (string key, string value)
		{
			if (attributes == null)
				attributes = new Dictionary <string, string> ();

			if (attributes.ContainsKey (key))
				attributes [key] = value;
			else
				attributes.Add (key, value);
		}
	}
}
