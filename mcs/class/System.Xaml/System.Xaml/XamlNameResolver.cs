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

		internal class NameFixupReguired
		{
			public NameFixupReguired (string name)
			{
				Name = name;
			}
			
			public string Name { get; set; }
		}

		List<NamedObject> objects = new List<NamedObject> ();

		[MonoTODO]
		public bool IsFixupTokenAvailable {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public event EventHandler OnNameScopeInitializationComplete;

		internal void AddNamedObject (string name, object value, bool fullyInitialized)
		{
			objects.Add (new NamedObject (name, value, fullyInitialized));
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
			foreach (var no in objects)
				yield return new KeyValuePair<string,object> (no.Name, no.Value);
		}

		public object Resolve (string name)
		{
			var ret = objects.FirstOrDefault (no => no.Name == name);
			return ret != null ? ret.Value : new NameFixupReguired (name);
		}

		public object Resolve (string name, out bool isFullyInitialized)
		{
			var ret = objects.FirstOrDefault (no => no.Name == name);
			isFullyInitialized = ret != null ? ret.FullyInitialized : false;
			return ret != null ? ret.Value : new NameFixupReguired (name);
		}
	}
}

