//
// System.ComponentModel.Design.Serialization.DesignerSerializationManager
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


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;



namespace System.ComponentModel.Design.Serialization
{
	
	public class DesignerSerializationManager : IDesignerSerializationManager, IServiceProvider
	{
		
		private class Session : IDisposable
		{
			
			private DesignerSerializationManager _manager;
			
			public Session (DesignerSerializationManager manager)
			{
				_manager = manager;
			}
			
			public void Dispose ()
			{
				_manager.OnSessionDisposed (EventArgs.Empty);
			}
		}

		
		
		public DesignerSerializationManager () : this (null)
		{
		}
		
		// This constructor sets the PreserveNames and ValidateRecycledTypes properties to true.
		//
		public DesignerSerializationManager (IServiceProvider provider)
		{
			_serviceProvider = provider;
			_preserveNames = true;
			_validateRecycledTypes = true;
		}
		
		private IServiceProvider _serviceProvider;
		private bool _preserveNames = false;
		private bool _validateRecycledTypes = false;
		private bool _recycleInstances = false;
		private IContainer _designerContainer = null;
		private object _propertyProvider = null;
		private Session _session = null;
		private ArrayList _errors = null;
		private List <IDesignerSerializationProvider> _serializationProviders;
		private Dictionary <Type, object> _serializersCache = null; // componentType - serializer instance
		private Dictionary <string, object> _instancesByNameCache = null; // name - instance
		private Dictionary <object, string> _instancesByValueCache = null; // instance - name
		private ContextStack _contextStack = null;
		
		
		public bool RecycleInstances {
			get { return _recycleInstances; }
			set {
				VerifyNotInSession ();
				_recycleInstances = value; 
			}
		}
		
		public bool PreserveNames {
			get { return _preserveNames; }
			set {
				VerifyNotInSession ();
				_preserveNames = value; 
			}
		}
		
		public bool ValidateRecycledTypes {
			get { return _validateRecycledTypes; }
			set {
				VerifyNotInSession ();
				_validateRecycledTypes = value; 
			}
		}
		
		public IContainer Container { 
			get {
				if (_designerContainer == null) {
					_designerContainer = (this.GetService (typeof (IDesignerHost)) as IDesignerHost).Container;
				}
				return _designerContainer;
			}
			set{
				VerifyNotInSession ();
				_designerContainer = value;
			}
		}
		
		public object PropertyProvider {
			get { return _propertyProvider; }
			set { _propertyProvider = value; }
		}
		
		public IList Errors {
			get { return _errors; }
		}
		
		public event EventHandler SessionDisposed;
		public event EventHandler SessionCreated;
		
		protected virtual void OnSessionCreated (EventArgs e)
		{
			if (SessionCreated != null) {
				SessionCreated (this, e);
			}
		}
			
		// For behaviour description:
		//
		// http://msdn2.microsoft.com/en-us/library/system.componentmodel.design.serialization.designerserializationmanager.validaterecycledtypes.aspx
		// http://msdn2.microsoft.com/en-us/library/system.componentmodel.design.serialization.designerserializationmanager.preservenames.aspx
		//
		protected virtual object CreateInstance (Type type, ICollection arguments, string name, bool addToContainer)
		{
			VerifyInSession ();
			object instance = null;

			if (name != null && _recycleInstances) {
				_instancesByNameCache.TryGetValue (name, out instance);
				if (instance != null && _validateRecycledTypes) {
					if (instance.GetType () != type)
						instance = null;
				}
			}
			
			if (instance == null || !_recycleInstances)
				instance = this.CreateInstance (type, arguments);
		
			if (addToContainer && instance != null && this.Container != null && typeof (IComponent).IsAssignableFrom (type)) {
				if (_preserveNames) {
					this.Container.Add ((IComponent) instance, name);
				}
				else {
					if (name != null && this.Container.Components[name] != null) {
						this.Container.Add ((IComponent) instance);
					}
					else {
						this.Container.Add ((IComponent) instance, name);
					}
				}
				ISite site = ((IComponent)instance).Site; // get the name from the site in case a name has been generated.
				if (site != null)
					name = site.Name;
			}
			
			if (instance != null && name != null) {
				_instancesByNameCache[name] = instance;
				_instancesByValueCache[instance] = name;
			}
			
			return instance;
		}

		// Invokes the constructor that matches the arguments
		//
		private object CreateInstance (Type type, ICollection argsCollection)
		{
			object instance = null;
			object[] arguments = null;
			Type[] types = new Type[0];

