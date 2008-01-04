//
// System.ComponentModel.Design.UndoEngine.cs
//
// Author:
// 	Ivan N. Zlatev  <contact@i-nz.net>
//
// Copyright (C) 2007 Ivan N. Zlatev <contact@i-nz.net>
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
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;

namespace System.ComponentModel.Design
{
	public abstract class UndoEngine : IDisposable
	{
		private bool _undoing;
		private UndoUnit _currentUnit;
		private IServiceProvider _provider;
		private bool _enabled;

		protected UndoEngine (IServiceProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			_provider = provider;
			_currentUnit = null;
			Enable ();
		}

		private void Enable ()
		{
			if (!_enabled) {
				IComponentChangeService changeService = GetRequiredService (typeof (IComponentChangeService)) as IComponentChangeService;
				changeService.ComponentAdding += new ComponentEventHandler (OnComponentAdding);
				changeService.ComponentAdded += new ComponentEventHandler (OnComponentAdded);
				changeService.ComponentRemoving += new ComponentEventHandler (OnComponentRemoving);
				changeService.ComponentRemoved += new ComponentEventHandler (OnComponentRemoved);
				changeService.ComponentChanging += new ComponentChangingEventHandler (OnComponentChanging);
				changeService.ComponentChanged += new ComponentChangedEventHandler (OnComponentChanged);
				changeService.ComponentRename += new ComponentRenameEventHandler (OnComponentRename);

				IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
				host.TransactionClosed += new DesignerTransactionCloseEventHandler (OnTransactionClosed);
				host.TransactionOpened += new EventHandler (OnTransactionOpened);

				_enabled = true;
			}
		}

		private void Disable ()
		{
			if (_enabled) {
				IComponentChangeService changeService = GetRequiredService (typeof (IComponentChangeService)) as IComponentChangeService;
				changeService.ComponentAdding -= new ComponentEventHandler (OnComponentAdding);
				changeService.ComponentAdded -= new ComponentEventHandler (OnComponentAdded);
				changeService.ComponentRemoving -= new ComponentEventHandler (OnComponentRemoving);
				changeService.ComponentRemoved -= new ComponentEventHandler (OnComponentRemoved);
				changeService.ComponentChanging -= new ComponentChangingEventHandler (OnComponentChanging);
				changeService.ComponentChanged -= new ComponentChangedEventHandler (OnComponentChanged);
				changeService.ComponentRename -= new ComponentRenameEventHandler (OnComponentRename);

				IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
				host.TransactionClosed -= new DesignerTransactionCloseEventHandler (OnTransactionClosed);
				host.TransactionOpened -= new EventHandler (OnTransactionOpened);

				_enabled = false;
			}
		}

		// FIXME: there could be more transactions opened and closed (but not commited) after the first one!!!
		// This means that there should be multiple units. Only the top level transaction is commited though
		// 
		private void OnTransactionOpened (object sender, EventArgs args)
		{
			if (_currentUnit == null) {
				IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
				_currentUnit = CreateUndoUnit (host.TransactionDescription, true);
			}
		}


		private void OnTransactionClosed (object sender, DesignerTransactionCloseEventArgs args)
		{
			IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
			if (!host.InTransaction) { // the "top-most" transaction was closed (currentUnit one)
				_currentUnit.Close ();
				if (args.TransactionCommitted) {
					AddUndoUnit (_currentUnit);
				} else {
					_currentUnit.Undo ();
					DiscardUndoUnit (_currentUnit);
				}
				_currentUnit = null;
			}
		}

		private void OnComponentAdding (object sender, ComponentEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Add " + args.Component.GetType ().Name, true);
			_currentUnit.ComponentAdding (args);
		}

		private void OnComponentAdded (object sender, ComponentEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Add " + args.Component.Site.Name, true);
			_currentUnit.ComponentAdded (args);

			IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
			if (!host.InTransaction) {
				_currentUnit.Close ();
				AddUndoUnit (_currentUnit);
				_currentUnit = null;
			}
		}

		private void OnComponentRemoving (object sender, ComponentEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Remove " + args.Component.Site.Name, true);
			_currentUnit.ComponentRemoving (args);
		}

