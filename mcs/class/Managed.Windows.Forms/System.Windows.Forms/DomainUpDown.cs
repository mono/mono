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
//	Miguel de Icaza (miguel@novell.com).
//
//

using System;
using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {
	public class DomainUpDown : UpDownBase, ISupportInitialize {
		public class DomainUpDownItemCollection : ArrayList {
			DomainUpDown owner;
			
			internal DomainUpDownItemCollection (DomainUpDown owner)
			{
				this.owner = owner;
			}

			public override int Add (object item)
			{
				int ret = base.Add (item);
				if (owner.sorted){
					Sort ();

					// Will trigger an update.
					owner.SelectedIndex = owner.SelectedIndex;
				}

				return ret;
			}

			public override void Insert (int index, object item)
			{
				base.Insert (index, item);
				if (owner.sorted){
					Sort ();
					owner.SelectedIndex = owner.SelectedIndex;
				} else {
					if (index == owner.SelectedIndex)
						owner.UpdateEditText ();
				}
			}

			public override void Remove (object item)
			{
				base.Remove (item);
				if (Count < owner.SelectedIndex)
					owner.SelectedIndex -= 1;
				if (owner.sorted){
					Sort ();
					owner.UpdateEditText ();
				}
			}

			public override void RemoveAt (int item)
			{
				base.RemoveAt (item);

				if (Count < owner.SelectedIndex)
					owner.SelectedIndex -= 1;
				if (owner.SelectedIndex == item)
					owner.UpdateEditText ();
				if (owner.sorted){
					Sort ();
					owner.UpdateEditText ();
				}
			}
		}

		int selected_index = -1;
		bool sorted = false;
		bool wrap = false;
		DomainUpDownItemCollection items;
		
#region ISupportInitialize methods
		
		public void BeginInit ()
		{
		}

		public void EndInit ()
		{
		}
#endregion

		public event EventHandler SelectedItemChanged;
		
		public DomainUpDown () : base () {
			items = new DomainUpDownItemCollection (this);
		}
		
		public override void DownButton ()
		{
			if (wrap)
				selected_index %= items.Count;
			else if (selected_index < items.Count)
				selected_index++;

			UpdateEditText ();
		}

		public override void UpButton ()
		{
			if (wrap){
				selected_index--;
				if (selected_index == -1)
					selected_index = items.Count-1;
			} else {
				if (selected_index > -1)
					selected_index--;
			}
			UpdateEditText ();
		}

		public override void UpdateEditText ()
		{
			ChangingText = true;
			if (selected_index == -1)
				Text = "";
			else
				Text = items [selected_index].ToString ();
			UserEdit = false;
		}

		protected override void OnChanged (object source, EventArgs e)
		{
			OnSelectedItemChanged (source, e);
		}
		
		protected void OnSelectedItemChanged (object source, EventArgs e)
		{
			if (SelectedItemChanged != null)
				SelectedItemChanged (source, e);
		}
		
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			AccessibleObject ao;
			
			ao = base.CreateAccessibilityInstance ();
			ao.description = "DomainUpDown";

			return ao;
		}

		public DomainUpDownItemCollection Items {
			get {
				return items;
			}
		}

		public int SelectedIndex {
			get {
				return selected_index;
			}

			set {
				if (value < -1 || value > items.Count)
					throw new ArgumentException (String.Format ("Boundaries are -1 to {0}", items.Count-1));

				selected_index = value;
				UpdateEditText ();
			}
		}

		public object SelectedItem {
			get {
				if (selected_index == -1)
					return null;

				return items [selected_index];
			}

			set {
				for (int i = 0; i < items.Count; i++)
					if (items [i] == value){
						SelectedIndex = i;
						break;
					}
			}
		}

		public bool Sorted {
			get {
				return sorted;
			}

			set {
				//
				// It never returns to unsorted state
				//

				sorted = value;
				
				if (sorted)
					items.Sort ();
			}
		}

		public bool Wrap {
			get {
				return wrap;
			}

			set {
				wrap = value;
			}
		}
	}
}
