//
// ServiceEndpointCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml;

namespace System.ServiceModel.Description
{
	public class ServiceEndpointCollection : Collection<ServiceEndpoint>
	{
		internal ServiceEndpointCollection ()
		{
		}

		public ServiceEndpoint Find (Type contractType)
		{
			foreach (ServiceEndpoint e in this)
				if (e.Contract.ContractType == contractType)
					return e;
			return null;
		}

		public ServiceEndpoint Find (Uri address)
		{
			foreach (ServiceEndpoint e in this)
				if (e.Address.Uri == address)
					return e;
			return null;
		}

		public ServiceEndpoint Find (XmlQualifiedName contractName)
		{
			foreach (ServiceEndpoint e in this)
				if (e.Contract.Name == contractName.Name &&
					e.Contract.Namespace == contractName.Namespace)
					return e;
			return null;
		}

		public ServiceEndpoint Find (XmlQualifiedName contractName,
			XmlQualifiedName bindingName)
		{
			foreach (ServiceEndpoint e in this)
				if (e.Contract.Name == contractName.Name &&
					e.Contract.Namespace == contractName.Namespace &&
					e.Binding.Name == bindingName.Name &&
					e.Binding.Namespace == bindingName.Namespace)
					return e;
			return null;
		}

		public ServiceEndpoint Find (Type contractType,
			XmlQualifiedName bindingName)
		{
			foreach (ServiceEndpoint e in this)
				if (e.Contract.ContractType == contractType &&
					e.Binding.Name == bindingName.Name &&
					e.Binding.Namespace == bindingName.Namespace)
					return e;
			return null;
		}

		public Collection<ServiceEndpoint> FindAll (Type contractType)
		{
			Collection<ServiceEndpoint> list =
				new Collection<ServiceEndpoint> ();
			foreach (ServiceEndpoint e in this)
				if (e.Contract.ContractType == contractType)
					list.Add (e);
			return list;
		}

		public Collection<ServiceEndpoint> FindAll (XmlQualifiedName contractName)
		{
			Collection<ServiceEndpoint> list =
				new Collection<ServiceEndpoint> ();
			foreach (ServiceEndpoint e in this)
				if (e.Contract.Name == contractName.Name &&
				    e.Contract.Namespace == contractName.Namespace)
					list.Add (e);
			return list;
		}

		protected override void InsertItem (int index, ServiceEndpoint item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			base.InsertItem (index, item);
		}

		protected override void SetItem (int index, ServiceEndpoint item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			base.SetItem (index, item);
		}
	}
}
