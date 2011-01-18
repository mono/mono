//
// ToolStripItemCollection.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0

using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	[ListBindable (false)]
	[Editor ("System.Windows.Forms.Design.ToolStripCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public class ToolStripItemCollection : ArrangedElementCollection, IList, ICollection, IEnumerable
	{
		private ToolStrip owner;
		private bool internal_created;
		
		#region Public Constructor
		public ToolStripItemCollection (ToolStrip owner, ToolStripItem[] value) : base ()
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");

			if (value == null)
				throw new ArgumentNullException ("toolStripItems");

			this.owner = owner;

			foreach (ToolStripItem tsi in value)
				this.AddNoOwnerOrLayout (tsi);
		}

		internal ToolStripItemCollection (ToolStrip owner, ToolStripItem[] value, bool internalcreated) : base ()
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");

			this.internal_created = internalcreated;
			this.owner = owner;
			
			if (value != null)
				foreach (ToolStripItem tsi in value)
					this.AddNoOwnerOrLayout (tsi);
		}
		#endregion

		#region Public Properties
		public override bool IsReadOnly { get { return base.IsReadOnly; } }
		
		public new virtual ToolStripItem this[int index] { get { return (ToolStripItem)base[index]; } }
		
		public virtual ToolStripItem this[string key] {
			get {
				foreach (ToolStripItem tsi in this)
					if (tsi.Name == key)
						return tsi;

				return null;
			}
		}
		#endregion

		#region Public Methods
		public ToolStripItem Add (Image image)
		{
			ToolStripItem tsb = owner.CreateDefaultItem (string.Empty, image, null);
			this.Add (tsb);
			return tsb;
		}

		public ToolStripItem Add (string text)
		{
			ToolStripItem tsb = owner.CreateDefaultItem (text, null, null);
			this.Add (tsb);
			return tsb;
		}

		public int Add (ToolStripItem value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			if (Contains (value))
				return IndexOf (value);

			value.InternalOwner = owner;
				
			if (value is ToolStripMenuItem && (value as ToolStripMenuItem).ShortcutKeys != Keys.None)
				ToolStripManager.AddToolStripMenuItem ((ToolStripMenuItem)value);
				
			int index = base.Add (value);
			
			if (this.internal_created)
				owner.OnItemAdded (new ToolStripItemEventArgs (value));
				
			return index;
		}

		public ToolStripItem Add (string text, Image image)
		{
			ToolStripItem tsb = owner.CreateDefaultItem (text, image, null);
			this.Add (tsb);
			return tsb;
		}

		public ToolStripItem Add (string text, Image image, EventHandler onClick)
		{
			ToolStripItem tsb = owner.CreateDefaultItem (text, image, onClick);
			this.Add (tsb);
			return tsb;
		}

		public void AddRange (ToolStripItem[] toolStripItems)
		{
			if (toolStripItems == null)
				throw new ArgumentNullException ("toolStripItems");
			if (this.IsReadOnly)
				throw new NotSupportedException ("This collection is read-only");

			this.owner.SuspendLayout ();

			foreach (ToolStripItem tsi in toolStripItems)
				this.Add (tsi);

			this.owner.ResumeLayout ();
		}

		public void AddRange (ToolStripItemCollection toolStripItems)
		{
			if (toolStripItems == null)
				throw new ArgumentNullException ("toolStripItems");
			if (this.IsReadOnly)
				throw new NotSupportedException ("This collection is read-only");

			this.owner.SuspendLayout ();

			foreach (ToolStripItem tsi in toolStripItems)
				this.Add (tsi);

			this.owner.ResumeLayout ();
		}

		public new virtual void Clear ()
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("This collection is read-only");

			if (internal_created)
				foreach (ToolStripItem item in this) {
					item.InternalOwner = null;
					item.Parent = null;
				}

			base.Clear ();
			owner.PerformLayout ();
		}

		// Don't modify Owner or Parent - used by internal collection instances.
		internal void ClearInternal ()
		{
			base.Clear ();
			owner.PerformLayout ();
		}

		public bool Contains (ToolStripItem value)
		{
			return base.Contains (value);
		}

		public virtual bool ContainsKey (string key)
		{
			return this[key] != null;
		}

		public void CopyTo (ToolStripItem[] array, int index)
		{
			base.CopyTo (array, index);
		}

		[MonoTODO ("searchAllChildren parameter isn't used")]
		public ToolStripItem[] Find (string key, bool searchAllChildren)
		{
			if (key == null || key.Length == 0)
				throw new ArgumentNullException ("key");

			List<ToolStripItem> list = new List<ToolStripItem> ();

			foreach (ToolStripItem tsi in this) {
				if (String.Compare (tsi.Name, key, true) == 0) {
					list.Add (tsi);

					if (searchAllChildren) {
						// TODO: tsi does not have an items property yet..
					}
				}
			}

			return list.ToArray ();
		}

		public int IndexOf (ToolStripItem value)
		{
			return base.IndexOf (value);
		}

		public virtual int IndexOfKey (string key)
		{
			ToolStripItem tsi = this[key];

			if (tsi == null)
				return -1;

			return this.IndexOf (tsi);
		}

		public void Insert (int index, ToolStripItem value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (value is ToolStripMenuItem && (value as ToolStripMenuItem).ShortcutKeys != Keys.None)
				ToolStripManager.AddToolStripMenuItem ((ToolStripMenuItem)value);

			if (value.Owner != null)
				value.Owner.Items.Remove (value);
				
			base.Insert (index, value);
			
			if (internal_created) {
				value.InternalOwner = owner;
				owner.OnItemAdded (new ToolStripItemEventArgs (value));
			}
			
			if (owner.Created)
				owner.PerformLayout ();
		}

		public void Remove (ToolStripItem value)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("This collection is read-only");

			base.Remove (value);
			
			if (value != null && internal_created) {
				value.InternalOwner = null;
				value.Parent = null;
			}
			
			if (internal_created)
				owner.OnItemRemoved (new ToolStripItemEventArgs (value));
			
			if (owner.Created)	
				owner.PerformLayout ();
		}

		public void RemoveAt (int index)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("This collection is read-only");

			ToolStripItem tsi = (ToolStripItem)base[index];
			this.Remove (tsi);
		}

		public virtual void RemoveByKey (string key)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("This collection is read-only");

			ToolStripItem tsi = this[key];

			if (tsi != null)
				this.Remove (tsi);

			return;
		}
		#endregion

		#region Internal Methods
		// When we create DisplayedItems, we don't want to modify the item's
		// parent or trigger a layout.
		internal int AddNoOwnerOrLayout (ToolStripItem value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			int index = base.Add (value);
			return index;
		}

		internal void InsertNoOwnerOrLayout (int index, ToolStripItem value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (index > Count)
				base.Add (value);
			else
				base.Insert (index, value);
		}

		internal void RemoveNoOwnerOrLayout (ToolStripItem value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			base.Remove (value);
		}
		#endregion
		
		#region IList Members
		int IList.Add (object value)
		{
			return this.Add ((ToolStripItem)value);
		}

		void IList.Clear ()
		{
			this.Clear ();
		}

		bool IList.Contains (object value)
		{
			return this.Contains ((ToolStripItem)value);
		}

		int IList.IndexOf (object value)
		{
			return this.IndexOf ((ToolStripItem)value);
		}

		void IList.Insert (int index, object value)
		{
			this.Insert (index, (ToolStripItem)value);
		}

		bool IList.IsFixedSize {
			get { return this.IsFixedSize; }
		}

		void IList.Remove (object value)
		{
			this.Remove ((ToolStripItem)value); ;
		}

		void IList.RemoveAt (int index)
		{
			this.RemoveAt (index);
		}

		object IList.this[int index] {
			get { return this[index]; }
			set { throw new NotSupportedException (); }
		}
		#endregion
	}
}
#endif
