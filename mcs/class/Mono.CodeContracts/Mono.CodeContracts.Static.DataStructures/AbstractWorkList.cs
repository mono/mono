// 
// AbstractWorkList.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using System.Collections.Generic;

namespace Mono.CodeContracts.Static.DataStructures {
	abstract class AbstractWorkList<T> : IWorkList<T> {
		protected HashSet<T> Elements = new HashSet<T> ();
		protected abstract IEnumerable<T> Collection { get; }

		public int Count
		{
			get { return this.Elements.Count; }
		}

		protected abstract void AddToCollection (T o);

		#region Implementation of IWorkList<T>
		public virtual bool Add (T o)
		{
			if (!this.Elements.Add (o))
				return false;
			AddToCollection (o);
			return true;
		}

		public virtual bool IsEmpty ()
		{
			return this.Elements.Count == 0;
		}

		public abstract T Pull ();

		public virtual bool AddAll (IEnumerable<T> objs)
		{
			bool any = false;
			foreach (T o in objs) {
				if (Add (o))
					any = true;
			}
			return any;
		}

		public virtual IEnumerator<T> GetEnumerator ()
		{
			return Collection.GetEnumerator ();
		}
		#endregion
	}
}
