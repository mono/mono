/**
 * Namespace: System.Web.UI.WebControls
 * Class:     HyperLinkColumn
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class HyperLinkColumn: DataGridColumn
	{
		PropertyDescriptor textFieldDescriptor;
		PropertyDescriptor urlFieldDescriptor;

		public HyperLinkColumn ()
		{
		}

		public virtual string DataNavigateUrlField {
			get {
				object o = ViewState ["DataNavigateUrlField"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["DataNavigateUrlField"] = value;
				OnColumnChanged ();
			}
		}

		public virtual string DataNavigateUrlFormatString {
			get {
				object o = ViewState ["DataNavigateUrlFormatString"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["DataNavigateUrlFormatString"] = value;
				OnColumnChanged ();
			}
		}

		public virtual string DataTextField {
			get {
				object o = ViewState ["DataTextField"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}
			set {
				ViewState ["DataTextField"] = value;
				OnColumnChanged ();
			}
		}

		public virtual string DataTextFormatString {
			get {
				object o = ViewState ["DataTextFormatString"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["DataTextFormatString"] = value;
				OnColumnChanged ();
			}
		}

		public virtual string NavigateUrl {
			get {
				object o = ViewState ["NavigateUrl"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["NavigateUrl"] = value;
				OnColumnChanged ();
			}
		}

		public virtual string Target {
			get {
				object o = ViewState ["Target"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["Target"] = value;
				OnColumnChanged ();
			}
		}

		public virtual string Text {
			get {
				object o = ViewState ["Text"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["Text"] = value;
				OnColumnChanged ();
			}
		}

		public override void Initialize ()
		{
			textFieldDescriptor = null;
			urlFieldDescriptor  = null;
			base.Initialize ();
		}

		public override void InitializeCell (TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell (cell, columnIndex, itemType);

			if (itemType != ListItemType.Header && itemType != ListItemType.Footer) {
				HyperLink toDisplay = new HyperLink ();
				toDisplay.Text = Text;
				toDisplay.NavigateUrl = NavigateUrl;
				toDisplay.Target = Target;

				if (DataTextField.Length > 0 || DataNavigateUrlField.Length > 0)
					toDisplay.DataBinding += new EventHandler (OnDataBindHyperLinkColumn);

				cell.Controls.Add (toDisplay);
			}
		}

		private void OnDataBindHyperLinkColumn (object sender, EventArgs e)
		{
			HyperLink link = (HyperLink) sender;
			object item    = ((DataGridItem) link.NamingContainer).DataItem;

			if (textFieldDescriptor == null && urlFieldDescriptor == null) {
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (item);
				textFieldDescriptor = properties.Find (DataTextField, true);
				if (textFieldDescriptor == null && !DesignMode)
					throw new HttpException (HttpRuntime.FormatResourceString (
									"Field_Not_Found", DataTextField));

				urlFieldDescriptor = properties.Find (DataNavigateUrlField, true);
				if (urlFieldDescriptor == null && !DesignMode)
					throw new HttpException (HttpRuntime.FormatResourceString (
									"Field_Not_Found", DataNavigateUrlField));
			}

			if (textFieldDescriptor != null) {
				link.Text = FormatDataTextValue (textFieldDescriptor.GetValue (item));
			} else {
				link.Text = "Sample_DataBound_Text";
			}

			if (urlFieldDescriptor != null) {
				link.NavigateUrl = FormatDataNavigateUrlValue (urlFieldDescriptor.GetValue (item));
			} else {
				link.NavigateUrl = "url";
			}
		}

		protected virtual string FormatDataNavigateUrlValue (object dataUrlValue)
		{
			if (dataUrlValue == null)
				return String.Empty;

			string retVal;
			if (DataNavigateUrlFormatString.Length > 0) {
				retVal = String.Format (DataNavigateUrlFormatString, dataUrlValue);
			} else {
				retVal = dataUrlValue.ToString ();
			}

			return retVal;
		}

		protected virtual string FormatDataTextValue (object dataTextValue)
		{
			if (dataTextValue == null)
				return String.Empty;

			string retVal;
			if (DataTextFormatString.Length > 0) {
				retVal = String.Format (DataTextFormatString, dataTextValue);
			} else {
				retVal = dataTextValue.ToString ();
			}

			return retVal;
		}
	}
}

