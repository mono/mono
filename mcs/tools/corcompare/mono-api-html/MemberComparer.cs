// 
// Authors
//    Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013-2014 Xamarin Inc. http://www.xamarin.com
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
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Xamarin.ApiDiff {

	public abstract class MemberComparer : Comparer {

		// true if this is the first element being added or removed in the group being rendered
		protected bool first;

		public abstract string GroupName { get; }
		public abstract string ElementName { get; }

		public void Compare (XElement source, XElement target)
		{
			var s = source.Element (GroupName);
			var t = target.Element (GroupName);
			if (XNode.DeepEquals (s, t))
				return;

			if (s == null) {
				Add (t.Elements (ElementName));
			} else if (t == null) {
				Remove (s.Elements (ElementName));
			} else {
				Compare (s.Elements (ElementName), t.Elements (ElementName));
			}
		}

		public override void SetContext (XElement current)
		{
		}

		public XElement Source { get; set; }

		public virtual bool Find (XElement e)
		{
			return e.GetAttribute ("name") == Source.GetAttribute ("name");
		}

		XElement Find (IEnumerable<XElement> target)
		{
			return State.Lax ? target.FirstOrDefault (Find) : target.SingleOrDefault (Find);
		}

		public override void Compare (IEnumerable<XElement> source, IEnumerable<XElement> target)
		{
			removed.Clear ();
			obsoleted.Clear ();

			foreach (var s in source) {
				SetContext (s);
				Source = s;
				var t = Find (target);
				if (t == null) {
					// not in target, it was removed
					removed.Add (s);
				} else {
					// possibly modified
					if (Equals (s, t)) {
						if (IsNowObsoleted (s, t)) {
							obsoleted.Add (t);
						}
						t.Remove ();
						continue;
					}

					// still in target so will be part of Added
					removed.Add (s);
					Modified (s, t);
				}
			}
			// delayed, that way we show "Modified", "Added" and then "Removed"
			Remove (removed);

			// remaining == newly added in target
			Add (target);

			// obsolete (considering as added, they were not obsolete before, wrt regex)
			bool o = false;
			foreach (var item in obsoleted) {
				SetContext (item);
				if (State.IgnoreAdded.Any (re => re.IsMatch (GetDescription (item))))
					continue;
				if (!o) {
					BeforeObsoleting (obsoleted);
					o = true;
				}
				Obsoleted (item);
			}
			if (o)
				AfterObsoleting ();
		}

		void Add (IEnumerable<XElement> elements)
		{
			bool a = false;
			foreach (var item in elements) {
				SetContext (item);
				if (State.IgnoreAdded.Any (re => re.IsMatch (GetDescription (item))))
					continue;
				if (!a) {
					BeforeAdding (elements);
					a = true;
				}
				Added (item);
			}
			if (a)
				AfterAdding ();
		}

		void Remove (IEnumerable<XElement> elements)
		{
			bool r = false;
			foreach (var item in elements) {
				if (State.IgnoreRemoved.Any (re => re.IsMatch (GetDescription (item))))
					continue;
				SetContext (item);
				if (!r) {
					BeforeRemoving (elements);
					r = true;
				}
				Removed (item);
			}
			if (r)
				AfterRemoving ();
		}

		public abstract string GetDescription (XElement e);

		protected StringBuilder GetObsoleteMessage (XElement e)
		{
			var sb = new StringBuilder ();
			string o = e.GetObsoleteMessage ();
			if (o != null) {
				sb.Append ("[Obsolete");
				if (o.Length > 0)
					sb.Append (" (\"").Append (o).Append ("\")");
				sb.AppendLine ("]");
				for (int i = 0; i < State.Indent + 1; i++)
					sb.Append ('\t');
			}
			return sb;
		}

		public override bool Equals (XElement source, XElement target)
		{
			if (base.Equals (source, target))
				return true;

			return GetDescription (source) == GetDescription (target);
		}

		bool IsNowObsoleted (XElement source, XElement target)
		{
			var s = GetObsoleteMessage (source).ToString ();
			var t = GetObsoleteMessage (target).ToString ();
			// true if it was no [Obsolete] in the source but now is [Obsolete] in the target
			return (s.Length == 0 && t.Length > 0);
		}

		public virtual void BeforeAdding (IEnumerable<XElement> list)
		{
			first = true;
			Output.WriteLine ("<p>Added {0}:</p><pre>", list.Count () > 1 ? GroupName : ElementName);
		}

		public override void Added (XElement target)
		{
			var o = GetObsoleteMessage (target);
			if (!first && (o.Length > 0))
				Output.WriteLine ();
			Indent ().WriteLine ("\t{0}{1}", o, GetDescription (target));
			first = false;
		}

		public virtual void AfterAdding ()
		{
			Output.WriteLine ("</pre>");
		}

		public virtual void BeforeObsoleting (IEnumerable<XElement> list)
		{
			Output.WriteLine ("<p>Obsoleted {0}:</p><pre>", list.Count () > 1 ? GroupName : ElementName);
		}

		public void Obsoleted (XElement target)
		{
			Indent ().WriteLine ("\t{0}{1}{2}", GetObsoleteMessage (target), GetDescription (target), Environment.NewLine);
		}

		public virtual void AfterObsoleting ()
		{
			Output.WriteLine ("</pre>");
		}

		public override void Modified (XElement source, XElement target)
		{
		}

		public virtual void BeforeRemoving (IEnumerable<XElement> list)
		{
			first = true;
			Output.WriteLine ("<p>Removed {0}:</p><pre>", list.Count () > 1 ? GroupName : ElementName);
		}

		public override void Removed (XElement source)
		{
			var o = GetObsoleteMessage (source);
			if (!first && (o.Length > 0))
				Output.WriteLine ();
			Indent ().WriteLine ("\t{0}{1}", o, GetDescription (source));
			first = false;
		}

		public virtual void AfterRemoving ()
		{
			Output.WriteLine ("</pre>");
		}
	}
}
