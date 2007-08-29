//
// System.ComponentModel.Design.DesignModeSite
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

//
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

	internal class DesignModeNestedContainer : NestedContainer
	{

		// DesignModeNestedContainer is a NestedContainer where:
		//  * Site's Name property is a qualified name that includes the owning component's name 
		// followed by a period (.) and the child component's name.
		//  * GetService is routed through the owner

		private class Site : DesignModeSite, INestedSite
		{

			public Site (IComponent component, string name, IContainer container, IServiceProvider serviceProvider) :
				base (component,  name,  container,  serviceProvider)
			{
			}

			// [owner].[container].[site]
			//
			public string FullName {
				get {
					if (this.Name == null)
						return null;

					string ownerName = ((DesignModeNestedContainer)this.Container).OwnerName;
					if (ownerName == null)
						return this.Name;

					return ownerName + "." + this.Name;
				}
			}
		}

		private string _containerName;

		public DesignModeNestedContainer (IComponent owner, string containerName) : base (owner)
		{
			_containerName = containerName;
		}

		public override void Add (IComponent component, string name)
		{
			if (this.Owner.Site != null) {
			   DesignerHost host = this.Owner.Site.GetService (typeof (IDesignerHost)) as DesignerHost;
			   if (host != null) {
				   host.AddPreProcess (component, name);
				   base.Add (component, name);
				   host.AddPostProcess (component, name);
			   }
			}
		}

		public override void Remove (IComponent component)
		{
			if (this.Owner.Site != null) {
			   DesignerHost host = this.Owner.Site.GetService (typeof (IDesignerHost)) as DesignerHost;
			   if (host != null) {
				   host.RemovePreProcess (component);
				   base.Remove (component);
				   host.RemovePostProcess (component);
			   }
			}
		}

		// [owner].[container]
		//
		protected override string OwnerName {
			get {
				if (_containerName != null)
					return base.OwnerName + "." + _containerName;
				else
					return base.OwnerName;
			}
		}

		protected override ISite CreateSite (IComponent component, string name)
		{
			if (component == null)
				throw new ArgumentNullException("component");

			if (Owner.Site == null)
				throw new InvalidOperationException ("Owner not sited.");

			return new DesignModeNestedContainer.Site (component, name, this, (IServiceProvider)Owner.Site);
		}
		
		protected override object GetService (Type service)
		{
			if (service == typeof (INestedContainer))
				return this;

			object serviceInstance = null;

			if (this.Owner.Site != null)
				serviceInstance = this.Owner.Site.GetService (service);

			if (serviceInstance == null)
				return base.GetService (service);

			return null;
		}
	}
}
#endif
