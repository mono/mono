//
// System.ComponentModel.Design.DesignModeSite
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

namespace System.ComponentModel.Design
{

	internal class DesignModeSite : ISite, IDictionaryService, IServiceProvider, IServiceContainer
	{

		// The DesignModeSite:
		//  * offers the IDictionaryService and INestedContaineroffers site-specific services
		//  * implements the IServiceContainer interface, but according to the tests it:
		//    - does *NOT* offer IServiceContainer as a site-specific service
		//    - offers the added site-specific services via IServiceProvider
		
		private IServiceProvider _serviceProvider;
		private IComponent _component;
		private IContainer _container;
		private string _componentName;
		private NestedContainer _nestedContainer;


		public DesignModeSite (IComponent component, string name, IContainer container, IServiceProvider serviceProvider)
		{
			_component = component;
			_container = container;
			_componentName = name;
			_serviceProvider = serviceProvider;
		}

		public IComponent Component {
			get { return _component; }
		}

		public IContainer Container {
			get { return _container; }
		}

		// Yay yay yay, guess who's in design mode ???
		//
		public bool DesignMode {
			get { return true; }
		}

		// The place where renaming of a component takes place.
		// We should validate the new name here, if INameCreation service is present
		//
		public string Name {
			get {
				return _componentName;
			}
			set {
				if (value != _componentName && value != null && value.Trim().Length > 0) {
					INameCreationService nameService = this.GetService (typeof (INameCreationService)) as INameCreationService;
					IComponent component = _container.Components[value]; // get the component with that name

					if (component == null &&
					    (nameService == null || (nameService != null && nameService.IsValidName (value)))) {
						string oldName = _componentName;
						_componentName = value;
						((DesignerHost)this.GetService (typeof (IDesignerHost))).OnComponentRename (_component, oldName, _componentName);
					}
				}
			}
		}

#region IServiceContainer

		private ServiceContainer _siteSpecificServices;

		private ServiceContainer SiteSpecificServices {
			get {
				if (_siteSpecificServices == null)
					_siteSpecificServices = new ServiceContainer (null);

				return _siteSpecificServices;
			}
		}

		void IServiceContainer.AddService (Type serviceType, object serviceInstance)
		{
			SiteSpecificServices.AddService (serviceType, serviceInstance);
		}

		void IServiceContainer.AddService (Type serviceType, object serviceInstance, bool promote)
		{
			SiteSpecificServices.AddService (serviceType, serviceInstance, promote);
		}
		void IServiceContainer.AddService (Type serviceType, ServiceCreatorCallback callback)
		{
			SiteSpecificServices.AddService (serviceType, callback);
		}

		void IServiceContainer.AddService (Type serviceType, ServiceCreatorCallback callback, bool promote)
		{
			SiteSpecificServices.AddService (serviceType, callback, promote);
		}

		void IServiceContainer.RemoveService (Type serviceType)
		{
			SiteSpecificServices.RemoveService (serviceType);
		}

		void IServiceContainer.RemoveService (Type serviceType, bool promote)
		{
			SiteSpecificServices.RemoveService (serviceType, promote);
		}

#endregion


#region IDictionaryService

		private Hashtable _dictionary;

		object IDictionaryService.GetKey (object value)
		{
			if (_dictionary != null) {
				foreach (DictionaryEntry entry in _dictionary) {
					if (value != null && value.Equals (entry.Value))
						return entry.Key;
				}
			}
			return null;
		}

		object IDictionaryService.GetValue (object key)
		{
			if (_dictionary != null)
				return _dictionary[key];

			return null;
		}

		// No Remove method: seting the value to null
		// will remove the pair.
		//
		void IDictionaryService.SetValue (object key, object value)
		{
			if (_dictionary == null)
				_dictionary = new Hashtable ();

			if (value == null)
				_dictionary.Remove (key);

			_dictionary[key] = value;
		}

#endregion

		
#region IServiceProvider

		public virtual object GetService (Type service)
		{
			object serviceInstance = null;

			if (typeof (IDictionaryService) == service)
				serviceInstance = (IDictionaryService) this;

			if (typeof (INestedContainer) == service) {
				if (_nestedContainer == null)
					_nestedContainer = new DesignModeNestedContainer (_component, null);
				serviceInstance = _nestedContainer;
			}

			// Avoid returning the site specific IServiceContainer
			if (serviceInstance == null && service != typeof (IServiceContainer) &&
			    _siteSpecificServices != null)
				serviceInstance = _siteSpecificServices.GetService (service);

			if (serviceInstance == null)
				serviceInstance = _serviceProvider.GetService (service);

			return serviceInstance;
		}
#endregion

	}
}
#endif
