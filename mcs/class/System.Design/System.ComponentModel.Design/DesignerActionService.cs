//
// System.ComponentModel.Design.DesignerActionService.cs
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
using System.ComponentModel;

namespace System.ComponentModel.Design
{
	public class DesignerActionService : IDisposable
	{
		[MonoTODO]
		public DesignerActionService (IServiceProvider serviceProvider)
		{
			throw new NotImplementedException ();
		}

		public event DesignerActionListsChangedEventHandler DesignerActionListsChanged;

		[MonoTODO]
		public void Add (IComponent comp, DesignerActionList actionList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (IComponent comp, DesignerActionListCollection designerActionListCollection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (IComponent comp)
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
		public DesignerActionListCollection GetComponentActions (IComponent component)
		{
			return GetComponentActions (component, ComponentActionsType.All); // not verified
		}

		[MonoTODO]
		public virtual DesignerActionListCollection GetComponentActions (IComponent component, ComponentActionsType type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void GetComponentDesignerActions (IComponent component, DesignerActionListCollection actionLists)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void GetComponentServiceActions (IComponent component, DesignerActionListCollection actionLists)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (DesignerActionList actionList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (IComponent comp)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (IComponent comp, DesignerActionList actionList)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
