//
// System.ComponentModel.Design.UndoEngine.cs
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
	public abstract class UndoEngine : IDisposable
	{
		[MonoTODO]
		protected UndoEngine (IServiceProvider provider)
		{
			throw new NotImplementedException ();
		}

		public event EventHandler Undoing;
		public event EventHandler Undone;

		[MonoTODO]
		public bool Enabled {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool UndoInProgress {
			get { throw new NotImplementedException (); }
		}

		protected abstract void AddUndoUnit (UndoEngine.UndoUnit unit);

		[MonoTODO]
		protected virtual UndoEngine.UndoUnit CreateUndoUnit (string name, bool primary)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void DiscardUndoUnit (UndoEngine.UndoUnit unit)
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
		protected object GetRequiredService (Type serviceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnUndoing (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnUndone (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected class UndoUnit
		{
			UndoEngine engine;
			string name;

			[MonoTODO]
			public UndoUnit (UndoEngine engine, string name)
			{
				this.engine = engine;
				this.name = name;
			}

			protected UndoEngine UndoEngine {
				get { return engine; }
			}

			[MonoTODO]
			public virtual bool IsEmpty {
				get { throw new NotImplementedException (); }
			}

			public virtual string Name {
				get { return name; }
			}

			[MonoTODO]
			public virtual void Close ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void ComponentAdded (ComponentEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void ComponentAdding (ComponentEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void ComponentChanged (ComponentEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void ComponentChanging (ComponentEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void ComponentRemoved (ComponentEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void ComponentRemoving (ComponentEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void ComponentRename (ComponentRenameEventArgs e)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			protected object GetService (Type serviceType)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override string ToString ()
			{
				return base.ToString ();
			}

			[MonoTODO]
			public void Undo ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			protected virtual void UndoCore ()
			{
				throw new NotImplementedException ();
			}
		}
	}
}
#endif
