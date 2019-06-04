// 
// Authors
//    Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013 Xamarin Inc. http://www.xamarin.com
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
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Mono.ApiTools {

	abstract class Comparer {

		protected List<XElement> removed;
		protected ApiChanges modified;

		public Comparer (State state)
		{
			State = state;
			removed = new List<XElement> ();
			modified = new ApiChanges (state);
		}

		public State State { get; }

		public TextWriter Output {
			get { return State.Output; }
		}

		public Formatter Formatter {
			get { return State.Formatter; }
		}

		protected TextWriter Indent ()
		{
			for (int i = 0; i < State.Indent; i++)
				State.Output.Write ("\t");
			return State.Output;
		}

		public abstract void Added (XElement target, bool wasParentAdded);
		public abstract void Modified (XElement source, XElement target, ApiChanges changes);
		public abstract void Removed (XElement source);

		public virtual bool Equals (XElement source, XElement target, ApiChanges changes)
		{
			return XNode.DeepEquals (source, target);
		}

		public abstract void SetContext (XElement current);

		public virtual void Compare (IEnumerable<XElement> source, IEnumerable<XElement> target)
		{
			removed.Clear ();
			modified.Clear ();

			foreach (var s in source) {
				SetContext (s);
				string sn = s.GetAttribute ("name");
				var t = target == null ? null : target.SingleOrDefault (x => x.GetAttribute ("name") == sn);
				if (t == null) {
					// not in target, it was removed
					removed.Add (s);
				} else {
					t.Remove ();
					// possibly modified
					if (Equals (s, t, modified))
						continue;

					// still in target so will be part of Added
					Modified (s, t, modified);
				}
			}
			// delayed, that way we show "Modified", "Added" and then "Removed"
			foreach (var item in removed) {
				SetContext (item);
				Removed (item);
			}
			// remaining == newly added in target
			if (target != null) {
				foreach (var item in target) {
					SetContext (item);
					Added (item, false);
				}
			}
		}
	}
}