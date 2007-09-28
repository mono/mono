//
// System.ComponentModel.Design.MenuCommandService.cs
//
// Author:
//      Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc
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
using System;
using System.Collections;

namespace System.ComponentModel.Design
{
	public class MenuCommandService : IMenuCommandService, IDisposable
	{
		[MonoTODO]
		public MenuCommandService (IServiceProvider serviceProvider)
		{
			throw new NotImplementedException ();
		}

		public event MenuCommandsChangedEventHandler MenuCommandsChanged;

		[MonoTODO]
		public virtual DesignerVerbCollection Verbs {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual void AddCommand (MenuCommand command)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddVerb (DesignerVerb verb)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
		}

		[MonoTODO]
		protected void EnsureVerbs ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MenuCommand FindCommand (CommandID commandID)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected MenuCommand FindCommand (Guid guid, int id)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ICollection GetCommandList (Guid guid)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool GlobalInvoke (CommandID commandID)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool GlobalInvoke (CommandID commandID, object arg)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnCommandsChanged (MenuCommandsChangedEventArgs e)
		{
			if (MenuCommandsChanged != null)
				MenuCommandsChanged (this, e);
		}

		[MonoTODO]
		public virtual void RemoveCommand (MenuCommand command)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveVerb (DesignerVerb verb)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ShowContextMenu (CommandID menuID, int x, int y)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
