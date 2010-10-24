//
// ServiceNameElementCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
#if NET_4_0 && CONFIGURATION_DEP

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.Security.Authentication.ExtendedProtection.Configuration
{
	[ConfigurationCollection (typeof (ServiceNameElement))]
	public sealed class ServiceNameElementCollection : ConfigurationElementCollection
	{
		public ServiceNameElement this [int index] {
			get { return (ServiceNameElement) BaseGet (index); }
		}

		public new ServiceNameElement this [string name] {
			get { return (ServiceNameElement) BaseGet (name); }
		}

		public void Add (ServiceNameElement element)
		{
			throw new NotImplementedException ();
		}

		public void Clear ()
		{
			throw new NotImplementedException ();
		}
		
		protected override ConfigurationElement CreateNewElement ()
		{
			return new ServiceNameElement ();
		}
		
		protected override object GetElementKey (ConfigurationElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return ((ServiceNameElement) element).Name;
		}
		
		public int IndexOf (ServiceNameElement element)
		{
			throw new NotImplementedException ();
		}
		
		public void Remove (string name)
		{
			throw new NotImplementedException ();
		}

		public void Remove (ServiceNameElement element)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
