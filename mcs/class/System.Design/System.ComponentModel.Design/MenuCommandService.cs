//
// System.ComponentModel.Design.MenuCommandService.cs
//
// Author:
//      Atsushi Enomoto  <atsushi@ximian.com>
//	Ivan N. Zlatev   <contact@i-nz.net>
//
// Copyright (C) 2007 Novell, Inc
// Copyright (C) Ivan N. Zlatev
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


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;

namespace System.ComponentModel.Design
{
	public class MenuCommandService : IMenuCommandService, IDisposable
	{
		private IServiceProvider _serviceProvider;
		private DesignerVerbCollection _globalVerbs;
		private DesignerVerbCollection _verbs;
		private Dictionary <CommandID, MenuCommand> _commands;

		public MenuCommandService (IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException ("serviceProvider");
			_serviceProvider = serviceProvider;
			ISelectionService selectionSvc = _serviceProvider.GetService (typeof (ISelectionService)) as ISelectionService;
			if (selectionSvc != null)
				selectionSvc.SelectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged (object sender, EventArgs arg)
		{
			this.OnCommandsChanged (new MenuCommandsChangedEventArgs (MenuCommandsChangedType.CommandChanged, null));
			// The commands will be updated whenever they are requested/modified.
		}

		public event MenuCommandsChangedEventHandler MenuCommandsChanged;

		public virtual DesignerVerbCollection Verbs {
			get {
				this.EnsureVerbs ();
				return _verbs;
			}
		}

		public virtual void AddCommand (MenuCommand command)
		{
			if (command == null)
				throw new ArgumentNullException ("command");

			if (_commands == null)
				_commands = new Dictionary <CommandID, MenuCommand> ();
			_commands.Add (command.CommandID, command);
			this.OnCommandsChanged (new MenuCommandsChangedEventArgs (MenuCommandsChangedType.CommandAdded, command));
		}

		public virtual void AddVerb (DesignerVerb verb)
		{
			if (verb == null)
				throw new ArgumentNullException ("verb");
			this.EnsureVerbs ();
			if (!_verbs.Contains (verb)) {
				if (_globalVerbs == null)
					_globalVerbs = new DesignerVerbCollection ();
				_globalVerbs.Add (verb);
			}
			this.OnCommandsChanged (new MenuCommandsChangedEventArgs (MenuCommandsChangedType.CommandAdded, verb));
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (_globalVerbs != null) {
					_globalVerbs.Clear ();
					_globalVerbs = null;
				}
				if (_verbs != null) {
					_verbs.Clear ();
					_verbs = null;
				}
				if (_commands != null) {
					_commands.Clear ();
					_commands = null;
				}
				if (_serviceProvider != null) {
					ISelectionService selectionSvc = _serviceProvider.GetService (typeof (ISelectionService)) as ISelectionService;
					if (selectionSvc != null)
						selectionSvc.SelectionChanged -= OnSelectionChanged;
					_serviceProvider = null;
				}
			}
		}

		protected void EnsureVerbs ()
		{
			DesignerVerbCollection selectionVerbs = null;

			ISelectionService selectionSvc = this.GetService (typeof (ISelectionService)) as ISelectionService;
			IDesignerHost host = this.GetService (typeof (IDesignerHost)) as IDesignerHost;
			if (selectionSvc != null && host != null && selectionSvc.SelectionCount == 1) {
				IComponent primarySelection = selectionSvc.PrimarySelection as IComponent;
				if (primarySelection != null) {
					IDesigner designer = host.GetDesigner (primarySelection);
					if (designer != null)
						selectionVerbs = designer.Verbs;
				}
			}

			// Designer provided verbs have the higher precedence than the global
			//
			Dictionary <string, DesignerVerb> allVerbs = new Dictionary <string, DesignerVerb> ();
			if (_globalVerbs != null) {
				foreach (DesignerVerb verb in _globalVerbs)
					allVerbs[verb.Text] = verb;
			}
			if (selectionVerbs != null) {
				foreach (DesignerVerb verb in selectionVerbs)
					allVerbs[verb.Text] = verb;
			}

			if (_verbs == null)
				_verbs = new DesignerVerbCollection ();
			else
				_verbs.Clear ();

			foreach (DesignerVerb verb in allVerbs.Values)
				_verbs.Add (verb);
		}

		protected MenuCommand FindCommand (Guid guid, int id)
		{
			return this.FindCommand (new CommandID (guid, id));
		}

		public MenuCommand FindCommand (CommandID commandID)
		{
			if (commandID == null)
				throw new ArgumentNullException ("commandID");

			MenuCommand command = null;
			if (_commands != null)
				_commands.TryGetValue (commandID, out command);
			if (command == null) {
				this.EnsureVerbs ();
				foreach (DesignerVerb verb in _verbs) {
					if (verb.CommandID.Equals (commandID)) {
						command = (MenuCommand) verb;
						break;
					}
				}
			}
			return command;
		}

		protected ICollection GetCommandList (Guid guid)
		{
			List<MenuCommand> list = new List<MenuCommand> ();
			if (_commands != null) {
				foreach (MenuCommand command in _commands.Values) {
					if (command.CommandID.Guid == guid)
						list.Add (command);
				}
			}
			return list;
		}

		public virtual bool GlobalInvoke (CommandID commandID)
		{
			if (commandID == null)
				throw new ArgumentNullException ("commandID");

			MenuCommand command = this.FindCommand (commandID);
			if (command != null) {
				command.Invoke ();
				return true;
			}
			return false;
		}

		public virtual bool GlobalInvoke (CommandID commandId, object arg)
		{
			if (commandId == null)
				throw new ArgumentNullException ("commandId");

			MenuCommand command = this.FindCommand (commandId);
			if (command != null) {
				command.Invoke (arg);
				return true;
			}
			return false;
		}

		protected virtual void OnCommandsChanged (MenuCommandsChangedEventArgs e)
		{
			if (MenuCommandsChanged != null)
				MenuCommandsChanged (this, e);
		}

		public virtual void RemoveCommand (MenuCommand command)
		{
			if (command == null)
				throw new ArgumentNullException ("command");
			if (_commands != null)
				_commands.Remove (command.CommandID);

			this.OnCommandsChanged (new MenuCommandsChangedEventArgs (MenuCommandsChangedType.CommandRemoved, null));
		}

		public virtual void RemoveVerb (DesignerVerb verb)
		{
			if (verb == null)
				throw new ArgumentNullException ("verb");

			if (_globalVerbs.Contains (verb))
				_globalVerbs.Remove (verb);

			this.OnCommandsChanged (new MenuCommandsChangedEventArgs (MenuCommandsChangedType.CommandRemoved, verb));
		}

		public virtual void ShowContextMenu (CommandID menuID, int x, int y)
		{
		}

		protected object GetService (Type serviceType)
		{
			if (_serviceProvider != null)
				return _serviceProvider.GetService (serviceType);
			return null;
		}
	}
}
