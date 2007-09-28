//
// System.ComponentModel.Design.ObjectSelectorEditor
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
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

using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.ComponentModel.Design
{
	public abstract class ObjectSelectorEditor : UITypeEditor
	{
		public ObjectSelectorEditor ()
		{
		}

		public ObjectSelectorEditor (bool subObjectSelector)
		{
			SubObjectSelector = subObjectSelector;
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		public bool EqualsToValue (object value)
		{
			return (currValue == value);
		}

		[MonoTODO]
		protected virtual void FillTreeWithData (Selector selector, ITypeDescriptorContext context, IServiceProvider provider)
		{
			throw new NotImplementedException ();
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public virtual void SetValue (object value)
		{
			currValue = value;
		}

		protected object currValue;
		protected object prevValue;
		public bool SubObjectSelector;

		public class Selector : TreeView
		{
			[MonoTODO]
			public Selector (ObjectSelectorEditor editor)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public SelectorNode AddNode (string label, object value, SelectorNode parent)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public void Clear ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			protected void OnAfterSelect (object sender, TreeViewEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			protected override void OnKeyDown (KeyEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			protected override void OnKeyPress (KeyPressEventArgs e)
			{
				throw new NotImplementedException ();
			}

#if NET_2_0
			[MonoTODO]
			protected override void OnNodeMouseClick (TreeNodeMouseClickEventArgs e)
			{
				throw new NotImplementedException ();
			}
#endif

			[MonoTODO]
			public bool SetSelection (object value, TreeNodeCollection nodes)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public void Start (IWindowsFormsEditorService edSvc, object value)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public void Stop ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			protected override void WndProc (ref Message m)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public bool clickSeen;
		}

		public class SelectorNode : TreeNode
		{
			public SelectorNode (string label, object value) : base (label)
			{
				this.value = value;
			}

			public object value;
		}

	}
}
