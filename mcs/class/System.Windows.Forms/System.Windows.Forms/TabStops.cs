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
// Copyright (c) 2015 Karl Scowen
//
// Authors:
//  Karl Scowen		<contact@scowencomputers.co.nz>
//
//

using System;
using System.Collections.Generic;

namespace System.Windows.Forms {
	abstract class TabStop : IComparable<TabStop> {
		float _pos = -1;

		internal float Position {
			get {
				return _pos;
			}

			set {
				if (_pos >= 0)
					throw new InvalidOperationException ("Can't change Position once it has been set!");

				_pos = value;
			}
		}

		internal virtual float GetInitialWidth (Line line, int pos)
		{
			return 0;
		}

		internal abstract float CalculateRight (Line line, int pos);

		public override bool Equals (object obj)
		{
			return obj.GetType () == this.GetType () && Math.Abs (((TabStop)obj).Position - Position) < 0.01;
		}

		public override int GetHashCode ()
		{
			return this.GetType ().GetHashCode () ^ Position.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("[{0}: Position {1}]", this.GetType().Name, Position);
		}

		#region IComparable implementation
		public int CompareTo (TabStop other)
		{
			return Position.CompareTo (other.Position);
		}

		#endregion
	}

	class LeftTabStop : TabStop {
		internal LeftTabStop ()
		{
		}
		internal LeftTabStop (float position)
		{
			Position = position;
		}

		internal override float GetInitialWidth (Line line, int pos)
		{
			return Position - line.widths[pos];
		}

		internal override float CalculateRight (Line line, int pos)
		{
			return Position;
		}
	}

	class CentredTabStop : TabStop {
		internal override float CalculateRight (Line line, int pos)
		{
			int endIndex = line.Text.IndexOfAny (new [] {'\t', '\n', '\r'}, pos + 1); // pos is this tab's index, so look after that.
			if (endIndex < 0)
				endIndex = line.text.Length;
			float textWidth = line.widths [endIndex] - line.widths [pos + 1]; // We use the position after this tabstop, hence pos + 1.
			return Math.Max (Position - textWidth / 2f, line.widths [pos]); // We want the width until the start of the text, which is before Position, but we can't go below zero.
		}
	}

	internal class RightTabStop : TabStop {
		internal override float CalculateRight (Line line, int pos)
		{
			int endIndex = line.Text.IndexOfAny (new [] {'\t', '\n', '\r'}, pos + 1); // pos is this tab's index, so look after that.
			return calcWidth (line, pos, endIndex);
		}

		protected float calcWidth (Line line, int pos, int endIndex)
		{
			if (endIndex < 0)
				endIndex = line.text.Length;
			float textWidth = line.widths [endIndex] - line.widths [pos + 1]; // We use the position after this tabstop, hence pos + 1.
			return Math.Max (Position - textWidth, line.widths [pos]);
		}
	}

	internal class DecimalTabStop : RightTabStop {
		internal override float CalculateRight (Line line, int pos)
		{
			// This is simply a right-align tabstop that regards the decimal as the end.
			int endIndex = line.Text.IndexOfAny (new [] {'\t', '\n', '\r', '.'}, pos + 1); // pos is this tab's index, so look after that.
			return calcWidth (line, pos, endIndex);
		}
	}

	internal class TabStopCollection : IList<TabStop> {
		SortedList<TabStop, TabStop> tabs = new SortedList<TabStop, TabStop> ();

		public TabStopCollection Clone ()
		{
			var n = new TabStopCollection ();
			foreach (var tab in tabs.Keys) {
				n.tabs.Add (tab, null);
			}
			return n;
		}

		public int IndexOf (TabStop tab)
		{
			return tabs.IndexOfKey (tab);
		}

		public void Insert (int index, TabStop item)
		{
			throw new NotSupportedException ("Not relevant to sorted data!");
		}

		public void RemoveAt (int index)
		{
			tabs.RemoveAt (index);
		}

		public TabStop this [int index] {
			get {
				return tabs.Keys [index];
			}
			set {
				throw new NotSupportedException ("Not relevant to sorted data!");
			}
		}

		#region ICollection implementation
		public void Add (TabStop tab)
		{
			tabs.Add (tab, null);
		}

		public void Clear ()
		{
			tabs.Clear ();
		}

		public bool Contains (TabStop tab)
		{
			return tabs.ContainsKey (tab);
		}

		public void CopyTo (TabStop[] array, int arrayIndex)
		{
			tabs.Keys.CopyTo (array, arrayIndex);
		}

		public TabStop[] ToArray ()
		{
			var arr = new TabStop [Count];
			CopyTo (arr, 0);
			return arr;
		}

		public int[] ToPosArray ()
		{
			var arr = new int [Count];
			for (int i = 0; i < Count; i++) {
				arr [i] = (int)this [i].Position;
			}
			return arr;
		}

		public bool Remove (TabStop tab)
		{
			return tabs.Remove (tab);
		}

		public int Count {
			get {
				return tabs.Count;
			}
		}

		bool ICollection<TabStop>.IsReadOnly {
			get {
				return false;
			}
		}
		#endregion

		#region IEnumerable implementation
		public IEnumerator<TabStop> GetEnumerator ()
		{
			return tabs.Keys.GetEnumerator ();
		}
		#endregion

		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		#endregion

		public override bool Equals (object obj)
		{
			var other = obj as TabStopCollection;
			if (other == null || other.Count != this.Count)
				return false;

			for (int i = 0; i < Count; i++) {
				if (!tabs.Keys [i].Equals (other.tabs.Keys [i]))
					return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			// I don't like warnings, but I honestly don't care about the hash code.
			return base.GetHashCode ();
		}
	}
}