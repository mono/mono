//
// System.ComponentModel.Design.ComponentDesigner
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//
// An implementation should be derived from the description here:
// "Writing Custom Designers for .NET Components"
//
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dndotnet/html/custdsgnrdotnet.asp
//
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

using System.Collections;

namespace System.ComponentModel.Design
{
	public class ComponentDesigner : IDesigner, IDisposable, IDesignerFilter
	{
		protected sealed class ShadowPropertyCollection
		{
			Hashtable collection;
			
			public object this[string propertyName] {
				get {
					if (collection == null)
						return null;
					
					return collection [propertyName];
				}

				set {
					if (collection == null)
						collection = new Hashtable ();
					
					collection [propertyName] = value;
				}
			}

			public bool Contains (string propertyName)
			{
				if (collection == null)
					return false;

				return collection.Contains (propertyName);
			}
		}

		IComponent component;
		ShadowPropertyCollection shadow_property_collection;
		DesignerVerbCollection verbs;
		
		public ComponentDesigner ()
		{
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

		#endregion Implementation of IDesignerFilter

		public virtual ICollection AssociatedComponents
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public IComponent Component
		{
			get {
				return component;
			}
		}

		public virtual DesignerVerbCollection Verbs
		{
			[MonoTODO]
			get {
				if (verbs == null) 
					verbs = new DesignerVerbCollection();

				return verbs;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		[MonoTODO("Not implemented, currently does nothing")]
		public virtual void DoDefaultAction ()
		{
			
		}

		public virtual void Initialize (IComponent component)
		{
			this.component = component;
		}

		[MonoTODO]
		public virtual void InitializeNonDefault ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnSetComponentDefaults ()
		{
			throw new NotImplementedException ();
		}


		protected InheritanceAttribute InheritanceAttribute
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		protected bool Inherited
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		protected ShadowPropertyCollection ShadowProperties
		{
			get {
				if (shadow_property_collection == null)
					shadow_property_collection = new ShadowPropertyCollection ();
				return shadow_property_collection;
			}
		}

		[MonoTODO("No designers services are provided in Mono")]
		protected virtual object GetService (Type serviceType)
		{
			return null;
		}

		[MonoTODO]
		protected InheritanceAttribute InvokeGetInheritanceAttribute (ComponentDesigner toInvoke)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PostFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PostFilterEvents (IDictionary events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PostFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PreFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PreFilterEvents (IDictionary events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Currently no event is raised")]
		protected void RaiseComponentChanged (MemberDescriptor member, object oldValue, object newValue)
		{
			// FIXME: Should notify the IComponentChangeService
			// that this component has changed
		}

		[MonoTODO("Currently no event is raised")]
		protected void RaiseComponentChanging (MemberDescriptor member)
		{
			
		}

		~ComponentDesigner ()
		{
		}
	}
}
