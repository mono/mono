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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jonathan Gilbert	<logic@deltaq.org>
//
// Integration into MWF:
//	Peter Bartok		<pbartok@novell.com>
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[DefaultProperty("Items")]
	[DefaultEvent("SelectedItemChanged")]
	[DefaultBindingProperty ("SelectedItem")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class DomainUpDown : UpDownBase {
		#region Local Variables
		private DomainUpDownItemCollection	items;
		private int				selected_index = -1;
		private bool				sorted;
		private bool				wrap;
		private int				typed_to_index = -1;
		#endregion	// Local Variables

		#region DomainUpDownAccessibleObject sub-class
		[ComVisible(true)]
		public class DomainItemAccessibleObject : AccessibleObject {
			#region DomainItemAccessibleObject Local Variables
			private AccessibleObject parent;
			#endregion	// DomainItemAccessibleObject Local Variables

			#region DomainItemAccessibleObject Constructors
			public DomainItemAccessibleObject(string name, AccessibleObject parent) {
				this.name = name;
				this.parent = parent;
			}
			#endregion	// DomainItemAccessibleObject Constructors

			#region DomainItemAccessibleObject Properties
			public override string Name {
				get {
					return base.Name;
				}

				set {
					base.Name = value;
				}
			}

			public override AccessibleObject Parent {
				get {
					return parent;
				}
			}

			public override AccessibleRole Role {
				get {
					return base.Role;
				}
			}

			public override AccessibleStates State {
				get {
					return base.State;
				}
			}

			public override string Value {
				get {
					return base.Value;
				}
			}
			#endregion	// DomainItemAccessibleObject Properties
		}
		#endregion	// DomainItemAccessibleObject sub-class

		#region DomainUpDownAccessibleObject sub-class
		[ComVisible(true)]
		public class DomainUpDownAccessibleObject : ControlAccessibleObject {
			#region DomainUpDownAccessibleObject Local Variables
			//private Control	owner;
			#endregion	// DomainUpDownAccessibleObject Local Variables

			#region DomainUpDownAccessibleObject Constructors
			public DomainUpDownAccessibleObject(Control owner) : base(owner)
			{
				//this.owner = owner;
			}
			#endregion	// DomainUpDownAccessibleObject Constructors

			#region DomainUpDownAccessibleObject Properties
			public override AccessibleRole Role {
				get {
					return base.Role;
				}
			}
			#endregion	// DomainUpDownAccessibleObject Properties

			#region DomainUpDownAccessibleObject Methods
			public override AccessibleObject GetChild(int index) {
				return base.GetChild (index);
			}

			public override int GetChildCount() {
				return base.GetChildCount ();
			}
			#endregion	// DomainUpDownAccessibleObject Methods
		}
		#endregion	// DomainUpDownAccessibleObject sub-class

		#region	DomainUpDownItemCollection sub-class
		public class DomainUpDownItemCollection : ArrayList {
			#region Local Variables
			#endregion	// Local Variables

			#region Constructors
			internal DomainUpDownItemCollection() {}
			#endregion	// Constructors

			#region Public Instance Properties
			[Browsable(false)]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public override object this[int index] {
				get {
					return base[index];
				}

				set {
					if (value == null) {
						throw new ArgumentNullException("value", "Cannot add null values to a DomainUpDownItemCollection");
					}

					base[index] = value;
					OnCollectionChanged(index, 0);
				}
			}
			#endregion	// Public Instance Properties

			#region Public Instance Methods
			public override int Add(object item) {
				if (item == null)
					throw new ArgumentNullException("value", "Cannot add null values to a DomainUpDownItemCollection");

				int ret = base.Add(item);
				OnCollectionChanged(Count - 1, +1);
				return ret;
			}

			public override void Insert(int index, object item) {
				if (item == null)
					throw new ArgumentNullException("value", "Cannot add null values to a DomainUpDownItemCollection");

				base.Insert(index, item);
				OnCollectionChanged(index, +1);
			}

			public override void Remove(object item) {
				int index = IndexOf(item);

				if (index >= 0)
					RemoveAt(index);
			}

			public override void RemoveAt(int item) {
				base.RemoveAt(item);
				OnCollectionChanged(item, -1);
			}
			#endregion	// Public Instance Methods

			#region Internal Methods and Events
			internal void OnCollectionChanged(int index, int size_delta) {
				CollectionChangedEventHandler handler = CollectionChanged;

				if (handler != null) {
					handler(index, size_delta);
				}
			}

			internal void PrivSort()
			{
				base.Sort (new ToStringSorter ());
			}

			private class ToStringSorter : IComparer
			{
				public int Compare (object x, object y)
				{
					return string.Compare (x.ToString (), y.ToString ());
				}
			}
			
			internal event CollectionChangedEventHandler CollectionChanged;
			#endregion	// Internal Methods and Events
		}
		#endregion	// DomainUpDownItemCollection sub-class

		#region Private Methods
		// normally I'd use an EventArgs class, but I don't want to create spurious objects here
		internal delegate void	CollectionChangedEventHandler(int index, int size_delta);

		internal void items_CollectionChanged(int index, int size_delta) {
			bool new_item = false;

			if ((index == selected_index) && (size_delta <= 0))
				new_item = true;
			else if (index <= selected_index)
				selected_index += size_delta;

			if (sorted && (index >= 0)) // index < 0 means it is already sorting
				items.PrivSort();

			// XXX this might be wrong - it might be an explict 'Text = ...' assignment.
			UpdateEditText();

			if (new_item) {
				OnSelectedItemChanged(this, EventArgs.Empty);
			}
		}

		void go_to_user_input() {
			UserEdit = false;

			if (typed_to_index >= 0) {
				selected_index = typed_to_index;
				OnSelectedItemChanged(this, EventArgs.Empty);
			}
		}

		private void TextBoxLostFocus(object source, EventArgs e) {
			Select(base.txtView.SelectionStart + base.txtView.SelectionLength, 0);
		}

		int SearchTextWithPrefix (char key_char)
		{
			string prefix = key_char.ToString ();
			int start_index, i;

			start_index = selected_index == -1 ? 0 : selected_index;
			i = selected_index == -1 || selected_index + 1 >= items.Count ? 0 : start_index + 1;

			while (true) {
				string item_text = items [i].ToString ();
				if (String.Compare (prefix, 0, item_text, 0, 1, true) == 0)
					return i;

				if (i + 1 >= items.Count)
					i = 0;
				else
					i++;

				if (i == start_index)
					break;
			}

			return -1;
		}

		bool IsValidInput (char key_char)
		{
			return Char.IsLetterOrDigit (key_char)
					|| Char.IsNumber (key_char)
					|| Char.IsPunctuation (key_char)
					|| Char.IsSymbol (key_char)
					|| Char.IsWhiteSpace (key_char);
		}

		private void TextBoxKeyDown(object source, KeyPressEventArgs e) {
			if (ReadOnly) {
				char key_char = e.KeyChar;
				if (IsValidInput (key_char) && items.Count > 0) {
					int idx = SearchTextWithPrefix (key_char);
					if (idx > -1) {
						SelectedIndex = idx;
						e.Handled = true;
					}
				}

				return;
			}

			if (!UserEdit) {
				base.txtView.SelectionLength = 0;
				typed_to_index = -1;
			}

			if (base.txtView.SelectionLength == 0) {
				base.txtView.SelectionStart = 0;
			}

			if (base.txtView.SelectionStart != 0) {
				return;
			}

			if (e.KeyChar == '\b') { // backspace
				if (base.txtView.SelectionLength > 0) {
					string prefix = base.txtView.SelectedText.Substring(0, base.txtView.SelectionLength - 1);

					bool found = false;

					if (typed_to_index < 0) {
						typed_to_index = 0;
					}

					if (sorted) {
						for (int i=typed_to_index; i >= 0; i--) {
							int difference = string.Compare(prefix, 0, items[i].ToString(), 0, prefix.Length, true);

							if (difference == 0) {
								found = true;
								typed_to_index = i;
							}

							if (difference > 0) { // since it is sorted, no strings after this point will match
								break;
							}
						}
					} else {
						for (int i=0; i < items.Count; i++) {
							if (0 == string.Compare(prefix, 0, items[i].ToString(), 0, prefix.Length, true)) {
								found = true;
								typed_to_index = i;
								break;
							}
						}
					}

					ChangingText = true;

					if (found)
						Text = items[typed_to_index].ToString();
					else
						Text = prefix;

					Select(0, prefix.Length);

					UserEdit = true;

					e.Handled = true;
				}
			}
			else {
				char key_char = e.KeyChar;

				if (IsValidInput (key_char)) {
					string prefix = base.txtView.SelectedText + key_char;

					bool found = false;

					if (typed_to_index < 0) {
						typed_to_index = 0;
					}

					if (sorted) {
						for (int i=typed_to_index; i < items.Count; i++) {
							int difference = string.Compare(prefix, 0, items[i].ToString(), 0, prefix.Length, true);

							if (difference == 0) {
								found = true;
								typed_to_index = i;
							}

							if (difference <= 0) { // since it is sorted, no strings after this point will match
								break;
							}
						}
					} else {
						for (int i=0; i < items.Count; i++) {
							if (0 == string.Compare(prefix, 0, items[i].ToString(), 0, prefix.Length, true)) {
								found = true;
								typed_to_index = i;
								break;
							}
						}
					}

					ChangingText = true;

					if (found) {
						Text = items[typed_to_index].ToString();
					} else {
						Text = prefix;
					}

					Select(0, prefix.Length);

					UserEdit = true;

					e.Handled = true;
				}
			}
		}
		#endregion	// Private Methods

		#region Public Constructors
		public DomainUpDown() {
			selected_index = -1;
			sorted = false;
			wrap = false;
			typed_to_index = -1;

			items = new DomainUpDownItemCollection();
			items.CollectionChanged += new CollectionChangedEventHandler(items_CollectionChanged);

			this.txtView.LostFocus +=new EventHandler(TextBoxLostFocus);
			this.txtView.KeyPress += new KeyPressEventHandler(TextBoxKeyDown);

			UpdateEditText ();
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		public DomainUpDownItemCollection Items {
			get {
				return items;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return Padding.Empty; }
			set { }
		}

		[Browsable(false)]
		[DefaultValue(-1)]
		public int SelectedIndex {
			get {
				return selected_index;
			}
			set {
				object before = (selected_index >= 0) ? items[selected_index] : null;

				selected_index = value;
				UpdateEditText();

				object after = (selected_index >= 0) ? items[selected_index] : null;

				if (!ReferenceEquals(before, after)) {
					OnSelectedItemChanged(this, EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object SelectedItem {
			get {
				if (selected_index >= 0) {
					return items[selected_index];
				} else {
					return null;
				}
			}

			set {
				SelectedIndex = items.IndexOf(value);
			}
		}

		[DefaultValue(false)]
		public bool Sorted {
			get {
				return sorted;
			}
			set {
				sorted = value;

				if (sorted)
					items.PrivSort();
			}
		}

		[DefaultValue(false)]
		[Localizable(true)]
		public bool Wrap {
			get {
				return wrap;
			}
			set {
				wrap = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override void DownButton() {
			if (UserEdit)
				go_to_user_input();

			int new_index = selected_index + 1;

			if (new_index >= items.Count) {
				if (!wrap)
					return;

				new_index = 0;
			}

			SelectedIndex = new_index;

			// UIA Framework Event: DownButton Click
			OnUIADownButtonClick (EventArgs.Empty);
		}

		public override string ToString() {
			return base.ToString() + ", Items.Count: " + items.Count + ", SelectedIndex: " + selected_index;
		}

		public override void UpButton() {
			if (UserEdit)
				go_to_user_input();

			int new_index = selected_index - 1;

			if (new_index < 0) {
				if (!wrap) {
					return;
				}

				new_index = items.Count - 1;
			}

			SelectedIndex = new_index;

			// UIA Framework Event: UpButton Click
			OnUIAUpButtonClick (EventArgs.Empty);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override AccessibleObject CreateAccessibilityInstance() {
			AccessibleObject	acc;

			acc = new AccessibleObject(this);
			acc.role = AccessibleRole.SpinButton;

			return acc;
		}

		protected override void OnChanged(object source, EventArgs e) {
			base.OnChanged (source, e);
		}

		protected void OnSelectedItemChanged(object source, EventArgs e) {
			EventHandler eh = (EventHandler)(Events [SelectedItemChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void UpdateEditText() {
			if ((selected_index >= 0) && (selected_index < items.Count)) {
				ChangingText = true;
				Text = items[selected_index].ToString();
			}
		}

		protected override void OnTextBoxKeyPress (object source, KeyPressEventArgs e)
		{
			base.OnTextBoxKeyPress (source, e);
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}

		static object SelectedItemChangedEvent = new object ();
		public event EventHandler SelectedItemChanged {
			add { Events.AddHandler (SelectedItemChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedItemChangedEvent, value); }
		}
		#endregion	// Events
	}
}