		private void OnComponentRemoved (object sender, ComponentEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Remove " + args.Component.GetType ().Name, true);
			_currentUnit.ComponentRemoved (args);

			IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
			if (!host.InTransaction) {
				_currentUnit.Close ();
				AddUndoUnit (_currentUnit);
				_currentUnit = null;
			}
		}

		private void OnComponentChanging (object sender, ComponentChangingEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Modify " + ((IComponent)args.Component).Site.Name + "." + args.Member.Name, true);
			_currentUnit.ComponentChanging (args);
		}

		private void OnComponentChanged (object sender, ComponentChangedEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Modify " + ((IComponent)args.Component).Site.Name + "." + args.Member.Name, true);
			_currentUnit.ComponentChanged (args);

			IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
			if (!host.InTransaction) {
				_currentUnit.Close ();
				AddUndoUnit (_currentUnit);
				_currentUnit = null;
			}
		}

		private void OnComponentRename (object sender, ComponentRenameEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Rename " + ((IComponent)args.Component).Site.Name, true);
			_currentUnit.ComponentRename (args);

			IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
			if (!host.InTransaction) {
				_currentUnit.Close ();
				AddUndoUnit (_currentUnit);
				_currentUnit = null;
			}
		}

		public event EventHandler Undoing;
		public event EventHandler Undone;

		public bool Enabled {
			get { return _enabled; }
			set {
				if (value)
					Enable ();
				else
					Disable ();
			}
		}

		public bool UndoInProgress {
			get { return _undoing; }
		}

		protected virtual UndoEngine.UndoUnit CreateUndoUnit (string name, bool primary)
		{
			return new UndoUnit (this, name);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (_currentUnit != null) {
					_currentUnit.Close ();
					_currentUnit = null;
				}
			}
		}

		protected object GetRequiredService (Type serviceType)
		{
			object service = this.GetService (serviceType);
			if (service == null)
				throw new NotSupportedException ("Service '" + serviceType.Name + "' missing");
			return service;
		}

		protected object GetService (Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException ("serviceType");

			if (_provider != null)
				return _provider.GetService (serviceType);
			return null;
		}

		protected virtual void OnUndoing (EventArgs e)
		{
			Disable ();
			_undoing = true;
			if (Undoing != null)
				Undoing (this, e);
		}

		protected virtual void OnUndone (EventArgs e)
		{
			Enable ();
			_undoing = false;
			if (Undone != null)
				Undone (this, e);
		}


		protected abstract void AddUndoUnit (UndoEngine.UndoUnit unit);

		protected virtual void DiscardUndoUnit (UndoEngine.UndoUnit unit)
		{
		}


		protected class UndoUnit
		{
			private class Action
			{
				public virtual void Undo (UndoEngine engine)
				{
				}
			}

			private class ComponentRenameAction : Action
			{
				private string _oldName;
				private string _currentName;

				public ComponentRenameAction (string currentName, string oldName)
				{
					_currentName = currentName;
					_oldName = oldName;
				}

				public override void Undo (UndoEngine engine)
				{
					IComponentChangeService changeService = engine.GetRequiredService (typeof (IComponentChangeService)) as IComponentChangeService;
					IDesignerHost host = engine.GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;

					IComponent component = host.Container.Components[_currentName];
					changeService.OnComponentChanging (component, null);
					component.Site.Name = _oldName;
					string tmp = _currentName;
					_currentName = _oldName;
					_oldName = tmp;
					changeService.OnComponentChanged (component, null, null, null);
				}
			} // ComponentRenameAction

			private class ComponentAddRemoveAction : Action
			{
				private string _componentName;
				private SerializationStore _serializedComponent;
				private bool _added;

				public ComponentAddRemoveAction (UndoEngine engine, IComponent component, bool added)
				{
					if (component == null)
						throw new ArgumentNullException ("component");
					ComponentSerializationService serializationService = engine.GetRequiredService (
						typeof (ComponentSerializationService)) as ComponentSerializationService;

					_serializedComponent = serializationService.CreateStore ();
					serializationService.Serialize (_serializedComponent, component);
					_serializedComponent.Close ();

					_added = added;
					_componentName = component.Site.Name;
				}

