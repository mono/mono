//
// Mainsoft.Web.Hosting.AbstractAttributeMap.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2008 Mainsoft Co. (http://www.mainsoft.com)
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

using java.util;
using System;

namespace Mainsoft.Web.Hosting
{
	partial class BaseExternalContext
	{
		public abstract class AbstractAttributeMap : AbstractMap
		{
			private Set _keySet;
			private Collection _values;
			private Set _entrySet;

			public override void clear () {
				List names = new ArrayList ();
				for (Enumeration e = getAttributeNames (); e.hasMoreElements (); ) {
					names.add (e.nextElement ());
				}

				for (Iterator it = names.iterator (); it.hasNext (); ) {
					removeAttribute ((String) it.next ());
				}
			}

			public override bool containsKey (Object key) {
				return getAttribute (key.ToString ()) != null;
			}

			public override bool containsValue (Object findValue) {
				if (findValue == null) {
					return false;
				}

				for (Enumeration e = getAttributeNames (); e.hasMoreElements (); ) {
					Object value = getAttribute ((String) e.nextElement ());
					if (findValue.Equals (value)) {
						return true;
					}
				}

				return false;
			}

			public override Set entrySet () {
				return (_entrySet != null) ? _entrySet : (_entrySet = new EntrySet (this));
			}

			public override Object get (Object key) {
				return getAttribute (key.ToString ());
			}

			public override bool isEmpty () {
				return !getAttributeNames ().hasMoreElements ();
			}

			public override Set keySet () {
				return (_keySet != null) ? _keySet : (_keySet = new KeySet (this));
			}

			public override Object put (Object key, Object value) {
				String key_ = key.ToString ();
				Object retval = getAttribute (key_);
				setAttribute (key_, value);
				return retval;
			}

			public override void putAll (Map t) {
				for (Iterator it = t.entrySet ().iterator (); it.hasNext (); ) {
					Map.Entry entry = (Map.Entry) it.next ();
					setAttribute (entry.getKey ().ToString (), entry.getValue ());
				}
			}

			public override Object remove (Object key) {
				String key_ = key.ToString ();
				Object retval = getAttribute (key_);
				removeAttribute (key_);
				return retval;
			}

			public override int size () {
				int size = 0;
				for (Enumeration e = getAttributeNames (); e.hasMoreElements (); ) {
					size++;
					e.nextElement ();
				}
				return size;
			}

			public override Collection values () {
				return (_values != null) ? _values : (_values = new Values (this));
			}


			abstract protected Object getAttribute (string key);

			abstract protected void setAttribute (string key, Object value);

			abstract protected void removeAttribute (string key);

			abstract protected Enumeration getAttributeNames ();


			private class KeySet : AbstractSet
			{
				protected readonly AbstractAttributeMap _owner;
				public KeySet (AbstractAttributeMap owner) {
					_owner = owner;
				}

				public override Iterator iterator () {
					return new KeyIterator (_owner);
				}

				public override bool isEmpty () {
					return _owner.isEmpty ();
				}

				public override int size () {
					return _owner.size ();
				}

				public override bool contains (Object o) {
					return _owner.containsKey (o);
				}

				public override bool remove (Object o) {
					return _owner.remove (o) != null;
				}

				public override void clear () {
					_owner.clear ();
				}
			}

			private class KeyIterator : Iterator
			{
				protected readonly AbstractAttributeMap _owner;
				protected readonly Enumeration _e;

				public KeyIterator (AbstractAttributeMap owner) {
					_owner = owner;
					_e = _owner.getAttributeNames ();
				}

				protected Object _currentKey;

				public virtual void remove () {
					// remove() may cause ConcurrentModificationException.
					// We could throw an exception here, but not throwing an exception
					//   allows one call to remove() to succeed
					if (_currentKey == null) {
						throw new NoSuchElementException (
							"You must call next() at least once");
					}
					_owner.remove (_currentKey);
				}

				public bool hasNext () {
					return _e.hasMoreElements ();
				}

				public virtual Object next () {
					return _currentKey = _e.nextElement ();
				}
			}

			private class Values : KeySet
			{

				public Values (AbstractAttributeMap owner)
					: base (owner) {
				}

				public override Iterator iterator () {
					return new ValuesIterator (_owner);
				}

				public override bool contains (Object o) {
					return _owner.containsValue (o);
				}

				public override bool remove (Object o) {
					if (o == null) {
						return false;
					}

					for (Iterator it = iterator (); it.hasNext (); ) {
						if (o.Equals (it.next ())) {
							it.remove ();
							return true;
						}
					}

					return false;
				}
			}

			private class ValuesIterator : KeyIterator
			{
				public ValuesIterator (AbstractAttributeMap owner)
					: base (owner) { }
				public override Object next () {
					base.next ();
					return _owner.get (_currentKey);
				}
			}

			private class EntrySet : KeySet
			{
				public EntrySet (AbstractAttributeMap owner) : base (owner) { }

				public override Iterator iterator () {
					return new EntryIterator (_owner);
				}

				public override bool contains (Object o) {
					if (!(o is Map.Entry)) {
						return false;
					}

					Map.Entry entry = (Map.Entry) o;
					Object key = entry.getKey ();
					Object value = entry.getValue ();
					if (key == null || value == null) {
						return false;
					}

					return value.Equals (_owner.get (key));
				}

				public override bool remove (Object o) {
					if (!(o is Map.Entry)) {
						return false;
					}

					Map.Entry entry = (Map.Entry) o;
					Object key = entry.getKey ();
					Object value = entry.getValue ();
					if (key == null || value == null
						|| !value.Equals (_owner.get (key))) {
						return false;
					}

					return _owner.remove (((Map.Entry) o).getKey ()) != null;
				}
			}

			/**
			 * Not very efficient since it generates a new instance of <code>Entry</code>
			 * for each element and still internaly uses the <code>KeyIterator</code>.
			 * It is more efficient to use the <code>KeyIterator</code> directly.
			 */
			private class EntryIterator : KeyIterator
			{
				public EntryIterator (AbstractAttributeMap owner) : base (owner) { }
				public override Object next () {
					base.next ();
					// Must create new Entry every time--value of the entry must stay
					// linked to the same attribute name
					return new EntrySetEntry (_currentKey);
				}
			}

			private class EntrySetEntry : Map.Entry
			{
				readonly AbstractAttributeMap _owner;

				public EntrySetEntry (AbstractAttributeMap owner) {
					_owner = owner;
				}

				private readonly Object _currentKey;

				public EntrySetEntry (Object currentKey) {
					_currentKey = currentKey;
				}

				public Object getKey () {
					return _currentKey;
				}

				public Object getValue () {
					return _owner.get (_currentKey);
				}

				public Object setValue (Object value) {
					return _owner.put (_currentKey, value);
				}

				public int hashCode () {
					return _currentKey == null ? 0 : _currentKey.GetHashCode ();
				}

				public bool equals (Object obj) {
					if (!(obj is EntrySetEntry))
						return false;
					return _currentKey != null && _currentKey.Equals (obj);
				}
			}
		}
	}
}