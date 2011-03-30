//
// FaultContractInfo.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Collections.Generic;

namespace System.ServiceModel.Dispatcher
{
	public class FaultContractInfo
	{
		public FaultContractInfo (string action, Type detail)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (detail == null)
				throw new ArgumentNullException ("detail");
			Action = action;
			Detail = detail;
		}

#if MOONLIGHT
		// introduced for silverlight sdk compatibility
		internal FaultContractInfo (string action, Type detail, XmlName elementName, string ns, IList<Type> knownTypes)
		{
			throw new NotImplementedException ();
		}
#endif

		DataContractSerializer serializer;

		public string Action { get; private set; }

		public Type Detail { get; private set; }

		internal DataContractSerializer Serializer {
			get {
				if (serializer == null)
					serializer = new DataContractSerializer (Detail);
				return serializer;
			}
		}
	}
}
