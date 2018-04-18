//
// System.ComponentModel.Design.ComponentDesigner
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


using System;
using System.Collections;
using System.ComponentModel;

namespace System.ComponentModel.Design
{

	public class ComponentDesigner : ITreeDesigner, IDesigner, IDisposable, IDesignerFilter, IComponentInitializer
	{

#region ShadowPropertyCollection

		protected sealed class ShadowPropertyCollection
		{

			private Hashtable _properties = null;
			private IComponent _component;

			internal ShadowPropertyCollection (IComponent component)
			{
				_component = component;
			}

			// Returns Control's property value (if available) if there is no shadowed one.
			//
			public object this[string propertyName]
			{
				get {
					if (propertyName == null)
						throw new System.ArgumentNullException("propertyName");

					if (_properties != null && _properties.ContainsKey (propertyName))
						return _properties[propertyName];

					PropertyDescriptor property = TypeDescriptor.GetProperties (_component.GetType ())[propertyName];
					if (property != null)
						return property.GetValue (_component);
					else
						throw new System.Exception ("Propery not found!");
				}
				set {
					if (_properties == null)
						_properties = new Hashtable ();
					_properties[propertyName] = value;
				}
			}

			public bool Contains (string propertyName)
			{
				if (_properties != null)
					return _properties.ContainsKey (propertyName);
				else
					return false;
			}

		} // ShadowPropertyCollection
#endregion

		public ComponentDesigner ()
		{
		}


		private IComponent _component;
		private DesignerVerbCollection _verbs;
		private ShadowPropertyCollection _shadowPropertyCollection;
		private DesignerActionListCollection _designerActionList;

		// This property indicates any components to copy or move along with the component managed
		// by the designer during a copy, drag, or move operation.
		// If this collection contains references to other components in the current design mode document,
		// those components will be copied along with the component managed by the designer during a copy operation.
		// When the component managed by the designer is selected, this collection is filled with any nested controls.
		// This collection can also include other components, such as the buttons of a toolbar.
		//
		// supposedly contains all the children of the component, thus used for ITreeDesigner.Children
		//
		public virtual ICollection AssociatedComponents {
			get { return new IComponent[0]; }
		}

		public IComponent Component {
			get { return _component; }
		}

		public virtual DesignerVerbCollection Verbs {
			get {
				if (_verbs == null)
					_verbs = new DesignerVerbCollection ();

				return _verbs;
			}
		}

		protected virtual InheritanceAttribute InheritanceAttribute {
			get {
				IInheritanceService service = (IInheritanceService) this.GetService (typeof (IInheritanceService));
				if (service != null)
					return service.GetInheritanceAttribute (_component);
				else
					return InheritanceAttribute.Default;
			}
		}

		protected bool Inherited {
			get { return !this.InheritanceAttribute.Equals (InheritanceAttribute.NotInherited); }
		}

		//Gets a collection of property values that override user settings.
		//
		protected ShadowPropertyCollection ShadowProperties {
			get {
				if (_shadowPropertyCollection == null) {
					_shadowPropertyCollection = new ShadowPropertyCollection(_component);
				}
				return _shadowPropertyCollection;
			}
		}

		public virtual DesignerActionListCollection ActionLists {
			get {
				if (_designerActionList == null)
					_designerActionList = new DesignerActionListCollection ();

				return _designerActionList;
			}
		}

		protected virtual IComponent ParentComponent {
			get {
				IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
				if (host != null) {
					IComponent rootComponent = host.RootComponent;
					if (rootComponent != _component)
						return rootComponent;
				}
				return null;
			}
		}

		public virtual void InitializeNewComponent (IDictionary defaultValues)
		{
			// Reset
			//
			OnSetComponentDefaults ();
		}

		// MSDN: The default implementation of this method does nothing.
		//
		public virtual void InitializeExistingComponent (IDictionary defaultValues)
		{
			InitializeNonDefault ();
		}


		public virtual void Initialize (IComponent component)
		{
			if (component == null)
				throw new ArgumentNullException ("component");
								
			_component = component;
		}

		[Obsolete ("This method has been deprecated. Use InitializeExistingComponent instead.")]
		public virtual void InitializeNonDefault ()
		{
		}


