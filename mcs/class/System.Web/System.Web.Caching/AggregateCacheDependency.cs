//
// System.Web.Compilation.AggregateCacheDependency
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace System.Web.Caching 
{
	public sealed class AggregateCacheDependency : CacheDependency
	{
		object dependenciesLock = new object();
		List <CacheDependency> dependencies;
		
		public AggregateCacheDependency ()
		{
			FinishInit ();
		}

		public void Add (params CacheDependency [] dependencies)
		{
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			if (dependencies.Length == 0)
				return;
			
			bool somethingChanged = false;
			foreach (CacheDependency dep in dependencies)
				if (dep == null || dep.IsUsed)
					throw new InvalidOperationException ("Cache dependency already in use");
				else if (!somethingChanged && dep != null && dep.HasChanged)
					somethingChanged = true;

			lock (dependenciesLock) {
				if (this.dependencies == null)
					this.dependencies = new List <CacheDependency> (dependencies.Length);
				foreach (CacheDependency dep in dependencies)
					if (dep != null)
						dep.DependencyChanged += new EventHandler (OnAnyChanged);
				
				this.dependencies.AddRange (dependencies);
				base.Start = DateTime.Now;
			}
			if (somethingChanged)
				base.NotifyDependencyChanged (this, null);
		}

		public override string GetUniqueID ()
		{
			if (dependencies == null || dependencies.Count == 0)
				return null;
			
			StringBuilder sb = new StringBuilder ();
			lock (dependenciesLock) {
				string depid = null;
				foreach (CacheDependency dep in dependencies) {
					depid = dep.GetUniqueID ();
					if (String.IsNullOrEmpty (depid))
						return null;
					sb.Append (depid);
					sb.Append (';');
				}
			}
			return sb.ToString ();
		}

		protected override void DependencyDispose ()
		{
			// MSDN doesn't document it as being part of the class, but assembly
			// comparison shows that it does exist in this type, so we're just calling
			// the base class here
			base.DependencyDispose ();
		}
		
		internal override void DependencyDisposeInternal ()
		{
			if (dependencies != null && dependencies.Count > 0)
				foreach (CacheDependency dep in dependencies)
					dep.DependencyChanged -= new EventHandler (OnAnyChanged);
		}
		
		void OnAnyChanged (object sender, EventArgs args)
		{
			base.NotifyDependencyChanged (sender, args);
		}
	}
}



