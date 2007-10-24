//
// System.ComponentModel.Design.Serialization.BasicDesignerLoader
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace System.ComponentModel.Design.Serialization
{
	public abstract class BasicDesignerLoader : DesignerLoader, IDesignerLoaderService
	{

		[Flags]
		protected enum ReloadOptions 
		{
			Default,
			Force,
			ModifyOnError,
			NoFlush
		}

		private bool _loaded;
		private bool _loading;
		private IDesignerLoaderHost _host;
		private int _dependenciesCount;
		private bool _notificationsEnabled;
		private bool _modified;
		private string _baseComponentClassName;
		private DesignerSerializationManager _serializationMananger;
		private bool _flushing;
		private bool _reloadScheduled;
		private ReloadOptions _reloadOptions;

		protected BasicDesignerLoader ()
		{
			_loading = _loaded = _flushing = _reloadScheduled = false;
			_host = null;
			_notificationsEnabled = false;
			_modified = false;
			_dependenciesCount = 0;
		}
		
		protected virtual void Initialize ()
		{
			_serializationMananger = new DesignerSerializationManager (_host);

			DesignSurfaceServiceContainer serviceContainer = _host.GetService (typeof (IServiceContainer)) as DesignSurfaceServiceContainer;
			if (serviceContainer != null) {
				serviceContainer.AddService (typeof (IDesignerLoaderService), (IDesignerLoaderService) this);
				serviceContainer.AddNonReplaceableService (typeof (IDesignerSerializationManager), _serializationMananger);
			}
		}
		
		public override void BeginLoad (IDesignerLoaderHost host)
		{
			if (host == null)
				throw new ArgumentNullException ("host");
			if (_loaded)
				throw new InvalidOperationException ("Already loaded.");
			if (_host != null && _host != host)
				throw new InvalidOperationException ("Trying to load with a different host");

			if (_host == null) { // beingload is called on reload - no need to initialize twice.
				_host = host;
				Initialize ();
			}
			IDisposable session = _serializationMananger.CreateSession ();

			IDesignerLoaderService loader = _host.GetService (typeof (IDesignerLoaderService)) as IDesignerLoaderService;

			if (loader != null) {
				_dependenciesCount = -1;
				loader.AddLoadDependency ();
			} else {
				OnBeginLoad ();
			}

			bool successful = true;

			try {
				PerformLoad (_serializationMananger);
			} catch (Exception e) {
				successful = false;
				_serializationMananger.Errors.Add (e);
			}

			if (loader != null)
				loader.DependentLoadComplete (successful, _serializationMananger.Errors);
			else
				OnEndLoad (successful, _serializationMananger.Errors);

			session.Dispose ();
		}
	
		protected abstract void PerformLoad (IDesignerSerializationManager serializationManager);
		
		protected virtual void OnBeginLoad ()
		{
			_loading = true;
		}
		
		protected virtual void OnEndLoad (bool successful, ICollection errors)
		{
			_host.EndLoad (_baseComponentClassName, successful,  errors);

			if (successful) {
				_loaded = true;
				EnableComponentNotification (true);
			} else {
				if (_reloadScheduled) { // we are reloading
					bool modify = ((_reloadOptions & ReloadOptions.ModifyOnError) == ReloadOptions.ModifyOnError);
					if (modify) {
						OnModifying ();
						this.Modified = true;
					}
				}
			}
			_loading = false;
		}
		
		public override bool Loading {
			get { return _loading; }
		}

		protected IDesignerLoaderHost LoaderHost { 
			get { return _host; }
		}

		protected virtual bool Modified { 
			get { return _modified; }
			set { _modified = value; }
		}

		protected object PropertyProvider { 
			get {
				if (!_loaded)
					throw new InvalidOperationException ("host not initialized");
				return _serializationMananger.PropertyProvider;
			} 
			set {
				if (!_loaded)
					throw new InvalidOperationException ("host not initialized");
				_serializationMananger.PropertyProvider = value;
			}
		}

		protected bool ReloadPending { 
			get { return _reloadScheduled; }
		}
		
		protected virtual bool EnableComponentNotification (bool enable)
		{
			if (!_loaded)
				throw new InvalidOperationException ("host not initialized");

			IComponentChangeService service = _host.GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (service != null && _notificationsEnabled != enable) {
				if (enable) {
					service.ComponentAdding += new ComponentEventHandler (OnComponentAdding);
					service.ComponentAdded += new ComponentEventHandler (OnComponentAdded);
					service.ComponentRemoving += new ComponentEventHandler (OnComponentRemoving);
					service.ComponentRemoved += new ComponentEventHandler (OnComponentRemoved);
					service.ComponentChanging += new ComponentChangingEventHandler (OnComponentChanging);
					service.ComponentChanged += new ComponentChangedEventHandler (OnComponentChanged);
					service.ComponentRename += new ComponentRenameEventHandler (OnComponentRename);
				} else {
					service.ComponentAdding -= new ComponentEventHandler (OnComponentAdding);
					service.ComponentAdded -= new ComponentEventHandler (OnComponentAdded);
					service.ComponentRemoving -= new ComponentEventHandler (OnComponentRemoving);
					service.ComponentRemoved -= new ComponentEventHandler (OnComponentRemoved);
					service.ComponentChanging -= new ComponentChangingEventHandler (OnComponentChanging);
					service.ComponentChanged -= new ComponentChangedEventHandler (OnComponentChanged);
					service.ComponentRename -= new ComponentRenameEventHandler (OnComponentRename);
				}
			}
			return _notificationsEnabled == true ? true : false;
		}

		private void OnComponentAdded (object sender, ComponentEventArgs args)
		{
			if (!_loading && _loaded)
				this.Modified = true;
		}

		private void OnComponentRemoved (object sender, ComponentEventArgs args)
		{
			if (!_loading && _loaded)
				this.Modified = true;
		}

		private void OnComponentAdding (object sender, ComponentEventArgs args)
		{
			if (!_loading && _loaded)
				OnModifying ();
		}

		private void OnComponentRemoving (object sender, ComponentEventArgs args)
		{
			if (!_loading && _loaded)
				OnModifying ();
		}

		private void OnComponentChanged (object sender, ComponentChangedEventArgs args)
		{
			if (!_loading && _loaded)
				this.Modified = true;
		}

		private void OnComponentChanging (object sender, ComponentChangingEventArgs args)
		{
			if (!_loading && _loaded)
				OnModifying ();
		}

		private void OnComponentRename (object sender, ComponentRenameEventArgs args)
		{
			if (!_loading && _loaded) {
					OnModifying ();
					this.Modified = true;
			}
		}

		public override void Flush ()
		{
			if (!_loaded)
				throw new InvalidOperationException ("host not initialized");

			if (!_flushing && this.Modified) {
				_flushing = true;
				using ((IDisposable)_serializationMananger.CreateSession ()) {
					try {
						PerformFlush (_serializationMananger);
					} catch (Exception e) {
						_serializationMananger.Errors.Add (e);
						ReportFlushErrors (_serializationMananger.Errors);
					}
				}
				_flushing = false;
			}
		}
		
		protected abstract void PerformFlush (IDesignerSerializationManager serializationManager);
		
		// MSDN: The default implementation always returns true.
		protected virtual bool IsReloadNeeded ()
		{
			return true;
		}
		

		protected virtual void OnBeginUnload ()
		{
		}
		
		protected virtual void OnModifying ()
		{
		}
		
		// MSDN: reloads are performed at idle time
		protected void Reload (ReloadOptions flags)
		{
			if (!_reloadScheduled) {
				_reloadScheduled = true;
				_reloadOptions = flags;
				bool force = ((flags & ReloadOptions.Force) == ReloadOptions.Force);
				if (force)
					ReloadCore ();
				else
					Application.Idle += new EventHandler (OnIdle);
			}
		}

		private void OnIdle (object sender,  EventArgs args)
		{
			Application.Idle -= new EventHandler (OnIdle);
			ReloadCore ();
		}

		private void ReloadCore ()
		{
			bool flush = !((_reloadOptions & ReloadOptions.NoFlush) == ReloadOptions.NoFlush);

			if (flush)
				Flush ();
			Unload ();
			_host.Reload ();
			BeginLoad (_host); // calls EndLoad, which will check for ReloadOptions.ModifyOnError
			_reloadScheduled = false;
		}

		private void Unload ()
		{
			if (_loaded) {
				OnBeginUnload ();
				EnableComponentNotification (false);
				_loaded = false;
				_baseComponentClassName = null;
			}
		}


		// The default implementation of ReportFlushErrors raises the last exception in the collection.
		//
		protected virtual void ReportFlushErrors (ICollection errors)
		{
			object last = null;
			foreach (object o in errors)
				last = o;
			throw (Exception)last;
		}
		
		// Must be called during PerformLoad by subclasses.
		protected void SetBaseComponentClassName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			_baseComponentClassName = name;
		}
		
#region IDesignerLoaderService implementation

		void IDesignerLoaderService.AddLoadDependency ()
		{
			_dependenciesCount++;
			if (_dependenciesCount == 0) {
				_dependenciesCount = 1;
				OnBeginLoad ();
			}
		}
		
		void IDesignerLoaderService.DependentLoadComplete (bool successful, ICollection errorCollection)
		{
			if (_dependenciesCount == 0)
				throw new InvalidOperationException ("dependencies == 0");

			_dependenciesCount--;
			if (_dependenciesCount == 0) {
				OnEndLoad (successful,  errorCollection);
			}
		}

		bool IDesignerLoaderService.Reload ()
		{
			if (_dependenciesCount == 0) {
				this.Reload (ReloadOptions.Force);
				return true;
			}
			return false;
		}
#endregion
		
		protected object GetService (Type serviceType)
		{
			if (_host != null)
				return _host.GetService (serviceType);
			return null;
		}
			
		public override void Dispose ()
		{
			this.LoaderHost.RemoveService (typeof (IDesignerLoaderService));
			Unload ();
		}
	}
}

#endif
