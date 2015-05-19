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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Andreas Nahr	(ClassDevelopment@A-SoftTech.com)
//

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Design
{
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public abstract class ComponentEditorPage : Panel
	{
		private bool commitOnDeactivate = false;
		private IComponent component;
		private bool firstActivate = true;
		private Icon icon;
		private int loading = 0;
		private bool loadRequired = false;
		private IComponentEditorPageSite pageSite;

		public ComponentEditorPage ()
		{
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public virtual bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		public bool CommitOnDeactivate
		{
			get { return commitOnDeactivate; }
			set { commitOnDeactivate = value; }
		}

		protected IComponent Component {
			get { return component; }
			set { component = value; }
		}

		[MonoTODO ("Find out what this does.")]
		protected override CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
			}
		}

		protected bool FirstActivate {
			get { return firstActivate; }
			set { firstActivate = value; }
		}

		public Icon Icon {
			get { return icon; }
			set { icon = value; }
		}

		protected int Loading {
			get { return loading; }
			set { loading = value; }
		}

		protected bool LoadRequired {
			get { return loadRequired; }
			set { loadRequired = value; }
		}

		protected IComponentEditorPageSite PageSite {
			get { return pageSite; }
			set { pageSite = value; }
		}

		public virtual string Title {
			get { return base.Text; }
		}

		public virtual void Activate ()
		{
			Visible = true;
			firstActivate = false;
			if (loadRequired) {
				EnterLoadingMode ();
				LoadComponent ();
				ExitLoadingMode ();
			}
		}

		public virtual void ApplyChanges ()
		{
			SaveComponent ();
		}

		public virtual void Deactivate ()
		{
			Visible = false;
		}

		protected void EnterLoadingMode ()
		{
			loading++;
		}

		protected void ExitLoadingMode ()
		{
			loading--;
		}

		public virtual Control GetControl ()
		{
			return this;
		}

		protected IComponent GetSelectedComponent ()
		{
			return component;
		}

		protected bool IsFirstActivate ()
		{
			return firstActivate;
		}

		protected bool IsLoading ()
		{
			return (loading != 0);
		}

		public virtual bool IsPageMessage (ref Message msg)
		{
			return PreProcessMessage (ref msg);
		}

		protected abstract void LoadComponent ();

		[MonoTODO ("Find out what this does.")]
		public virtual void OnApplyComplete ()
		{
		}

		protected virtual void ReloadComponent ()
		{
			loadRequired = true;
		}

		protected abstract void SaveComponent ();

		public virtual void SetComponent (IComponent component)
		{
			this.component = component;
			ReloadComponent ();
		}

		[MonoTODO ("Find out what this does.")]
		protected virtual void SetDirty ()
		{
		}

		public virtual void SetSite (IComponentEditorPageSite site)
		{
			pageSite = site;
			pageSite.GetControl ().Controls.Add (this);

		}

		public virtual void ShowHelp ()
		{
		}

		public virtual bool SupportsHelp ()
		{
			return false;
		}

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}
		#endregion
	}
}
