//
// System.ComponentModel.Design.DesignSurface
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace System.ComponentModel.Design
{

	public class DesignSurface : IServiceProvider, IDisposable
	{

#region DefaultDesignerLoader : DesignerLoader
		
		internal class DefaultDesignerLoader : DesignerLoader
		{
			//  When DesignSurface.BeginLoad is invoked, the designer loader loads the design document, displays the designer
			//  surface using the IDesignerHost interface, and calls IDesignerLoaderHost.EndLoad
			//  when done. The IDesignerLoaderHost implementation is usually the same class that implements IDesignerHost.

			// The designer loader informs the designer host that it needs to invoke a load or reload so that the designer
			// host can perform additional tasks at these times.

			private Type _componentType;
			private bool _loading;


			public override bool Loading
			{
				get { return _loading; }
			}		  

			public DefaultDesignerLoader (Type componentType)
			{
				if (componentType == null)
					throw new ArgumentNullException ("componentType");
				
				_componentType = componentType;
			}

			// Note that IDesignerLoader : IDesignerHost
			//
			public override void BeginLoad (IDesignerLoaderHost loaderHost)
			{
				_loading = true;
				// initializa root component and designer
				//
				loaderHost.CreateComponent (_componentType);
				// finish off loading - no error collection here.
				//
				loaderHost.EndLoad (_componentType.FullName, true, null);
				_loading = false;
			}
			
			public override void Dispose ()
			{
				_componentType = null;
			}
		} // DesignerLoader

#endregion



		
		private DesignerHost _designerHost;	 
		private DesignSurfaceServiceContainer _serviceContainer;
		private ICollection _loadErrors;
		private bool _isLoaded;
		private DesignerLoader _designerLoader;


		public DesignSurface () : this ((IServiceProvider) null)
		{
		}
		
		public DesignSurface (Type rootComponentType) : this (null, rootComponentType)
		{
		}
		
		
		public DesignSurface (IServiceProvider parentProvider, Type rootComponentType) : this (parentProvider)
		{
			if (rootComponentType == null)
				throw new System.ArgumentNullException ("rootComponentType");

			BeginLoad (rootComponentType);
		}

		// this ctor doesn't load the surface
		//
		public DesignSurface (IServiceProvider parentProvider)
		{
			
			_serviceContainer = new DesignSurfaceServiceContainer (parentProvider);
			_serviceContainer.AddNonReplaceableService (typeof (IServiceContainer), _serviceContainer);

			_designerHost = new DesignerHost ((IServiceProvider) _serviceContainer);
			_designerHost.DesignerLoaderHostLoaded += new LoadedEventHandler (OnDesignerHost_Loaded);
			_designerHost.DesignerLoaderHostLoading += new EventHandler (OnDesignerHost_Loading);
			_designerHost.DesignerLoaderHostUnloading += new EventHandler (OnDesignerHost_Unloading);
			_designerHost.DesignerLoaderHostUnloaded += new EventHandler (OnDesignerHost_Unloaded);

			_designerHost.Activated += new EventHandler (OnDesignerHost_Activated);

			_serviceContainer.AddNonReplaceableService (typeof (IComponentChangeService), _designerHost);
			_serviceContainer.AddNonReplaceableService (typeof (IDesignerHost), _designerHost);
			_serviceContainer.AddNonReplaceableService (typeof (IContainer), _designerHost);
			_serviceContainer.AddService (typeof (ITypeDescriptorFilterService),
							  (ITypeDescriptorFilterService) new TypeDescriptorFilterService (_serviceContainer));

			ExtenderService extenderService = new ExtenderService ();
			_serviceContainer.AddService (typeof (IExtenderProviderService), (IExtenderProviderService) extenderService);
			_serviceContainer.AddService (typeof (IExtenderListService), (IExtenderListService) extenderService);
			_serviceContainer.AddService (typeof (DesignSurface), this);

			SelectionService selectionService = new SelectionService (_serviceContainer);
			_serviceContainer.AddService (typeof (ISelectionService), (ISelectionService) selectionService);
		}
		
		protected ServiceContainer ServiceContainer {
			get {
				if (_designerHost == null)
					throw new ObjectDisposedException ("DesignSurface");

				return _serviceContainer;
			}
		}

		public IContainer ComponentContainer {
			get {
				if (_designerHost == null)
					throw new ObjectDisposedException ("DesignSurface");

				return _designerHost.Container;
			}
		}

		public bool IsLoaded {
			get { return _isLoaded; }
		}

		// Returns a collection of loading errors or a void collection.
		//
		public ICollection LoadErrors {
			get {
					if (_loadErrors == null)
						_loadErrors = new object[0];

					return _loadErrors;
				}   
		}

		public object View {
			get {
				if (_designerHost == null)
					throw new ObjectDisposedException ("DesignSurface");
				
				if (_designerHost.RootComponent == null || this.LoadErrors.Count > 0)
					throw new InvalidOperationException ("The DesignSurface isn't loaded.");

				IRootDesigner designer = _designerHost.GetDesigner (_designerHost.RootComponent) as IRootDesigner;
				if (designer == null)
					throw new InvalidOperationException ("The DesignSurface isn't loaded.");

				ViewTechnology[] viewTech = designer.SupportedTechnologies;
				for (int i = 0; i < viewTech.Length; i++) {
					try { 
						return designer.GetView (viewTech[i]); 
					} catch {}
				}

				throw new NotSupportedException ("No supported View Technology found.");
			}
		}

		public event EventHandler Disposed;
		public event EventHandler Flushed;
		public event LoadedEventHandler Loaded;
		public event EventHandler Loading;
		public event EventHandler Unloaded;
		public event EventHandler Unloading;
		public event EventHandler ViewActivated;

		public void BeginLoad (Type rootComponentType)
		{
			if (rootComponentType == null)
				throw new System.ArgumentNullException ("rootComponentType");
			if (_designerHost == null)
				throw new ObjectDisposedException ("DesignSurface");
			
			this.BeginLoad (new DefaultDesignerLoader (rootComponentType));
		}
		
		public void BeginLoad (DesignerLoader loader)
		{
			if (loader == null)
				throw new System.ArgumentNullException ("loader");
			if (_designerHost == null)
				throw new ObjectDisposedException ("DesignSurface");
			
			if (!_isLoaded) {
				_loadErrors = null;
				_designerLoader = loader;
				this.OnLoading (EventArgs.Empty);
				_designerLoader.BeginLoad (_designerHost);
			}
		} 

		
#region IDisposable

		public void Dispose ()
		{
			this.Dispose (true);
		}


		protected virtual void Dispose (bool disposing)
		{
			if (_designerLoader != null) {
				_designerLoader.Dispose ();
				_designerLoader = null;
			}
			if (_designerHost != null) {
				_designerHost.Dispose ();
				_designerHost.DesignerLoaderHostLoaded -= new LoadedEventHandler (OnDesignerHost_Loaded);
				_designerHost.DesignerLoaderHostLoading -= new EventHandler (OnDesignerHost_Loading);
				_designerHost.DesignerLoaderHostUnloading -= new EventHandler (OnDesignerHost_Unloading);
				_designerHost.DesignerLoaderHostUnloaded -= new EventHandler (OnDesignerHost_Unloaded);
				_designerHost.Activated -= new EventHandler (OnDesignerHost_Activated);
				_designerHost = null;	
			}
			if (_serviceContainer != null) {
				_serviceContainer.Dispose ();
				_serviceContainer = null;
			}
			
			if (Disposed != null)
				Disposed (this, EventArgs.Empty);
		}
		
#endregion

		
		public void Flush ()
		{	   
			if (_designerLoader != null)
				_designerLoader.Flush ();

			if (Flushed != null)
				Flushed (this, EventArgs.Empty);
		}

		private void OnDesignerHost_Loaded (object sender, LoadedEventArgs e)
		{		   
			this.OnLoaded (e);
		}

		private void OnDesignerHost_Loading (object sender, EventArgs e)
		{		   
			this.OnLoading (EventArgs.Empty);
		}


		private void OnDesignerHost_Unloading (object sender, EventArgs e)
		{		   
			this.OnUnloading (EventArgs.Empty);
		}


		private void OnDesignerHost_Unloaded (object sender, EventArgs e)
		{		   
			this.OnUnloaded (EventArgs.Empty);
		}
		
		protected virtual void OnLoaded (LoadedEventArgs e)
		{
			_loadErrors = e.Errors;
			_isLoaded = e.HasSucceeded;
			
			if (Loaded != null)
				Loaded (this, e);
		}
		
		protected virtual void OnLoading (EventArgs e)
		{
			if (Loading != null)
				Loading (this, e);
		}

		
		protected virtual void OnUnloaded (EventArgs e)
		{
			if (Unloaded != null)
				Unloaded (this, e);
		}

		
		protected virtual void OnUnloading (EventArgs e)
		{
			if (Unloading != null)
				Unloading (this, e);
		}

		internal void OnDesignerHost_Activated (object sender, EventArgs args)
		{
			this.OnViewActivate (EventArgs.Empty);
		}
		
		protected virtual void OnViewActivate (EventArgs e)
		{
			if (ViewActivated != null)
				ViewActivated (this, e);
		}

		
		[ObsoleteAttribute("CreateComponent has been replaced by CreateInstance")] 
		protected internal virtual IComponent CreateComponent (Type componentType)
		{
			return (this.CreateInstance (componentType)) as IComponent;
		}


		// XXX: I am not quite sure if this should add the created instance of the component
		// to the surface, but it does. (If one finds out that this is wrong just use
		// _designerHost.CreateInstance (..)
		//
		protected internal virtual object CreateInstance (Type type)
		{
			if (type == null)
				throw new System.ArgumentNullException ("type");

			return _designerHost.CreateComponent (type);
		}

		
		protected internal virtual IDesigner CreateDesigner (IComponent component, bool rootDesigner)
		{
			if (component == null)
				throw new System.ArgumentNullException ("component");
			if (_designerHost == null)
				throw new System.ObjectDisposedException ("DesignerSurface");

			return _designerHost.CreateDesigner (component, rootDesigner);
		}

		public INestedContainer CreateNestedContainer (IComponent owningComponent)
		{
			return this.CreateNestedContainer (owningComponent, null);
		}

		public INestedContainer CreateNestedContainer (IComponent owningComponent, string containerName)
		{
			if (_designerHost == null)
				throw new ObjectDisposedException ("DesignSurface");

			return new DesignModeNestedContainer (owningComponent, containerName);
		}


#region IServiceProvider

		public object GetService (Type serviceType)
		{
			if (typeof (IServiceContainer) == serviceType)
				return _serviceContainer;
			
			return _serviceContainer.GetService (serviceType);
		}

#endregion

	}

}
#endif
