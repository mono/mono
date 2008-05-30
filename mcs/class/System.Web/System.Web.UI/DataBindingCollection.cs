//
// System.Web.UI.DataBindingCollection.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Collections;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class DataBindingCollection : ICollection, IEnumerable
	{
		static readonly object changedEvent = new object ();
		Hashtable list;
		ArrayList removed;

		EventHandlerList events = new EventHandlerList ();
#if NET_2_0
		public 
#else
		internal
#endif
		event EventHandler Changed {
			add { events.AddHandler (changedEvent, value); }
			remove { events.RemoveHandler (changedEvent, value); }
		}
		
		public DataBindingCollection ()
		{
			list = new Hashtable ();
			removed = new ArrayList ();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public DataBinding this [string propertyName] {
			get { return list [propertyName] as DataBinding; }
		}

		public string [] RemovedBindings {
			get { return (string []) removed.ToArray (typeof (string)); }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public void Add (DataBinding binding)
		{
			list.Add (binding.PropertyName, binding);
			RaiseChanged ();
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public void CopyTo (Array array, int index)
		{
			list.Values.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public void Remove (DataBinding binding)
		{
			string key = binding.PropertyName;
			Remove (key);
		}

		public void Remove (string propertyName)
		{
			removed.Add (propertyName);
			list.Remove (propertyName);
			RaiseChanged ();
		}

		public void Remove (string propertyName,
				    bool addToRemovedList)
		{
			if (addToRemovedList)
				removed.Add (String.Empty); // LAMESPEC
			else
				removed.Add (propertyName);

			list.Remove (propertyName);
		}

#if NET_2_0
		public bool Contains (string propertyName)
		{
			return list.Contains (propertyName);
		}
#endif

		internal void RaiseChanged ()
		{
			EventHandler eh = events [changedEvent] as EventHandler;
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
	}
}