			if (argsCollection != null) {
				arguments = new object[argsCollection.Count];
				types = new Type[argsCollection.Count];
				argsCollection.CopyTo (arguments, 0);

				for (int i=0; i < arguments.Length; i++) {
					if (arguments[i] == null)
						types[i] = null;
					else
						types[i] = arguments[i].GetType ();
				}
			}

			ConstructorInfo ctor = type.GetConstructor (types);
			if (ctor != null) {
				instance = ctor.Invoke (arguments);
			}

			return instance;
		}

		public object GetSerializer (Type objectType, Type serializerType)
		{
			VerifyInSession ();
			
			if (serializerType == null)
				throw new ArgumentNullException ("serializerType");
				
			object serializer = null;

			if (objectType != null) {
				// try 1: from cache
				//
				_serializersCache.TryGetValue (objectType, out serializer);

				// check for provider attribute and add it to the list of providers
				//
				if (serializer != null && !serializerType.IsAssignableFrom (serializer.GetType ()))
					serializer = null;
				
				AttributeCollection attributes = TypeDescriptor.GetAttributes (objectType);
				DefaultSerializationProviderAttribute providerAttribute = attributes[typeof (DefaultSerializationProviderAttribute)] 
																			   as DefaultSerializationProviderAttribute;
				if (providerAttribute != null && this.GetType (providerAttribute.ProviderTypeName) == serializerType) {
					object provider = Activator.CreateInstance (this.GetType (providerAttribute.ProviderTypeName), 
																 BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic, 
																 null, null, null);
					((IDesignerSerializationManager)this).AddSerializationProvider ((IDesignerSerializationProvider) provider);
				}
			}

			// try 2: DesignerSerializerAttribute
			//
			if (serializer == null && objectType != null) {
				AttributeCollection attributes = TypeDescriptor.GetAttributes (objectType);
				DesignerSerializerAttribute serializerAttribute = attributes[typeof (DesignerSerializerAttribute)] as DesignerSerializerAttribute;
				if (serializerAttribute != null && 
					this.GetType (serializerAttribute.SerializerBaseTypeName) == serializerType) {
					try {
						serializer = Activator.CreateInstance (this.GetType (serializerAttribute.SerializerTypeName), 
										       BindingFlags.CreateInstance | BindingFlags.Instance | 
										       BindingFlags.Public | BindingFlags.NonPublic, 
										       null, null, null);
					} catch {}
				}
				
				if (serializer != null)
					_serializersCache[objectType] = serializer;
			}

			// try 3: from provider
			//
			if (serializer == null && _serializationProviders != null) {
				foreach (IDesignerSerializationProvider provider in _serializationProviders) {
					serializer = provider.GetSerializer (this, null, objectType, serializerType);
					if (serializer != null)
						break;
				}
			}

			return serializer;
		}

		private void VerifyInSession ()
		{
			if (_session == null)
				throw new InvalidOperationException ("Not in session.");
		}
		
		private void VerifyNotInSession ()
		{
			if (_session != null)
				throw new InvalidOperationException ("In session.");
		}
		
		public IDisposable CreateSession ()
		{
			VerifyNotInSession ();
			_errors = new ArrayList ();
			_session = new Session (this);
			_serializersCache = new Dictionary<System.Type,object> ();
			_instancesByNameCache = new Dictionary<string,object> ();
			_instancesByValueCache = new Dictionary<object, string> ();
			_contextStack = new ContextStack ();
			
			this.OnSessionCreated (EventArgs.Empty);

			return _session;
		}
		
		protected virtual void OnSessionDisposed (EventArgs e)
		{
			_errors.Clear ();
			_errors = null;
			_serializersCache.Clear ();
			_serializersCache = null;
			_instancesByNameCache.Clear ();
			_instancesByNameCache = null;
			_instancesByValueCache.Clear ();
			_instancesByValueCache = null;
			_session = null;
			_contextStack = null;
			_resolveNameHandler = null;
			_serializationCompleteHandler = null;
			
			if (SessionDisposed != null) {
				SessionDisposed (this, e);
			}

			if (_serializationCompleteHandler != null)
				_serializationCompleteHandler (this, EventArgs.Empty);
		}
				
		protected virtual Type GetType (string typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");
			
			this.VerifyInSession ();
			
			Type result = null;
			ITypeResolutionService typeResSvc = this.GetService (typeof (ITypeResolutionService)) as ITypeResolutionService;
			if (typeResSvc != null)
				result = typeResSvc.GetType (typeName);
			if (result == null)
				result = Type.GetType (typeName);
			
			return result;
		}
										
#region IDesignerSerializationManager implementation
		
