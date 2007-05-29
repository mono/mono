//
// System.ComponentModel.Design.CollectionEditor
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
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

		[MonoTODO("Low-priority: designers are no-ops on Mono")]
		public CollectionEditor (Type type)
		{
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context,
						  IServiceProvider provider,
						  object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override UITypeEditorEditStyle GetEditStyle (
						      ITypeDescriptorContext context)
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
	}
}
