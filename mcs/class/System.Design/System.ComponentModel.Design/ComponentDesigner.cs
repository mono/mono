//
// System.ComponentModel.Design.ComponentDesigner
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
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
			public object this[string propertyName]
			{
				[MonoTODO]
				get { throw new NotImplementedException (); }

				[MonoTODO]
				set { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public bool Contains (string propertyName)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			~ShadowPropertyCollection ()
			{
			}
		}

		[MonoTODO]
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
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public virtual DesignerVerbCollection Verbs
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DoDefaultAction ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
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
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		protected void RaiseComponentChanged (MemberDescriptor member, object oldValue, object newValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void RaiseComponentChanging (MemberDescriptor member)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		~ComponentDesigner ()
		{
		}
	}
}
