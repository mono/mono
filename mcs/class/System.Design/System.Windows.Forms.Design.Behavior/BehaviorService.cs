//
// System.Windows.Forms.Design.Behavior.BehaviorService
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

using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Design.Behavior
{
	public sealed class BehaviorService : IDisposable
	{
		internal BehaviorService ()
		{
		}

		public event BehaviorDragDropEventHandler BeginDrag;
		public event BehaviorDragDropEventHandler EndDrag;
		public event EventHandler Synchronize;

		[MonoTODO]
		public BehaviorServiceAdornerCollection Adorners {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Graphics AdornerWindowGraphics {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Behavior CurrentBehavior {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Point AdornerWindowPointToScreen (Point p)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Point AdornerWindowToScreen ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Rectangle ControlRectInAdornerWindow (Control c)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Point ControlToAdornerWindow (Control c)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Behavior GetNextBehavior (Behavior behavior)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Invalidate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Invalidate (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Invalidate (Region r)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Point MapAdornerWindowPoint (IntPtr handle, Point pt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Behavior PopBehavior (Behavior behavior)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PushBehavior (Behavior behavior)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PushCaptureBehavior (Behavior behavior)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Point ScreenToAdornerWindow (Point p)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SyncSelection ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
