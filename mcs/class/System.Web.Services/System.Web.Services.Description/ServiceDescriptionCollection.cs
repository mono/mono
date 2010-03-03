// 
// System.Web.Services.Description.ServiceDescriptionCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Description {
	public sealed class ServiceDescriptionCollection : ServiceDescriptionBaseCollection {

#if !TARGET_JVM //code generation is not supported
		ServiceDescriptionImporter importer;
#endif
		
		#region Constructors
	
		public ServiceDescriptionCollection () 
			: base (null)
		{
		}

		#endregion // Constructors

		#region Properties

		public ServiceDescription this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (ServiceDescription) List[index]; 
			}
			set { List [index] = value; }
		}

		public ServiceDescription this [string ns] {
			get { 
				return (ServiceDescription) Table[ns];
			}
		}

		#endregion // Properties

		#region Methods
#if !TARGET_JVM //code generation is not supported
		internal void SetImporter (ServiceDescriptionImporter i)
		{
			importer = i;
		}
#endif
		public int Add (ServiceDescription serviceDescription) 
		{
			Insert (Count, serviceDescription);
			return (Count - 1);
		}
		
		public bool Contains (ServiceDescription serviceDescription)
		{
			return List.Contains (serviceDescription);
		}

		public void CopyTo (ServiceDescription[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public Binding GetBinding (XmlQualifiedName name)
		{
			foreach (ServiceDescription desc in List) {
				if (desc.TargetNamespace == name.Namespace) {
					foreach (Binding binding in desc.Bindings) 
						if (binding.Name == name.Name)
							return binding;
				}
			}
			throw new InvalidOperationException ("Binding '" + name + "' not found");
		}

		protected override string GetKey (object value) 
		{
			return ((ServiceDescription) value).TargetNamespace;
		}

		public Message GetMessage (XmlQualifiedName name)
		{
			foreach (ServiceDescription desc in List) {
				if (desc.TargetNamespace == name.Namespace) {
					foreach (Message message in desc.Messages) 
						if (message.Name == name.Name)
							return message;
				}
			}
			throw new InvalidOperationException ("Message '" + name + "' not found");
		}

		public PortType GetPortType (XmlQualifiedName name)
		{
			foreach (ServiceDescription desc in List) {
				if (desc.TargetNamespace == name.Namespace) {
					foreach (PortType portType in desc.PortTypes) 
						if (portType.Name == name.Name)
							return portType;
				}
			}
			throw new InvalidOperationException ("Port type '" + name + "' not found");
		}

		public Service GetService (XmlQualifiedName name)
		{
			foreach (ServiceDescription desc in List) {
				if (desc.TargetNamespace == name.Namespace) {
					foreach (Service service in desc.Services) 
						if (service.Name == name.Name)
							return service;
				}
			}
			throw new InvalidOperationException ("Service '" + name + "' not found");
		}

		public int IndexOf (ServiceDescription serviceDescription)
		{
			return List.IndexOf (serviceDescription);
		}

		public void Insert (int index, ServiceDescription serviceDescription)
		{
			List.Insert (index, serviceDescription);
			OnInsertComplete (index, serviceDescription);
		}
	
		public void Remove (ServiceDescription serviceDescription)
		{
			List.Remove (serviceDescription);
		}

#if NET_2_0
		[MonoTODO]
		protected override
#endif
		void OnInsertComplete (int index, object item)
		{
			base.OnInsertComplete (index, item);
		}

#if NET_2_0
		[MonoTODO]
		protected override void SetParent (object value, object parent)
		{
		}
#endif

		#endregion // Methods
	}
}
