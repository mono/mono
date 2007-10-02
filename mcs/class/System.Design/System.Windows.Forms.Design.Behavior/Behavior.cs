//
// System.Windows.Forms.Design.Behavior.Behavior
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

using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Design.Behavior
{
	public abstract class Behavior
	{
		[MonoTODO]
		protected Behavior ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected Behavior (bool callParentBehavior, BehaviorService behaviorService)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Cursor Cursor {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual bool DisableAllCommands {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual MenuCommand FindCommand (CommandID commandId)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnDragDrop (Glyph g, DragEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnDragEnter (Glyph g, DragEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnDragLeave (Glyph g, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnDragOver (Glyph g, DragEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnGiveFeedback (Glyph g, GiveFeedbackEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnLoseCapture (Glyph g, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool OnMouseDoubleClick (Glyph g, MouseButtons button, Point mouseLoc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool OnMouseDown (Glyph g, MouseButtons button, Point mouseLoc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool OnMouseEnter (Glyph g)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool OnMouseHover (Glyph g, Point mouseLoc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool OnMouseLeave (Glyph g)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool OnMouseMove (Glyph g, MouseButtons button, Point mouseLoc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool OnMouseUp (Glyph g, MouseButtons button)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnQueryContinueDrag (Glyph g, QueryContinueDragEventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
