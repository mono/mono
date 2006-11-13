//
// System.Web.UI.WebControls.WizardStepCollection
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

using System;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public sealed class WizardStepCollection : IList, ICollection, IEnumerable
	{
		ArrayList list = new ArrayList ();
		Wizard wizard;
		
		internal WizardStepCollection (Wizard wizard)
		{
			this.wizard = wizard;
		}
		
		public int Count {
			get { return list.Count; }
		} 
		
		public bool IsReadOnly {
			get { return false; }
		} 
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public WizardStepBase this [int index] {
			get { return (WizardStepBase) list [index]; }
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		public void Add (WizardStepBase wizardStep)
		{
			if (wizardStep == null) throw new ArgumentNullException ("wizardStep");
			wizardStep.SetWizard (wizard);
			list.Add (wizardStep);
			wizard.UpdateViews ();
		}
		
		public void AddAt (int index, WizardStepBase wizardStep)
		{
			if (wizardStep == null) throw new ArgumentNullException ("wizardStep");
			wizardStep.SetWizard (wizard);
			list.Insert (index, wizardStep);
			wizard.UpdateViews ();
		}
		
		public void Clear ()
		{
			list.Clear ();
			wizard.UpdateViews ();
		}
		
		public bool Contains (WizardStepBase wizardStep)
		{
			if (wizardStep == null) throw new ArgumentNullException ("wizardStep");
			return list.Contains (wizardStep);
		}
		
		public void CopyTo (WizardStepBase[] array, int index)
		{
			list.CopyTo (array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		public int IndexOf (WizardStepBase wizardStep)
		{
			if (wizardStep == null) throw new ArgumentNullException ("wizardStep");
			return list.IndexOf (wizardStep);
		}
		
		public void Insert (int index, WizardStepBase wizardStep)
		{
			AddAt (index, wizardStep);
		}
		
		public void Remove (WizardStepBase wizardStep)
		{
			if (wizardStep == null) throw new ArgumentNullException ("wizardStep");
			list.Remove (wizardStep);
			wizard.UpdateViews ();
		}
		
		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
			wizard.UpdateViews ();
		}
		
		bool IList.IsFixedSize {
			get { return false; }
		}
		
		object IList.this [int index] {
			get { return list [index]; }
			set { list [index] = value; }
		}
		
		int IList.Add (object ob)
		{
			int res = list.Add ((WizardStepBase)ob);
			wizard.UpdateViews ();
			return res;
		}
		
		bool IList.Contains (object ob)
		{
			return Contains ((WizardStepBase)ob);
		}
		
		int IList.IndexOf (object ob)
		{
			return IndexOf ((WizardStepBase)ob);
		}
		
		void IList.Insert (int index, object ob)
		{
			AddAt (index, (WizardStepBase)ob);
		}
		
		void IList.Remove (object ob)
		{
			Remove ((WizardStepBase)ob);
		}
		
		void ICollection.CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}
	}
}

#endif
