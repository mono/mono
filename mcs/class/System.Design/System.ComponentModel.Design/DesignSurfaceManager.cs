//
// System.ComponentModel.Design.DesignSurfaceManager
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006 Ivan N. Zlatev

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
using System.ComponentModel;
using System.Collections;

namespace System.ComponentModel.Design
{

	public class DesignSurfaceManager : IServiceProvider, IDisposable
	{

		private class MergedServiceProvider : IServiceProvider
		{
			private IServiceProvider _primaryProvider;
			private IServiceProvider _secondaryProvider;
			
			public MergedServiceProvider (IServiceProvider primary, IServiceProvider secondary)
			{
				if (primary == null)
					throw new ArgumentNullException ("primary");
				if (secondary == null)
					throw new ArgumentNullException ("secondary");
				
				_primaryProvider = primary;
				_secondaryProvider = secondary;
			}

			public object GetService (Type service)
			{
				object result = _primaryProvider.GetService (service);
				if (result == null)
					result = _secondaryProvider.GetService (service);

				return result;
			}
			
		} // MergedServiceProvider

		
		private IServiceProvider _parentProvider;
		private ServiceContainer _serviceContainer;
		
		public DesignSurfaceManager () : this (null)
		{
		}

		public DesignSurfaceManager (IServiceProvider parentProvider)
		{
			_parentProvider = parentProvider;
			this.ServiceContainer.AddService (typeof (IDesignerEventService), new DesignerEventService ());
		}

		// The CreateDesignSurfaceCore method is called by both CreateDesignSurface methods.
		// It is the implementation that actually creates the design surface. The default
		// implementation just returns a new DesignSurface. You may override this method to provide
		// a custom object that derives from the DesignSurface class.
		//
		protected virtual DesignSurface CreateDesignSurfaceCore (IServiceProvider parentProvider)
		{
			DesignSurface surface = new DesignSurface (parentProvider);
			OnDesignSurfaceCreated (surface);
			return surface;
		}
		
		public DesignSurface CreateDesignSurface ()
		{
			return CreateDesignSurfaceCore (this);	
		}

		// MSDN: parentProvider - A parent service provider. A new merged service provider will be created that
		// will first ask this provider for a service, and then delegate any failures to the design surface
		// manager object. This merged provider will be passed into the CreateDesignSurfaceCore method.
		//
		public DesignSurface CreateDesignSurface (IServiceProvider parentProvider)
		{
			if (parentProvider == null)
				throw new ArgumentNullException ("parentProvider");
			
			return CreateDesignSurfaceCore (new MergedServiceProvider (parentProvider, this));
		}
		
		public virtual DesignSurface ActiveDesignSurface {
			get {
				DesignerEventService eventService = GetService (typeof (IDesignerEventService)) as DesignerEventService;
				if (eventService != null) {
					IDesignerHost designer = eventService.ActiveDesigner;
					if (designer != null)
						return designer.GetService (typeof (DesignSurface)) as DesignSurface;
				}
				return null;
			}
			set {
				if (value != null) {
					DesignSurface oldSurface = null;
				
					// get current surface
					DesignerEventService eventService = GetService (typeof (IDesignerEventService)) as DesignerEventService;
					if (eventService != null) {
						IDesignerHost designer = eventService.ActiveDesigner;
						if (designer != null)
							oldSurface = designer.GetService (typeof (DesignSurface)) as DesignSurface;
					}
					
					ISelectionService selectionService = null;
					
					if (oldSurface != value) {
						// unsubscribe from current's selectionchanged
						if (oldSurface != null) {
							selectionService = oldSurface.GetService (typeof (ISelectionService)) as ISelectionService;
							if (selectionService != null)
								selectionService.SelectionChanged -= new EventHandler (OnSelectionChanged);
						}
						// subscribe to new's selectionchanged
						selectionService = value.GetService (typeof (ISelectionService)) as ISelectionService;
						if (selectionService != null)
							selectionService.SelectionChanged += new EventHandler (OnSelectionChanged);
						
						// set it
						eventService.ActiveDesigner = value.GetService (typeof (IDesignerHost)) as IDesignerHost;
						
						// fire event
						if (this.ActiveDesignSurfaceChanged != null)
							this.ActiveDesignSurfaceChanged (this, new ActiveDesignSurfaceChangedEventArgs (oldSurface, value));
					}
				}
			}
		}
		
		public DesignSurfaceCollection DesignSurfaces {
			get {
				DesignerEventService eventService = GetService (typeof (IDesignerEventService)) as DesignerEventService;
				if (eventService != null) 
					return new DesignSurfaceCollection (eventService.Designers);

				return new DesignSurfaceCollection (null);
			}
		}
		
		protected ServiceContainer ServiceContainer {
			get {
				if (_serviceContainer == null)
					_serviceContainer = new ServiceContainer (_parentProvider);

				return _serviceContainer;
			}
		}
				
		// MSDN2 says those events are mapped through the IDesignerEventService,
		// but I preferd not to do that. Should not cause compitability issues.
		//
		// 
		// The SelectionChanged is fired only for a changed component selection on the
		// active designersurface.
		//
		public event EventHandler SelectionChanged;	 
		public event DesignSurfaceEventHandler DesignSurfaceDisposed;
		public event DesignSurfaceEventHandler DesignSurfaceCreated;
		public event ActiveDesignSurfaceChangedEventHandler ActiveDesignSurfaceChanged;
		
		private void OnSelectionChanged (object sender, EventArgs args)
		{
			if (SelectionChanged != null)
				SelectionChanged (this, EventArgs.Empty);
			
			DesignerEventService eventService = GetService (typeof (IDesignerEventService)) as DesignerEventService;
			if (eventService != null)
				eventService.RaiseSelectionChanged ();
		}
		
		private void OnDesignSurfaceCreated (DesignSurface surface)
		{
			if (DesignSurfaceCreated != null)
				DesignSurfaceCreated (this, new DesignSurfaceEventArgs (surface));
			
			// monitor disposing
			surface.Disposed += new EventHandler (OnDesignSurfaceDisposed);
			
			DesignerEventService eventService = GetService (typeof (IDesignerEventService)) as DesignerEventService;
			if (eventService != null)
				eventService.RaiseDesignerCreated (surface.GetService (typeof (IDesignerHost)) as IDesignerHost);
		}

		private void OnDesignSurfaceDisposed (object sender, EventArgs args)
		{
			DesignSurface surface = (DesignSurface) sender;
			
			surface.Disposed -= new EventHandler (OnDesignSurfaceDisposed);
			
			if (DesignSurfaceDisposed != null)
				DesignSurfaceDisposed (this, new DesignSurfaceEventArgs (surface));
			
			DesignerEventService eventService = GetService (typeof (IDesignerEventService)) as DesignerEventService;
			if (eventService != null)
				eventService.RaiseDesignerDisposed (surface.GetService (typeof (IDesignerHost)) as IDesignerHost);
				
		}
		
		public object GetService (Type serviceType)
		{
			if (_serviceContainer != null)
				return _serviceContainer.GetService (serviceType);

			return null;
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing && _serviceContainer != null) {
				_serviceContainer.Dispose ();
				_serviceContainer = null;
			}
		}
	}

}
