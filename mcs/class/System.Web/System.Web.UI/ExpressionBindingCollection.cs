//
// System.Web.UI.ExpressionBindingCollection.cs
//
// Authors:
// 	Sanjay Gupta gsanjay@novell.com)
//
// (C) 2004-2010 Novell, Inc. (http://www.novell.com)
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

using System;
using System.ComponentModel;
using System.Collections;

namespace System.Web.UI {
    	
	public sealed class ExpressionBindingCollection : ICollection, IEnumerable
    	{
		static readonly object changedEvent = new object ();
		
		Hashtable list;
		ArrayList removed;

		EventHandlerList events = new EventHandlerList ();
		
        	public event EventHandler Changed {
			add { events.AddHandler (changedEvent, value); }
			remove { events.RemoveHandler (changedEvent, value); }
		}
		
		public ExpressionBindingCollection ()
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

		public ExpressionBinding this [string propertyName] {
            		get { return list [propertyName] as ExpressionBinding; }
        	}

		public ICollection RemovedBindings {
			get { return removed; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public void Add (ExpressionBinding binding)
		{
			list.Add (binding.PropertyName, binding);
            		OnChanged (new EventArgs ());
        	}

		public void Clear ()
		{
			list.Clear ();
            		removed.Clear ();
            		OnChanged (new EventArgs ());
        	}

        	public bool Contains (string propName)
        	{
            		return list.Contains (propName);
        	}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

        	public void CopyTo (ExpressionBinding [] array, int index)
        	{
            		if (index < 0)
                		throw new ArgumentNullException ("Index cannot be negative");
            		if (index >= array.Length)
                		throw new ArgumentException ("Index cannot be greater than or equal to length of array passed");            
            		if (list.Count > (array.Length - index + 1))
                		throw new ArgumentException ("Number of elements in source is greater than available space from index to end of destination");
            
            		foreach (string key in list.Keys)
                		array [index++] = (ExpressionBinding) list [key];
        	}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public void Remove (ExpressionBinding binding)
		{
			Remove(binding.PropertyName, true);
        	}

		public void Remove (string propertyName)
		{
			Remove (propertyName, true);            
        	}

		public void Remove (string propertyName, bool addToRemovedList)
		{
			if (addToRemovedList)
				removed.Add (String.Empty); 
			else
				removed.Add (propertyName);

			list.Remove (propertyName);
            		OnChanged (new EventArgs ());
        	}

        	void OnChanged (EventArgs e)   
        	{
			EventHandler eh = events [changedEvent] as EventHandler;
            		if (eh != null)
                		eh (this, e);
        	}        

    	}
}
