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
		public ITemplate AlternatingItemTemplate {
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
		public ITemplate EditItemTemplate {
			get { return editItemTemplate; }
			set { editItemTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate FooterTemplate {
			get { return footerTemplate; }
			set { footerTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate HeaderTemplate {
			get { return headerTemplate; }
			set { headerTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate InsertItemTemplate {
			get { return insertItemTemplate; }
			set { insertItemTemplate = value; OnFieldChanged (); }
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(IDataItemContainer), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate ItemTemplate {
			get { return itemTemplate; }
			set { itemTemplate = value; OnFieldChanged (); }
		}
		
		public override void InitializeCell (DataControlFieldCell cell,
			DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			if (cellType == DataControlCellType.Header) {
				if (headerTemplate != null && ShowHeader) {
					headerTemplate.InstantiateIn (cell);
					return;
				}
			} else if (cellType == DataControlCellType.Footer) {
				if (footerTemplate != null) {
					footerTemplate.InstantiateIn (cell);
					return;
				}
			} else {
				if ((rowState & DataControlRowState.Insert) != 0) {
					if (insertItemTemplate != null) {
						insertItemTemplate.InstantiateIn (cell);
						return;
					}
				}
				else if ((rowState & DataControlRowState.Edit) != 0) {
					if (editItemTemplate != null) {
						editItemTemplate.InstantiateIn (cell);
						return;
					}
				}
				else if ((rowState & DataControlRowState.Alternate) != 0 && alternatingItemTemplate != null) {
					alternatingItemTemplate.InstantiateIn (cell);
					return;
				}
				else if (itemTemplate != null) {
					itemTemplate.InstantiateIn (cell);
					return;
				}
			}
			
			base.InitializeCell (cell, cellType, rowState, rowIndex);
		}
		
		public override void ExtractValuesFromCell (IOrderedDictionary dictionary,
			DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			IBindableTemplate bt;
			
			if ((rowState & DataControlRowState.Insert) != 0)
				bt = insertItemTemplate as IBindableTemplate; 
			else if ((rowState & DataControlRowState.Edit) != 0)
				bt = editItemTemplate as IBindableTemplate;
			else
				return;
			
			if (bt != null) {
				IOrderedDictionary values = bt.ExtractValues (cell);
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
