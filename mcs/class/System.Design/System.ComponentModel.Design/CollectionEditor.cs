//
// System.ComponentModel.Design.CollectionEditor
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Design;

namespace System.ComponentModel.Design
{
	public class CollectionEditor : UITypeEditor
	{
		protected abstract class CollectionForm : Form
		{
			[MonoTODO]
			public CollectionForm (CollectionEditor editor)
			{
			}

			public object EditValue {
				[MonoTODO]
				get { throw new NotImplementedException(); } 

				[MonoTODO]
				set { throw new NotImplementedException(); }
			}

			public override ISite Site {
				[MonoTODO]
				get { throw new NotImplementedException(); } 

				[MonoTODO]
				set { throw new NotImplementedException(); }
			}

			protected Type CollectionItemType {
				[MonoTODO]
				get { throw new NotImplementedException(); }
			}
			
			protected Type CollectionType {
				[MonoTODO]
				get { throw new NotImplementedException(); }
			}

			protected ITypeDescriptorContext Context {
				[MonoTODO]
				get { throw new NotImplementedException(); }
			}

			protected override ImeMode DefaultImeMode {
				[MonoTODO]
				get { throw new NotImplementedException(); }
			}

			protected object[] Items {
				[MonoTODO]
				get { throw new NotImplementedException(); } 

				[MonoTODO]
				set { throw new NotImplementedException(); }
			}

			protected Type[] NewItemTypes {
				[MonoTODO]
				get { throw new NotImplementedException(); }
			}

			[MonoTODO]
			protected bool CanRemoveInstance (object value)
			{
				throw new NotImplementedException();
			}

			[MonoTODO]
			protected virtual bool CanSelectMultipleInstances()
			{
				throw new NotImplementedException();
			}

			[MonoTODO]
			protected object CreateInstance (Type itemType)
			{
				throw new NotImplementedException();
			}

			[MonoTODO]
			protected void DestroyInstance (object instance)
			{
				throw new NotImplementedException();
			}

			[MonoTODO]
			protected virtual void DisplayError (Exception e)
			{
				throw new NotImplementedException();
			}

			[MonoTODO]
			protected override object GetService (Type serviceType)
			{
				throw new NotImplementedException();
			}

			protected abstract void OnEditValueChanged();

			[MonoTODO]
			protected internal virtual DialogResult ShowEditorDialog (
						   IWindowsFormsEditorService edSvc)
			{
				throw new NotImplementedException();
			}

			[MonoTODO]
			~CollectionForm ()
			{
			}
		}

		[MonoTODO]
		public CollectionEditor (Type type)
		{
		}

		[MonoTODO]
		public object EditValue (ITypeDescriptorContext context,
						  IServiceProvider provider,
						  object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public object EditValue (IServiceProvider provider,
					 object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public UITypeEditorEditStyle GetEditStyle (
						      ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public UITypeEditorEditStyle GetEditStyle()
		{
			throw new NotImplementedException();
		}

		protected Type CollectionItemType {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		protected Type CollectionType {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		protected ITypeDescriptorContext Context {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}
		
		protected virtual string HelpTopic {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		protected Type[] NewItemTypes {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		protected virtual bool CanRemoveInstance (object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual bool CanSelectMultipleInstances()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual CollectionForm CreateCollectionForm()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual Type CreateCollectionItemType()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual object CreateInstance (Type itemType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual Type[] CreateNewItemTypes()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void DestroyInstance (object instance)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual object[] GetItems (object editValue)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected object GetService (Type serviceType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual object SetItems (object editValue,
						   object[] value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void ShowHelp()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		~CollectionEditor()
		{
		}
	}
}
