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
			// Console.WriteLine ("TransactionClosed: Commited: " + args.TransactionCommitted.ToString ());
			IDesignerHost host = GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
			if (!host.InTransaction) { // the "top-most"/last transaction was closed (currentUnit one)
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
				_currentUnit = CreateUndoUnit ("Modify " + ((IComponent)args.Component).Site.Name + 
							       (args.Member != null ? "." + args.Member.Name : ""), 
							       true);
			_currentUnit.ComponentChanging (args);
		}

		private void OnComponentChanged (object sender, ComponentChangedEventArgs args)
		{
			if (_currentUnit == null)
				_currentUnit = CreateUndoUnit ("Modify " + ((IComponent)args.Component).Site.Name + "." + 
							       (args.Member != null ? "." + args.Member.Name : ""), 
							       true);
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
			// Console.WriteLine ("CreateUndoUnit: " + name);
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
			// Console.WriteLine ("DiscardUndoUnit: " + unit.Name);
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
					// Console.WriteLine ("ComponentRenameAction (" + oldName + "): " + currentName);
					_currentName = currentName;
					_oldName = oldName;
				}

				public override void Undo (UndoEngine engine)
				{
					// Console.WriteLine ("ComponentRenameAction.Undo (" + _currentName + "): " + _oldName);
					IDesignerHost host = engine.GetRequiredService (typeof (IDesignerHost)) as IDesignerHost;
					IComponent component = host.Container.Components[_currentName];
					component.Site.Name = _oldName;
					string tmp = _currentName;
					_currentName = _oldName;
					_oldName = tmp;
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
					// Console.WriteLine ((added ? "Component*Add*RemoveAction" : "ComponentAdd*Remove*Action") +
					// 		   " (" + component.Site.Name + ")");
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
						// Console.WriteLine ("Component*Add*RemoveAction.Undo (" + _componentName + ")");
						IComponent component = host.Container.Components[_componentName];
						if (component != null) // the component might have been destroyed already
							host.DestroyComponent (component);
						_added = false;
					} else {
						// Console.WriteLine ("ComponentAdd*Remove*Action.Undo (" + _componentName + ")");
						ComponentSerializationService serializationService = engine.GetRequiredService (
							typeof (ComponentSerializationService)) as ComponentSerializationService;

						serializationService.DeserializeTo (_serializedComponent, host.Container);
						_added = true;
					}
				}
			} // ComponentAddRemoveAction


			private class ComponentChangeAction : Action
			{
				private string _componentName;
				private MemberDescriptor _member;
				private IComponent _component;
				private SerializationStore _afterChange;
				private SerializationStore _beforeChange;

				public ComponentChangeAction ()
				{
				}

				public void SetOriginalState (UndoEngine engine, IComponent component, MemberDescriptor member)
				{
					_member = member;
					_component = component;
					_componentName = component.Site != null ? component.Site.Name : null;
					// Console.WriteLine ("ComponentChangeAction.SetOriginalState (" + (_componentName != null ? (_componentName + ".") : "") +
					// 		   member.Name + "): " +
					// 		   (((PropertyDescriptor)member).GetValue (component) == null ? "null" :
					// 		   ((PropertyDescriptor)member).GetValue (component).ToString ()));
					ComponentSerializationService serializationService = engine.GetRequiredService (
						typeof (ComponentSerializationService)) as ComponentSerializationService;
					_beforeChange = serializationService.CreateStore ();
					serializationService.SerializeMemberAbsolute (_beforeChange, component, member);
					_beforeChange.Close ();
				}


				public void SetModifiedState (UndoEngine engine, IComponent component, MemberDescriptor member)
				{
					// Console.WriteLine ("ComponentChangeAction.SetModifiedState (" + (_componentName != null ? (_componentName + ".") : "") +
					// 		   member.Name + "): " +
					// 		   (((PropertyDescriptor)member).GetValue (component) == null ? "null" :
					// 		   ((PropertyDescriptor)member).GetValue (component).ToString ()));
					ComponentSerializationService serializationService = engine.GetRequiredService (
						typeof (ComponentSerializationService)) as ComponentSerializationService;
					_afterChange = serializationService.CreateStore ();
					serializationService.SerializeMemberAbsolute (_afterChange, component, member);
					_afterChange.Close ();
				}

				public bool IsComplete {
					get { return (_beforeChange != null && _afterChange != null); }
				}

				public string ComponentName {
					get { return _componentName; }
				}

				public IComponent Component {
					get { return _component; }
				}

				public MemberDescriptor Member {
					get { return _member; }
				}

				// Reminder: _component might no longer be a valid instance
				// so one should request a new one.
				// 
				public override void Undo (UndoEngine engine)
				{
					if (_beforeChange == null) {
						// Console.WriteLine ("ComponentChangeAction.Undo: ERROR: UndoUnit is not complete.");
						return;
					}

					// Console.WriteLine ("ComponentChangeAction.Undo (" + _componentName + "." + _member.Name + ")");
					IDesignerHost host = (IDesignerHost)engine.GetRequiredService (typeof(IDesignerHost));
					_component = host.Container.Components[_componentName];

					ComponentSerializationService serializationService = engine.GetRequiredService (
						typeof (ComponentSerializationService)) as ComponentSerializationService;
					serializationService.DeserializeTo (_beforeChange, host.Container);

					SerializationStore tmp = _beforeChange;
					_beforeChange = _afterChange;
					_afterChange = tmp;
				}
			} // ComponentChangeAction

			private UndoEngine _engine;
			private string _name;
			private bool _closed;
			private List<Action> _actions;

			public UndoUnit (UndoEngine engine, string name)
			{
				if (engine == null)
					throw new ArgumentNullException ("engine");
				if (name == null)
					throw new ArgumentNullException ("name");

				_engine = engine;
				_name = name;
				_actions = new List <Action> ();
			}

			public void Undo ()
			{
				_engine.OnUndoing (EventArgs.Empty);
				UndoCore ();
				_engine.OnUndone (EventArgs.Empty);
			}

			protected virtual void UndoCore ()
			{
				for (int i = _actions.Count - 1; i >= 0; i--) {
					// Console.WriteLine ("Undoing action type: " + _actions[i].GetType ().Name);
					_actions[i].Undo (_engine);
				}
				// Also reverses the stack of actions, so that
				// if Undo is called twice it will Redo in the proper order
				// 
				_actions.Reverse ();
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
				// Console.WriteLine ("UndoUnot.Close (" + _name + ")");
				_closed = true;
			}

			public virtual void ComponentAdded (ComponentEventArgs e)
			{
				if (!_closed) {
					// Console.WriteLine ("New Action: Component*Add*RemoveAction (" + ((IComponent)e.Component).Site.Name + ")");
					_actions.Add (new ComponentAddRemoveAction (_engine, (IComponent) e.Component, true));
				}
			}

			public virtual void ComponentAdding (ComponentEventArgs e)
			{
			}

			public virtual void ComponentChanged (ComponentChangedEventArgs e)
			{
				if (_closed)
					return;

				// A component starts getting removed. 
				// ComponentRemoving -> remove component -> ComponentRemoved
				// The problem is that someone can subscribe to the Removed event after us (the UndoEngine) - e.g
				// ParentControlDesigner will explicitly request (by setting it to null between Removing and Removed 
				// the serialization of the Parent property of the removed child.
				// In the case where someone subscribes after and performs changes to the component, we might get 
				// ComponentChanged events after we've already created the addremove action, but the componentchangeaction
				// will be incomplete standing before the addremove one.
				//
				ComponentChangeAction changeAction = null;
				for (int i=0; i < _actions.Count; i++) {
					changeAction = _actions[i] as ComponentChangeAction;
					if (changeAction != null && !changeAction.IsComplete &&
					    changeAction.Component == e.Component &&
					    changeAction.Member.Equals (e.Member)) {
						changeAction.SetModifiedState (_engine, (IComponent) e.Component, e.Member);
						break;
					}
				}
			}

			public virtual void ComponentChanging (ComponentChangingEventArgs e)
			{
				if (_closed)
					return;

				// Console.WriteLine ("New Action: ComponentChangeAction (" + ((IComponent)e.Component).Site.Name + ")");
				ComponentChangeAction action = new ComponentChangeAction ();
				action.SetOriginalState (_engine, (IComponent) e.Component, e.Member);
				_actions.Add (action);
			}

			public virtual void ComponentRemoved (ComponentEventArgs e)
			{
			}

			public virtual void ComponentRemoving (ComponentEventArgs e)
			{
				if (!_closed) {
					// Console.WriteLine ("New Action: ComponentAdd*Remove*Action (" + ((IComponent)e.Component).Site.Name + ")");
					_actions.Add (new ComponentAddRemoveAction (_engine, e.Component, false));
				}
			}

			public virtual void ComponentRename (ComponentRenameEventArgs e)
			{
				if (!_closed) {
					// Console.WriteLine ("New Action: ComponentRenameAction (" + ((IComponent)e.Component).Site.Name + ")");
					_actions.Add (new ComponentRenameAction (e.NewName, e.OldName));
				}
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
