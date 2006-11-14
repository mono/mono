//
// System.Web.Caching.AggregateCacheDependency
//
// Author(s):
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Text;

namespace System.Web.Caching {
	public sealed class AggregateCacheDependency : CacheDependency {
		bool changed;
		DateTime last_modified;
		CacheDependency [] deps;

		public AggregateCacheDependency ()
		{
		}

		void OnChanged (object sender, EventArgs args)
		{
			changed = true;
			last_modified = DateTime.UtcNow;
			OnDependencyChanged ();
		}

		public bool HasChanged {
			get { return changed; }
		}

		public DateTime UtcLastModified {
			get { return last_modified; }
		}

		public void Add (params CacheDependency [] dependencies)
		{
			if (dependencies == null)
				return;

			deps = dependencies;
			for (int i = dependencies.Length; i >= 0; i--) {
				dependencies [i].DependencyChanged += OnChanged;
			}
		}

		public override string GetUniqueID ()
		{
			if (deps == null)
				return null;

			StringBuilder sb = new StringBuilder ();
			foreach (CacheDependency dep in deps) {
				sb.Append (dep.GetUniqueID ());				
			}

			return sb.ToString ();
		}
	}
}
#endif

