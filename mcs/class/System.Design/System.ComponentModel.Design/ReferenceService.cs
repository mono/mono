//
// System.ComponentModel.Design.ReferenceService
//
// Authors: 
//  Ivan N. Zlatev (contact i-nZ.net)
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;

namespace System.ComponentModel.Design
{

	internal class ReferenceService : IReferenceService, IDisposable
	{

		private List<IComponent> _references;

		internal ReferenceService (IServiceProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			_references = new List<IComponent>();
			IComponentChangeService serv = provider.GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (serv != null) {
				serv.ComponentAdded += OnComponentAdded;
				serv.ComponentRemoved += OnComponentRemoved;
			}
		}

		private void OnComponentAdded (object sender, ComponentEventArgs args)
		{
			_references.Add (args.Component);
		}

		private void OnComponentRemoved (object sender, ComponentEventArgs args)
		{
			_references.Remove (args.Component);
		}

		public IComponent GetComponent (object reference)
		{
			return reference as IComponent;
		}

		public string GetName (object reference)
		{
			IComponent comp = reference as IComponent;
			if (comp != null && comp.Site != null)
				return comp.Site.Name;
			return null;
		}

		public object GetReference (string name)
		{
			foreach (IComponent component in _references)
				if (component.Site != null && component.Site.Name == name)
					return component;
			return null;
		}

		public object[] GetReferences ()
		{
			IComponent[] references = new IComponent[_references.Count];
			_references.CopyTo (references);
			return references;
		}

		public object[] GetReferences (Type baseType)
		{
			List<IComponent> references = new List<IComponent>();

			foreach (IComponent component in _references)
				if (baseType.IsAssignableFrom ((component.GetType ())))
					references.Add (component);

			IComponent[] refArray = new IComponent[references.Count];
			references.CopyTo (refArray);
			return refArray;
		}

		public void Dispose ()
		{
			_references.Clear ();
		}
	}
}
#endif
