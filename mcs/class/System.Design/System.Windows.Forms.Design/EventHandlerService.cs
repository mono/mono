//
// System.Windows.Forms.Design.DockEditor.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System;

namespace System.Windows.Forms.Design
{
	public sealed class EventHandlerService : IEventHandlerService
	{
		public event EventHandler EventHandlerChanged;

		public EventHandlerService (Control focusWnd)
		{
			_focusWnd = focusWnd;
		}

		[MonoTODO]
		public object GetHandler (Type handlerType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PopHandler (object handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PushHandler (object handler)
		{
			throw new NotImplementedException ();
		}

		public Control FocusWindow {
			get {
				return _focusWnd;
			}
		}

		private readonly Control _focusWnd;
	}
}
