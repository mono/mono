//
// System.Web.UI.WebControls.RadioButton.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	[Designer ("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	public class RadioButton : CheckBox, IPostBackDataHandler
	{
		public RadioButton () : base ()
		{
		}

		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("The name of the group that this control belongs to.")]
		public virtual string GroupName
		{
			get {
				object o = ViewState ["GroupName"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["GroupName"] = value; }
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null && Enabled && !Checked)
				Page.RegisterRequiresPostBack (this);

			if(GroupName.Length == 0)
				GroupName = UniqueID;
		}

		internal override void RenderInputTag (HtmlTextWriter writer, string id)
		{
			writer.AddAttribute (HtmlTextWriterAttribute.Id, id);
			writer.AddAttribute (HtmlTextWriterAttribute.Type, "radio");
			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueGroupNamePrivate);
			writer.AddAttribute (HtmlTextWriterAttribute.Value, ValueAttributePrivate);

			if (Checked)
				writer.AddAttribute (HtmlTextWriterAttribute.Checked, "checked");

			if (AutoPostBack){
				writer.AddAttribute (HtmlTextWriterAttribute.Onclick,
						     Page.GetPostBackClientEvent (this, ""));
				writer.AddAttribute ("language", "javascript");
			}

			if (AccessKey.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Accesskey, AccessKey);

			if (TabIndex > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Tabindex,
						     TabIndex.ToString (NumberFormatInfo.InvariantInfo));

			writer.RenderBeginTag (System.Web.UI.HtmlTextWriterTag.Input);
			writer.RenderEndTag ();
		}

		private string UniqueGroupNamePrivate
		{
			get {
				string retVal = GroupName;
				int unique = UniqueID.LastIndexOf (':');
				if (unique >= 0)
					retVal += UniqueID.Substring (unique + 1);

				return retVal;
			}
		}

		private string ValueAttributePrivate
		{
			get {
				string retVal = Attributes ["value"];
				if (retVal != null)
					return retVal;

				if (ID != null)
					return ID;

				return UniqueID;
			}
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey,
						        NameValueCollection postCollection)
		{
			bool _checked = Checked;
			if (postCollection [UniqueGroupNamePrivate] == ValueAttributePrivate){
				if (_checked)
					return false;
				Checked = true;
				return true;
			}

			if (_checked)
				Checked = false;
			return true;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnCheckedChanged (EventArgs.Empty);
		}
	}
}
