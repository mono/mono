//
// System.ComponentModel.Design.ComponentDesigner
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;

namespace System.ComponentModel.Design
{
	public class ComponentDesigner : IDesigner, IDisposable,
	                                 IDesignerFilter
	{
		protected sealed class ShadowPropertyCollection
		{
			public object this [string propertyName] {
				[MonoTODO]
				get { throw new NotImplementedException(); } 

				[MonoTODO]
				set { throw new NotImplementedException(); }
			}

			[MonoTODO]
			public bool Contains (string propertyName)
			{
				throw new NotImplementedException();
			}

			[MonoTODO]
			~ShadowPropertyCollection()
			{
			}
		}

		[MonoTODO]
		public ComponentDesigner()
		{
		}

		public virtual ICollection AssociatedComponents {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public IComponent Component {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public virtual DesignerVerbCollection Verbs {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public virtual void DoDefaultAction()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void Initialize (IComponent component)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void InitializeNonDefault()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void OnSetComponentDefaults()
		{
			throw new NotImplementedException();
		}


		protected InheritanceAttribute InheritanceAttribute {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		protected bool Inherited {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		protected ShadowPropertyCollection ShadowProperties {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		protected virtual object GetService (Type serviceType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected InheritanceAttribute InvokeGetInheritanceAttribute (
					       ComponentDesigner toInvoke)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public virtual void PostFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public virtual void PostFilterEvents (IDictionary events)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public virtual void PostFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public virtual void PreFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public virtual void PreFilterEvents (IDictionary events)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		// LAMESPEC: Spec says protected but cannot compile
		public virtual void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected void RaiseComponentChanged (MemberDescriptor member, 
						      object oldValue,
						      object newValue)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected void RaiseComponentChanging (MemberDescriptor member)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ComponentDesigner()
		{
		}
		
	}
}
