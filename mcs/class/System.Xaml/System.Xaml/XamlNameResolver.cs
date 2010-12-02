//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace System.Xaml
{
	internal class XamlNameResolver : IXamlNameResolver
	{
		public XamlNameResolver ()
		{
		}
		
		public bool IsCollectingReferences { get; set; }

		internal class NamedObject
		{
			public NamedObject (string name, object value, bool fullyInitialized)
			{
				Name = name;
				Value = value;
				FullyInitialized = fullyInitialized;
			}
			public string Name { get; set; }
			public object Value { get; set; }
			public bool FullyInitialized { get; set; }
		}
		
		internal class NameFixupRequired
		{
			public NameFixupRequired (string name)
			{
				Name = name;
			}
			
			public string Name { get; set; }
		}

		Dictionary<string,NamedObject> objects = new Dictionary<string,NamedObject> ();
		List<object> referenced = new List<object> ();

		[MonoTODO]
		public bool IsFixupTokenAvailable {
			get { throw new NotImplementedException (); }
		}

		public event EventHandler OnNameScopeInitializationComplete;

		internal void NameScopeInitializationCompleted (object sender)
		{
			if (OnNameScopeInitializationComplete != null)
				OnNameScopeInitializationComplete (sender, EventArgs.Empty);
			objects.Clear ();
		}
		
		int saved_count, saved_referenced_count;
		public void Save ()
		{
			if (saved_count != 0)
				throw new Exception ();
			saved_count = objects.Count;
			saved_referenced_count = referenced.Count;
		}
		public void Restore ()
		{
			while (saved_count < objects.Count)
				objects.Remove (objects.Last ().Key);
				referenced.Remove (objects.Last ().Key);
			saved_count = 0;
			referenced.RemoveRange (saved_referenced_count, referenced.Count - saved_referenced_count);
			saved_referenced_count = 0;
		}

		internal void SetNamedObject (string name, object value, bool fullyInitialized)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			objects [name] = new NamedObject (name, value, fullyInitialized);
		}
		
		internal bool Contains (string name)
		{
			return objects.ContainsKey (name);
		}
		
		internal string GetName (object value)
		{
			foreach (var no in objects.Values)
				if (object.ReferenceEquals (no.Value, value))
					return no.Name;
			return null;
		}

		internal void SaveAsReferenced (object val)
		{
			referenced.Add (val);
		}
		
		internal string GetReferencedName (object val)
		{
			if (!referenced.Contains (val))
				return null;
			return GetName (val);
		}
		
		[MonoTODO]
		public object GetFixupToken (IEnumerable<string> names)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetFixupToken (IEnumerable<string> names, bool canAssignDirectly)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope ()
		{
			foreach (var pair in objects)
				yield return new KeyValuePair<string,object> (pair.Key, pair.Value.Value);
		}

		public object Resolve (string name)
		{
			bool dummy;
			return Resolve (name, out dummy);
		}

		public object Resolve (string name, out bool isFullyInitialized)
		{
			NamedObject ret;
			if (objects.TryGetValue (name, out ret)) {
				isFullyInitialized = ret.FullyInitialized;
				return ret.Value;
			} else {
				isFullyInitialized = false;
				return new NameFixupRequired (name);
			}
		}
	}
}

