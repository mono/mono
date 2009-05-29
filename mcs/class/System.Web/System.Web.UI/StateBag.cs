// 
// System.Web.UI.StateBag
//
// Author:
//        Ben Maurer <bmaurer@novell.com>
//
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

using System.Collections;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class StateBag : IDictionary, IStateManager {

		HybridDictionary ht;
		bool track;
		
		public StateBag (bool ignoreCase)
		{
			ht = new HybridDictionary (ignoreCase);
		}
	
		public StateBag () : this (false)
		{
		}

		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}

		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}
		
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}

		bool IStateManager.IsTrackingViewState {
			get {
				return track;
			}
		}
		
		internal bool IsTrackingViewState {
			get {
				return track;
			}
		}	

		
		internal void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;
			
			foreach (DictionaryEntry de in (Hashtable) savedState)
				Add ((string) de.Key, de.Value);
		}
		
		internal object SaveViewState ()
		{
			Hashtable h = null;

			foreach (DictionaryEntry de in ht) {
				StateItem si = (StateItem) de.Value;
				if (si.IsDirty) {
					if (h == null)
						h = new Hashtable ();
					h.Add (de.Key, si.Value);
				}
			}

			return h;
		}
		
		internal void TrackViewState ()
		{
			track = true;
		}
	
		public StateItem Add (string key, object value)
		{
			StateItem si = ht [key] as StateItem;
			if (si == null)
				ht [key] = si = new StateItem (value);
			si.Value = value;
			si.IsDirty |= track;
			
			return si;
		}

		internal string GetString (string key, string def)
		{
			string s = (string) this [key];
			return s == null ? def : s;
		}
		
		internal bool GetBool (string key, bool def)
		{
			object o = this [key];
			return o == null ? def : (bool) o;
		}

		internal char GetChar (string key, char def)
		{
			object o = this [key];
			return o == null ? def : (char) o;
		}

		internal int GetInt (string key, int def)
		{
			object o = this [key];
			return o == null ? def : (int) o;
		}

		internal short GetShort (string key, short def)
		{
			object o = this [key];
			return o == null ? def : (short) o;
		}
		
		public void Clear ()
		{
			ht.Clear ();
		}
	
		public IDictionaryEnumerator GetEnumerator ()
		{
			return ht.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		public bool IsItemDirty (string key)
		{
			StateItem si = ht [key] as StateItem;
			return si != null && si.IsDirty;
		}
	
		public void Remove (string key)
		{
			ht.Remove (key);
		}
	
		public void SetItemDirty (string key, bool dirty) 
		{
			StateItem si = (StateItem) ht [key];
			if (si != null)
				si.IsDirty = dirty;
		}

		public int Count {
			get {
				return ht.Count;
			}
		}
	
		public object this [string key] {
			get {
				StateItem i = ht [key] as StateItem;
				if (i != null)
					return i.Value;
				return null;
			}
		
			set {
				if (value == null && ! IsTrackingViewState)
					Remove (key);
				else
					Add (key, value);
			}
		}
	
		public ICollection Keys {
			get {
				return ht.Keys;
			}
		
		}
	
		public ICollection Values {
			get {
				return ht.Values;
			}
		}

		void IDictionary.Add (object key, object value)
		{
			Add ((string) key, value);
		}
	
		void IDictionary.Remove (object key)
		{
			Remove ((string) key);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			ht.CopyTo (array, index);
		}
	
		bool IDictionary.Contains (object key) 
		{
			return ht.Contains (key);
		}

		bool ICollection.IsSynchronized {
			get {
				return false;
			}
		}

		object ICollection.SyncRoot {
			get {
				return ht;
			}	
		}

		object IDictionary.this [object key] {
			get {
				return this [(string) key];
			}
		
			set {
				this [(string) key] = value;
			}
		}

		bool IDictionary.IsFixedSize {
			get {
				return false;
			}
		}
	
		bool IDictionary.IsReadOnly {
			get {
				return false;
			}
		}

#if NET_2_0
		public
#else
		internal
#endif
		void SetDirty (bool dirty)
		{
			foreach (DictionaryEntry de in ht) {
				StateItem si = (StateItem) de.Value;
				si.IsDirty = dirty;
			}
		}
	}
}
