//
// System.Web.UI.ControlCollection.cs
//
// Authors:
//	Duncan Mak  (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2002-2010 Novell, Inc. (http://www.novell.com)
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

using System.Collections;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ControlCollection : ICollection, IEnumerable
	{
		Control owner;
		Control [] controls;
		int version;
		int count;
		bool readOnly;
		
		public ControlCollection (Control owner)
		{
			if (owner == null)
				throw new ArgumentException ("owner");

			this.owner = owner;
		}

		public virtual int Count {
			get { return count; }
		}

		public bool IsReadOnly {
			get { return readOnly; }
		}

		public bool  IsSynchronized {
			get { return false; }
		}

		public virtual Control this [int index] {
			get {
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException ("index");

				return controls [index];
			}
		}

		protected Control Owner {
			get { return owner; }
		}

		public object SyncRoot {
			get { return this; }
		}

		void EnsureControls ()
		{
			if (controls == null) {
				controls = new Control [5];
			} else if (controls.Length < count + 1) {
				int n = controls.Length == 5 ? 3 : 2;
				Control [] newControls = new Control [controls.Length * n];
				Array.Copy (controls, 0, newControls, 0, controls.Length);
				controls = newControls;
			}
		}

		public virtual void Add (Control child)
		{
			if (child == null)
				throw new ArgumentNullException ("child");

			if (readOnly)
				throw new HttpException (Locale.GetText ("Collection is read-only."));

			if (Object.ReferenceEquals (owner, child))
				throw new HttpException (Locale.GetText ("Cannot add collection's owner."));

			EnsureControls ();
			version++;
			controls [count++] = child;
			owner.AddedControl (child, count - 1);
		}

		public virtual void AddAt (int index, Control child)
		{
			if (child == null) // maybe we should check for ! (child is Control)?
				throw new ArgumentNullException ();
			
			if (index < -1 || index > count)
				throw new ArgumentOutOfRangeException ();

			if (readOnly)
				throw new HttpException (Locale.GetText ("Collection is read-only."));

			if (Object.ReferenceEquals (owner, child))
				throw new HttpException (Locale.GetText ("Cannot add collection's owner."));

			if (index == -1) {
				Add (child);
				return;
			}

			EnsureControls ();
			version++;
			Array.Copy (controls, index, controls, index + 1, count - index);
			count++;
			controls [index] = child;
			owner.AddedControl (child, index);
		}

		public virtual void Clear ()
		{
			if (controls == null)
				return;

			version++;
			for (int i = 0; i < count; i++)
				owner.RemovedControl (controls [i]);

			count = 0;
			if (owner != null)
				owner.ResetChildNames ();
		}

		public virtual bool Contains (Control c)
		{
			return (controls != null && Array.IndexOf (controls, c) != -1);
		}

		public virtual void CopyTo (Array array, int index)
		{
			if (controls == null)
				return;

			// can't use controls.CopyTo (array, index);
			// as we do not allocate it based on the true 
			// numbers of items we have in the collection
			// so we must re-implement Array.CopyTo :(

			if (array == null)
				throw new ArgumentNullException ("array");
			if (index + count > array.GetLowerBound (0) + array.GetLength (0))
				throw new ArgumentException ();
			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", Locale.GetText ("Value has to be >= 0."));

			for (int i=0; i < count; i++)
				array.SetValue (controls [i], i + index);
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return new SimpleEnumerator (this);
		}

		public virtual int IndexOf (Control value)
		{
			if (controls == null || value == null)
				return -1;

			return Array.IndexOf (controls, value);
		}

		public virtual void Remove (Control value)
		{
			int idx = IndexOf (value);
			if (idx == -1)
				return;
			RemoveAt (idx);
		}

		public virtual void RemoveAt (int index)
		{
			if (readOnly)
				throw new HttpException ();

			version++;
			Control ctrl = controls [index];
			count--;
			if (count - index > 0)
				Array.Copy (controls, index + 1, controls, index, count - index);

			controls [count] = null;
			owner.RemovedControl (ctrl);
		}
		
		internal void SetReadonly (bool readOnly)
		{
			this.readOnly = readOnly;
		}

		// Almost the same as in ArrayList
		sealed class SimpleEnumerator : IEnumerator
		{
			ControlCollection coll;
			int index;
			int version;
			object currentElement;
							
			public SimpleEnumerator (ControlCollection coll)
			{
				this.coll = coll;
				index = -1;
				version = coll.version;
			}
	
			public bool MoveNext ()
			{
				if (version != coll.version)
					throw new InvalidOperationException ("List has changed.");
				
				if (index >= -1 && ++index < coll.Count) {
					currentElement = coll [index];
					return true;
				} else {
					index = -2;
					return false;
				}
			}
	
			public object Current {
				get {
					if (index < 0)
						throw new InvalidOperationException (index == -1 ? "Enumerator not started" : "Enumerator ended");
					
					return currentElement;
				}
			}
	
			public void Reset ()
			{
				if (version != coll.version)
					throw new InvalidOperationException ("List has changed.");
				
				index = -1;
			}
		}
	}
}

