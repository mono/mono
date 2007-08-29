//
// System.ComponentModel.Design.DesignSurfaceServiceContainer
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nz.net)
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

#if NET_2_0

using System;
using System.Collections;
using System.ComponentModel;

namespace System.ComponentModel.Design
{
	
	// Implements a ServiceContainer, which allows specific sets of services
	// to be non-replacable for users of the IServiceContainer .
	// 
	internal sealed class DesignSurfaceServiceContainer : ServiceContainer
	{

		private Hashtable _nonRemoveableServices;
		
		public DesignSurfaceServiceContainer () : this (null)
		{
		}

		public DesignSurfaceServiceContainer (IServiceProvider parentProvider) : base (parentProvider)
		{
		}
		
		internal void AddNonReplaceableService (Type serviceType, object instance)
		{
			if (_nonRemoveableServices == null)
				_nonRemoveableServices = new Hashtable ();

			_nonRemoveableServices[serviceType] = serviceType;
			base.AddService (serviceType, instance);
		}

			
		internal void RemoveNonReplaceableService (Type serviceType, object instance)
		{
			if (_nonRemoveableServices != null) 
				_nonRemoveableServices.Remove (serviceType);
			base.RemoveService (serviceType);
		}
		
		public override void RemoveService (Type serviceType, bool promote)
		{
			if (serviceType != null && _nonRemoveableServices != null && _nonRemoveableServices.ContainsKey (serviceType))
				throw new InvalidOperationException ("Cannot remove non-replaceable service: " + serviceType.AssemblyQualifiedName);

			base.RemoveService (serviceType, promote);				
		}
	}
}
#endif
