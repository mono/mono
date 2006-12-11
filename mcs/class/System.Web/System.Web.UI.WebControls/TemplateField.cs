//
// System.Web.UI.WebControls.TemplateField.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.Security.Permissions;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TemplateField : DataControlField
	{
		ITemplate alternatingItemTemplate;
		ITemplate editItemTemplate;
		ITemplate footerTemplate;
		ITemplate headerTemplate;
		ITemplate insertItemTemplate;
		ITemplate itemTemplate;
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate AlternatingItemTemplate {
			get { return alternatingItemTemplate; }
			set { alternatingItemTemplate = value; OnFieldChanged (); }
		}
		
		[DefaultValueAttribute (true)]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ConvertEmptyStringToNull {
			get {
				object ob = ViewState ["ConvertEmptyStringToNull"];
				if (ob != null) return (bool) ob;
				return true;
			}
			set {
				ViewState ["ConvertEmptyStringToNull"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate EditItemTemplate {
			get { return editItemTemplate; }
			set { editItemTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate FooterTemplate {
			get { return footerTemplate; }
			set { footerTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate HeaderTemplate {
			get { return headerTemplate; }
			set { headerTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate InsertItemTemplate {
			get { return insertItemTemplate; }
			set { insertItemTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate ItemTemplate {
			get { return itemTemplate; }
			set { itemTemplate = value; OnFieldChanged (); }
		}
		
		public override void InitializeCell (DataControlFieldCell cell,
						     DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell (cell, cellType, rowState, rowIndex);
			if (cellType == DataControlCellType.Header) {
				if (headerTemplate != null && ShowHeader) {
					cell.Text = String.Empty;
					headerTemplate.InstantiateIn (cell);
				}
			} else if (cellType == DataControlCellType.Footer) {
				if (footerTemplate != null) {
					cell.Text = String.Empty;
					footerTemplate.InstantiateIn (cell);
				}
			} else {
				cell.Text = String.Empty;
				if ((rowState & DataControlRowState.Insert) != 0 &&
						insertItemTemplate != null) {
					insertItemTemplate.InstantiateIn (cell);
				}
				else if ((rowState & DataControlRowState.Edit) != 0 &&
						editItemTemplate != null) {
					editItemTemplate.InstantiateIn (cell);
				}
				else if ((rowState & DataControlRowState.Alternate) != 0 &&
						alternatingItemTemplate != null) {
					alternatingItemTemplate.InstantiateIn (cell);
				}
				else if (itemTemplate != null) {
					itemTemplate.InstantiateIn (cell);
				}
				else
					cell.Text = "&nbsp;";
			}
		}
		
		public override void ExtractValuesFromCell (IOrderedDictionary dictionary,
							    DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			IBindableTemplate bt;
			
			if ((rowState & DataControlRowState.Insert) != 0)
				bt = insertItemTemplate as IBindableTemplate; 
			else if ((rowState & DataControlRowState.Edit) != 0)
				bt = editItemTemplate as IBindableTemplate;
			else if (alternatingItemTemplate !=null && (rowState & DataControlRowState.Alternate) != 0)
				bt = alternatingItemTemplate as IBindableTemplate;
			else
				bt = itemTemplate as IBindableTemplate;
			
			if (bt != null) {
				IOrderedDictionary values = bt.ExtractValues (cell);
				if (values == null)
					return;
				foreach (DictionaryEntry e in values)
					dictionary [e.Key] = e.Value; 
			}
		}
		
		public override void ValidateSupportsCallback ()
		{
			throw new NotSupportedException ("Callback not supported on TemplateField. Turn disable callbacks on '" + Control.ID + "'.");
		}
		
		protected override DataControlField CreateField ()
		{
			return new TemplateField ();
		}
		
		protected override void CopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
			TemplateField field = (TemplateField) newField;
			field.AlternatingItemTemplate = AlternatingItemTemplate;
			field.ConvertEmptyStringToNull = ConvertEmptyStringToNull;
			field.EditItemTemplate = EditItemTemplate;
			field.FooterTemplate = FooterTemplate;
			field.HeaderTemplate = HeaderTemplate;
			field.InsertItemTemplate = InsertItemTemplate;
			field.ItemTemplate = ItemTemplate;
		}
	}
}
#endif
