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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


namespace System.Workflow.ComponentModel
{
	public sealed class ActivityCollection : List <Activity>, IList <Activity>, ICollection <Activity>,
		IEnumerable <Activity>, IList, ICollection, IEnumerable
	{
		private CompositeActivity owner;

		public ActivityCollection (Activity owner)
		{
			if (owner == null) {
				throw new ArgumentNullException ("Owner cannot be null");
			}

			this.owner = (CompositeActivity) owner;
		}

		// Properties
      		public Activity this [string key] {
      			get {
      				throw new NotImplementedException ();
      			}
      		}

		public Activity this [int index] {
			get {
				return base[index];
			}
			set {
				base[index] = value;
			}
		}

		// Methods
		public void Add (Activity item)
		{
			base.Add (item);
			item.SetParent (owner);
		}

		public void Clear ()
		{
			base.Clear ();
		}

		public bool Contains (Activity item)
		{
			return base.Contains (item);
		}

		public IEnumerator<Activity> GetEnumerator ()
		{
			return base.GetEnumerator ();
		}

		public int IndexOf (Activity item)
		{
			return base.IndexOf (item);
		}

		public void Insert (int index, Activity item)
		{
			base.Insert (index, item);
			item.SetParent (owner);
		}

		public bool Remove (Activity item)
		{
			return base.Remove (item);
		}

		public void RemoveAt (int index)
		{
			base.RemoveAt (index);
		}

		//TODO: Implement ICollection and IList
	}
}

