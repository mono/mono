//
// System.ComponentModel.Design.ObjectSelectorEditor
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
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