		protected virtual void OnResolveName (ResolveNameEventArgs e)
		{
			if (_resolveNameHandler != null) {
				_resolveNameHandler (this, e);
			}
		}	
		
		void IDesignerSerializationManager.AddSerializationProvider (IDesignerSerializationProvider provider)
		{
			if (_serializationProviders == null)
				_serializationProviders = new List <IDesignerSerializationProvider> ();
			
			if (!_serializationProviders.Contains (provider))
				_serializationProviders.Add (provider);
		}
		
		void IDesignerSerializationManager.RemoveSerializationProvider (IDesignerSerializationProvider provider)
		{
			if (_serializationProviders != null)
				_serializationProviders.Remove (provider);
		}
		
		object IDesignerSerializationManager.CreateInstance (Type type, ICollection arguments, string name, bool addToContainer)
		{
			return this.CreateInstance (type, arguments, name, addToContainer);
		}
		
		object IDesignerSerializationManager.GetInstance (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.VerifyInSession ();
			
			object instance = null;
			_instancesByNameCache.TryGetValue (name, out instance);

			if (instance == null && this.Container != null)
				instance = this.Container.Components[name];

			if (instance == null)
				instance = this.RequestInstance (name);

			return instance;
		}
		
		private object RequestInstance (string name)
		{
			ResolveNameEventArgs args = new ResolveNameEventArgs (name);
			this.OnResolveName (args);
			return args.Value;
		}
		
		Type IDesignerSerializationManager.GetType (string name)
		{
			return this.GetType (name);
		}
		
		object IDesignerSerializationManager.GetSerializer (Type type, Type serializerType)
		{
			return this.GetSerializer (type, serializerType);
		}
		
		string IDesignerSerializationManager.GetName (object instance)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			this.VerifyInSession ();
			
			string name = null;
			if (instance is IComponent) {
				ISite site = ((IComponent)instance).Site;
				if (site != null && site is INestedSite)
					name = ((INestedSite)site).FullName;
				else if (site != null)
					name = site.Name;
			}
			if (name == null)
				_instancesByValueCache.TryGetValue (instance, out name);
			return name;
		}
		
		void IDesignerSerializationManager.SetName (object instance, string name)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (name == null)
				throw new ArgumentNullException ("name");
			
			if (_instancesByNameCache.ContainsKey (name))
				throw new ArgumentException ("The object specified by instance already has a name, or name is already used by another named object.");
			
			_instancesByNameCache.Add (name, instance);
			_instancesByValueCache.Add (instance, name);
		}
		
		void IDesignerSerializationManager.ReportError (object error)
		{
		   this.VerifyInSession ();
		   _errors.Add (error);
		}
		
		ContextStack IDesignerSerializationManager.Context {
			get { return _contextStack; }
		}
		
		PropertyDescriptorCollection IDesignerSerializationManager.Properties {
			get {
				PropertyDescriptorCollection properties = new PropertyDescriptorCollection (new PropertyDescriptor[0]);
				object component = this.PropertyProvider;
				if (component != null)
				   properties = TypeDescriptor.GetProperties (component);
				
				return properties;
			}
		}
		
		private EventHandler _serializationCompleteHandler;
		private ResolveNameEventHandler _resolveNameHandler;
		
		event EventHandler IDesignerSerializationManager.SerializationComplete {
			add {
				this.VerifyInSession ();
				_serializationCompleteHandler = (EventHandler) Delegate.Combine (_serializationCompleteHandler, value);
			}
			remove {
				_serializationCompleteHandler = (EventHandler) Delegate.Remove (_serializationCompleteHandler, value);
			}
		}
				
		event ResolveNameEventHandler IDesignerSerializationManager.ResolveName {
			add {
				this.VerifyInSession ();
				_resolveNameHandler = (ResolveNameEventHandler) Delegate.Combine (_resolveNameHandler, value);
			}
			remove {
				_resolveNameHandler = (ResolveNameEventHandler) Delegate.Remove (_resolveNameHandler, value);
			}
		}	  
#endregion
		
		object IServiceProvider.GetService (Type serviceType)
		{
			return this.GetService (serviceType);
		}
		
		protected virtual object GetService (Type serviceType)
		{
			object result = null;
			if (_serviceProvider != null)
				result = _serviceProvider.GetService (serviceType);
			
			return result;
		}
	}
}
