//
// System.Windows.Forms.Design.Behavior.SnapLine
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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


namespace System.Windows.Forms.Design.Behavior
{
	public sealed class SnapLine
	{
		[MonoTODO]
		public static bool ShouldSnap (SnapLine line1, SnapLine line2)
		{
			throw new NotImplementedException ();
		}

		SnapLineType type;
		int offset;
		string filter;
		SnapLinePriority priority;

		[MonoTODO]
		public SnapLine (SnapLineType type, int offset)
			: this (type, offset, null)
		{
		}

		[MonoTODO]
		public SnapLine (SnapLineType type, int offset, string filter)
			: this (type, offset, filter, default (SnapLinePriority))
		{
		}

		[MonoTODO]
		public SnapLine (SnapLineType type, int offset, SnapLinePriority priority)
			: this (type, offset, null, priority)
		{
		}

		[MonoTODO]
		public SnapLine (SnapLineType type, int offset, string filter, SnapLinePriority priority)
		{
			this.type =type;
			this.offset = offset;
			this.filter = filter;
			this.priority = priority;
		}

		public string Filter {
			get { return filter; }
		}

		public bool IsHorizontal {
			get {
				switch (SnapLineType) {
				case SnapLineType.Top:
				case SnapLineType.Bottom:
				case SnapLineType.Horizontal:
				case SnapLineType.Baseline:
					return true;
				default:
					return false;
				}
			}
		}

		public bool IsVertical {
			get {
				switch (SnapLineType) {
				case SnapLineType.Left:
				case SnapLineType.Right:
				case SnapLineType.Vertical:
					return true;
				default:
					return false;
				}
			}
		}

		public int Offset {
			get { return offset; }
		}

		public SnapLinePriority Priority {
			get { return priority; }
		}

		public SnapLineType SnapLineType {
			get { return type; }
		}

		[MonoTODO]
		public void AdjustOffset (int adjustment)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}

#endif
