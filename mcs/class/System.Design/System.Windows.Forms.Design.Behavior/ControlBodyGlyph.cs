//
// System.Windows.Forms.Design.Behavior.ControlBodyGlyph
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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Design.Behavior
{
	public class ControlBodyGlyph : ComponentGlyph
	{
		Rectangle bounds;

		[MonoTODO]
		public ControlBodyGlyph (Rectangle bounds, Cursor cursor, IComponent relatedComponent, Behavior behavior)
			: base (relatedComponent, behavior)
		{
			this.bounds = bounds;
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ControlBodyGlyph (Rectangle bounds, Cursor cursor, IComponent relatedComponent, ControlDesigner designer)
			: this (bounds, cursor, relatedComponent, designer.BehaviorService.CurrentBehavior)
		{
		}

		[MonoTODO]
		public override Rectangle Bounds {
			get { return bounds; }
		}

		[MonoTODO]
		public override Cursor GetHitTest (Point p)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