		// This method is called when a user double-clicks (the representation of) a component.
		// Tries to bind the default event to a method or creates a new one.
		// 
		public virtual void DoDefaultAction()
		{
			IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
			DesignerTransaction transaction = null;
			if (host != null)
				transaction = host.CreateTransaction ("ComponentDesigner_AddEvent");

			IEventBindingService eventBindingService = GetService (typeof(IEventBindingService)) as IEventBindingService;
			EventDescriptor defaultEventDescriptor = null;

			if (eventBindingService != null) {
				ISelectionService selectionService = this.GetService (typeof (ISelectionService)) as ISelectionService;
				try {
					if (selectionService != null) {
						ICollection selectedComponents = selectionService.GetSelectedComponents ();

						foreach (IComponent component in selectedComponents) {
							EventDescriptor eventDescriptor = TypeDescriptor.GetDefaultEvent (component);
							if (eventDescriptor != null) {
								PropertyDescriptor eventProperty = eventBindingService.GetEventProperty (eventDescriptor);
								if (eventProperty != null && !eventProperty.IsReadOnly) {
									string methodName = eventProperty.GetValue (component) as string;
									bool newMethod = true;

									if (methodName != null || methodName != String.Empty) {
										ICollection compatibleMethods = eventBindingService.GetCompatibleMethods (eventDescriptor);
										foreach (string signature in compatibleMethods) {
											if (signature == methodName) {
												newMethod = false;
												break;
											}
										}
									}
									if (newMethod) {
										if (methodName == null)
											methodName = eventBindingService.CreateUniqueMethodName (component, eventDescriptor);
															
										eventProperty.SetValue (component, methodName);
									}

									if (component == _component)
										defaultEventDescriptor = eventDescriptor;
								}
							}
						}

					}
				}
				catch {
					if (transaction != null) {
						transaction.Cancel ();
						transaction = null;
					}
				}
				finally {
					if (transaction != null)
						transaction.Commit ();
				}

				if (defaultEventDescriptor != null)
					eventBindingService.ShowCode (_component, defaultEventDescriptor);
			}
		}



		[Obsolete ("This method has been deprecated. Use InitializeNewComponent instead.")]
		// The default implementation of this method sets the default property of the component to
		// the name of the component if the default property is a string and the property is not already set.
		// This method can be implemented in a derived class to customize the initialization of the component
		// that this designer is designing.
		//
		public virtual void OnSetComponentDefaults ()
		{
			if (_component != null && _component.Site != null) {
				PropertyDescriptor property = TypeDescriptor.GetDefaultProperty (_component);
				if (property != null && property.PropertyType.Equals (typeof (string))) {
					string propertyValue = (string)property.GetValue (_component);
					if (propertyValue != null && propertyValue.Length != 0)
						property.SetValue (_component, _component.Site.Name);
				}
			}
		}




		protected InheritanceAttribute InvokeGetInheritanceAttribute (ComponentDesigner toInvoke)
		{
			return toInvoke.InheritanceAttribute;
		}

#region IDesignerFilter

		// TypeDescriptor queries the component's site for ITypeDescriptorFilterService 
		// then invokes ITypeDescriptorFilterService.XXXX before retrieveing props/event/attributes, 
		// which then invokes the IDesignerFilter implementation of the component
		// 
		protected virtual void PostFilterAttributes (IDictionary attributes)
		{
		}

		protected virtual void PostFilterEvents (IDictionary events)
		{
		}

		protected virtual void PostFilterProperties (IDictionary properties)
		{
		}

		protected virtual void PreFilterAttributes (IDictionary attributes)
		{
		}

		protected virtual void PreFilterEvents (IDictionary events)
		{
		}

		protected virtual void PreFilterProperties (IDictionary properties)
		{
		}
#endregion

		protected void RaiseComponentChanged (MemberDescriptor member, object oldValue, object newValue)
		{
			IComponentChangeService service = GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (service != null)
				service.OnComponentChanged (_component, member, oldValue, newValue);
		}

		protected void RaiseComponentChanging (MemberDescriptor member)
		{
			IComponentChangeService service = GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (service != null)
				service.OnComponentChanging (_component, member);
		}

#region Implementation of IDesignerFilter

		void IDesignerFilter.PostFilterAttributes (IDictionary attributes)
		{
			PostFilterAttributes (attributes);
		}

		void IDesignerFilter.PostFilterEvents (IDictionary events)
		{
			PostFilterEvents (events);
		}

		void IDesignerFilter.PostFilterProperties (IDictionary properties)
		{
			PostFilterProperties (properties);
		}

		void IDesignerFilter.PreFilterAttributes (IDictionary attributes)
		{
			PreFilterAttributes (attributes);
		}

		void IDesignerFilter.PreFilterEvents (IDictionary events)
		{
			PreFilterEvents (events);
		}

		void IDesignerFilter.PreFilterProperties (IDictionary properties)
		{
			PreFilterProperties (properties);
		}

#endregion


#region ITreeDesigner
		// Returns a collection of the designers of the associated components
		//
		ICollection ITreeDesigner.Children {
			get {
				ICollection components = this.AssociatedComponents;
				IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
				
				if (host != null) {
					ArrayList designers = new ArrayList ();
					foreach (IComponent component in components) {
						IDesigner designer = host.GetDesigner (component);
						if (designer != null)
							designers.Add (designer);
					}
					IDesigner[] result = new IDesigner[designers.Count];
					designers.CopyTo (result);
					return result;
				}
				return new IDesigner[0];
			}
		}

		IDesigner ITreeDesigner.Parent {
			get {
				IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
				if (host != null && this.ParentComponent != null)
					return host.GetDesigner (this.ParentComponent);
	
				return null;
			}
		}
#endregion

		// Helper method - not an ISerivceProvider
		//
		protected virtual object GetService (Type serviceType)
		{
			if (_component != null && _component.Site != null)
				return _component.Site.GetService (serviceType);

			return null;
		}

		public void Dispose ()
		{
			this.Dispose (true);
			GC.SuppressFinalize (this);
		}


		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				_component = null;
		}

		~ComponentDesigner ()
		{
			this.Dispose (false);
		}
	}
}
