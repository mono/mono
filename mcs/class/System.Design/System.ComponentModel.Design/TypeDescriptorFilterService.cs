//
// System.ComponentModel.Design.TypeDescriptorFilterService
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

#if NET_2_0

using System;
using System.Collections;
using System.ComponentModel;

namespace System.ComponentModel.Design
{
	
	internal sealed class TypeDescriptorFilterService : ITypeDescriptorFilterService, IDisposable
	{

		IServiceProvider _serviceProvider;
		
		public TypeDescriptorFilterService (IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException ("serviceProvider");

			_serviceProvider = serviceProvider;
		}

		// Return values are:
		// true if the set of filtered attributes is to be cached; false if the filter service must query again.
		//	  
		public bool FilterAttributes (IComponent component, IDictionary attributes)
		{
			if (_serviceProvider == null)
				throw new ObjectDisposedException ("TypeDescriptorFilterService");
			if (component == null)
				throw new ArgumentNullException ("component");
			
			IDesignerHost designerHost = _serviceProvider.GetService (typeof (IDesignerHost)) as IDesignerHost;
			if (designerHost != null) {
				IDesigner designer = designerHost.GetDesigner (component);
				if (designer is IDesignerFilter) {
					((IDesignerFilter) designer).PreFilterAttributes (attributes);
					((IDesignerFilter) designer).PostFilterAttributes (attributes);
				}
			}

			return true;
		}
	
		public bool FilterEvents (IComponent component, IDictionary events)
		{
			if (_serviceProvider == null)
				throw new ObjectDisposedException ("TypeDescriptorFilterService");
			if (component == null)
				throw new ArgumentNullException ("component");
			
			IDesignerHost designerHost = _serviceProvider.GetService (typeof (IDesignerHost)) as IDesignerHost;
			if (designerHost != null) {
				IDesigner designer = designerHost.GetDesigner (component);
				if (designer is IDesignerFilter) {
					((IDesignerFilter) designer).PreFilterEvents (events);
					((IDesignerFilter) designer).PostFilterEvents (events);
				}
			}
			
			return true;
		}
	
		public bool FilterProperties (IComponent component, IDictionary properties)
		{
			if (_serviceProvider == null)
				throw new ObjectDisposedException ("TypeDescriptorFilterService");
			if (component == null)
				throw new ArgumentNullException ("component");

			IDesignerHost designerHost = _serviceProvider.GetService (typeof (IDesignerHost)) as IDesignerHost;
			if (designerHost != null) {
				IDesigner designer = designerHost.GetDesigner (component);
				if (designer is IDesignerFilter) {
					((IDesignerFilter) designer).PreFilterProperties (properties);
					((IDesignerFilter) designer).PostFilterProperties (properties);
				}
			}
			
			return true;
		}

		public void Dispose ()
		{
			_serviceProvider = null;
		}

	}
}

#endif