				public override void Undo (UndoEngine engine)
				{
					IDesignerHost host = engine.GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
					if (_added) {
						host.DestroyComponent (host.Container.Components[_componentName]);
						_added = false;
					} else {
						ComponentSerializationService serializationService = engine.GetRequiredService (
							typeof (ComponentSerializationService)) as ComponentSerializationService;

						serializationService.DeserializeTo (_serializedComponent, host.Container);
						_added = true;
					}
				}
			} // ComponentAddRemoveAction


			private class ComponentChangeAction : Action
			{
				private SerializationStore _currentState;
				private SerializationStore _oldState;

				public ComponentChangeAction ()
				{
				}

				public void SetOriginalState (UndoEngine engine, IComponent component, MemberDescriptor member)
				{
					ComponentSerializationService serializationService = engine.GetRequiredService (
						typeof (ComponentSerializationService)) as ComponentSerializationService;
					_oldState = serializationService.CreateStore ();
					serializationService.SerializeMember (_oldState, component, member);
					_oldState.Close ();
				}


				public void SetModifiedState (UndoEngine engine, IComponent component, MemberDescriptor member)
				{
					ComponentSerializationService serializationService = engine.GetRequiredService (
						typeof (ComponentSerializationService)) as ComponentSerializationService;
					_currentState = serializationService.CreateStore ();
					serializationService.SerializeMember (_currentState, component, member);
					_currentState.Close ();
				}

				public override void Undo (UndoEngine engine)
				{
					ComponentSerializationService serializationService = engine.GetRequiredService (
						typeof (ComponentSerializationService)) as ComponentSerializationService;
					IDesignerHost host = engine.GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
					serializationService.Deserialize (_oldState, host.Container);

					SerializationStore tmp = _currentState;
					_currentState = _oldState;
					_oldState = tmp;
				}
			} // ComponentChangeAction

			private UndoEngine _engine;
			private string _name;
			private bool _closed;
			private Stack <Action> _actions;

			public UndoUnit (UndoEngine engine, string name)
			{
				if (engine == null)
					throw new ArgumentNullException ("engine");
				if (name == null)
					throw new ArgumentNullException ("name");
					//name = String.Empty;

				_engine = engine;
				_name = name;
				_actions = new Stack <Action> ();
			}

			public void Undo ()
			{
				_engine.OnUndoing (EventArgs.Empty);
				UndoCore ();
				_engine.OnUndone (EventArgs.Empty);
			}

			protected virtual void UndoCore ()
			{
				foreach (Action action in _actions)
					action.Undo (_engine);
			}

			protected UndoEngine UndoEngine {
				get { return _engine; }
			}

			public virtual bool IsEmpty {
				get { return _actions.Count == 0; }
			}

			public virtual string Name {
				get { return _name; }
			}

			public virtual void Close ()
			{
				_closed = true;
			}

			public virtual void ComponentAdded (ComponentEventArgs e)
			{
				if (!_closed)
					_actions.Push (new ComponentAddRemoveAction (_engine, (IComponent) e.Component, true));
			}

			public virtual void ComponentAdding (ComponentEventArgs e)
			{
			}

			public virtual void ComponentChanged (ComponentChangedEventArgs e)
			{
				if (!_closed) {
					ComponentChangeAction action = _actions.Peek () as ComponentChangeAction;
					if (action != null)
						action.SetModifiedState (_engine, (IComponent) e.Component, e.Member);
				}
			}

			public virtual void ComponentChanging (ComponentChangingEventArgs e)
			{
				if (!_closed) {
					ComponentChangeAction action = new ComponentChangeAction ();
					action.SetOriginalState (_engine, (IComponent) e.Component, e.Member);
					_actions.Push (action);
				}
			}

			public virtual void ComponentRemoved (ComponentEventArgs e)
			{
				if (!_closed)
					_actions.Push (new ComponentAddRemoveAction (_engine, e.Component, false));
			}

			public virtual void ComponentRemoving (ComponentEventArgs e)
			{
			}

			public virtual void ComponentRename (ComponentRenameEventArgs e)
			{
				if (!_closed)
					_actions.Push (new ComponentRenameAction (((IComponent)e.Component).Site.Name, e.OldName));
			}

			protected object GetService (Type serviceType)
			{
				return _engine.GetService (serviceType);
			}

			public override string ToString ()
			{
				return _name;
			}
		}
	}
}
#endif
