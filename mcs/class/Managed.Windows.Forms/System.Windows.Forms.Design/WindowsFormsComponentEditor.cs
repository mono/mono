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

namespace System.Windows.Forms.Design
{
	public abstract class WindowsFormsComponentEditor : ComponentEditor
	{
		protected WindowsFormsComponentEditor ()
		{
		}

		public override bool EditComponent (ITypeDescriptorContext context, object component)
		{
			return EditComponent (context, component, null);
		}

		public virtual bool EditComponent (ITypeDescriptorContext context, object component, IWin32Window owner)
		{
			ComponentEditorForm f = new ComponentEditorForm (component, GetComponentEditorPages ());
			if (f.ShowForm (owner, GetInitialComponentEditorPageIndex ()) == DialogResult.OK)
				return true;
			return false;
		}

		public bool EditComponent (object component, IWin32Window owner)
		{
			return EditComponent (null, component, owner);
		}

		protected virtual Type[] GetComponentEditorPages ()
		{
			return null;
		}

		protected virtual int GetInitialComponentEditorPageIndex ()
		{
			return 0;
		}
	}
}
