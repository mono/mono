//
// System.Web.UI.WebControls.ListBox.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ValidationProperty("SelectedItem")]
	public class ListBox : ListControl, IPostBackDataHandler
	{
		public ListBox () : base ()
		{
		}

		[Browsable (false)]
		public override Color BorderColor
		{
			get { return base.BorderColor; }
			set { base.BorderColor = value; }
		}

		[Browsable (false)]
		public override BorderStyle BorderStyle
		{
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}

		[Browsable (false)]
		public override Unit BorderWidth
		{
			get { return base.BorderWidth; }
			set { base.BorderWidth = value; }
		}


		[DefaultValue (4), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The number of rows displayed by the control.")]
		public virtual int Rows
		{
			get {
				object o = ViewState ["Rows"];
				return (o == null) ? 4 : (int) o;
			}

			set {
				if (value < 1 || value > 2000)
					throw new ArgumentOutOfRangeException ("value", "Rows value has to be >= 0 and <= 2000.");

				ViewState ["Rows"] = value;
			}
		}

		[DefaultValue (typeof (ListSelectionMode), "Single"), WebCategory ("Behavior")]
		[WebSysDescription ("The mode describing how the entries can be selected.")]
		public virtual ListSelectionMode SelectionMode
		{
			get
			{
				object o = ViewState ["SelectionMode"];
				return (o == null) ? ListSelectionMode.Single : (ListSelectionMode) o;
			}
			set
			{
				if (!Enum.IsDefined (typeof (ListSelectionMode), value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				ViewState ["SelectionMode"] = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Bindable (false), EditorBrowsable (EditorBrowsableState.Never)]
		public override string ToolTip
		{
			get { return String.Empty; }
			set { /* Don't do anything. */ }
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			base.AddAttributesToRender (writer);
			writer.AddAttribute (HtmlTextWriterAttribute.Size,
					     Rows.ToString (NumberFormatInfo.InvariantInfo));

			if (SelectionMode == ListSelectionMode.Multiple)
				writer.AddAttribute (HtmlTextWriterAttribute.Multiple, "multiple");

			if (AutoPostBack && Page != null){
				writer.AddAttribute (HtmlTextWriterAttribute.Onchange,
						     Page.GetPostBackClientEvent (this, ""));
				writer.AddAttribute ("language", "javascript");
			}
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null && SelectionMode == ListSelectionMode.Multiple && Enabled)
				Page.RegisterRequiresPostBack(this);
		}

		protected override void RenderContents (HtmlTextWriter writer)
		{
			bool isMultAllowed = (SelectionMode == ListSelectionMode.Multiple);
			bool selMade = false;
			foreach (ListItem current in Items){
				writer.WriteBeginTag ("option");
				if (current.Selected){
					if (!isMultAllowed && selMade)
						throw new HttpException ("Cannot_MultiSelect_In_Single_Mode");
					selMade = true;
					writer.WriteAttribute ("selected", "selected");
				}
				writer.WriteAttribute ("value", current.Value, true);
				writer.Write ('>');
				writer.Write (HttpUtility.HtmlEncode (current.Text));
				writer.WriteEndTag ("option");
				writer.WriteLine ();
			}
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
		{
			string[] vals = postCollection.GetValues (postDataKey);
			bool updated = false;
			ArrayList selected = SelectedIndices;
			ArrayList final = new ArrayList (vals.Length);
			if (vals != null){
				if (SelectionMode == ListSelectionMode.Single){
					int index = Items.FindByValueInternal (vals [0]);
					if (SelectedIndex != index){
						SelectedIndex = index;
						updated       = true;
					}
				} else {
					foreach (string current in vals)
						final.Add (Items.FindByValueInternal (current));

					if (selected != null && selected.Count == vals.Length){
						for (int ctr = 0; ctr < vals.Length; ctr++){
							if (((int) final [ctr]) != ((int) selected [ctr])){
								updated = true;
								break;
							}
						}
					} else {
						updated = true;
					}
				}
				if (!updated)
					Select (final);
			} else {
				if (SelectedIndex != -1)
					SelectedIndex = -1;
				updated = true;
			}
			return updated;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnSelectedIndexChanged (EventArgs.Empty);
		}
	}
}
