//
// System.Runtime.Remoting.Messaging.ConstructionCallDictionary.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Remoting.Activation;

namespace System.Runtime.Remoting.Messaging
{
	class ConstructionCallDictionary : MethodDictionary
	{
		public static string[] InternalKeys = new string[] {"__Uri", "__MethodName", "__TypeName", "__MethodSignature", "__Args", "__CallContext", "__CallSiteActivationAttributes", "__ActivationType", "__ContextProperties", "__Activator", "__ActivationTypeName"};

		public ConstructionCallDictionary(IConstructionCallMessage message) : base (message) 
		{ 
			MethodKeys = InternalKeys;
		}

		protected override object GetMethodProperty (string key)
		{
			switch (key)
			{
				case "__Activator" : return ((IConstructionCallMessage)_message).Activator;
				case "__CallSiteActivationAttributes" : return ((IConstructionCallMessage)_message).CallSiteActivationAttributes;
				case "__ActivationType" : return ((IConstructionCallMessage)_message).ActivationType;
				case "__ContextProperties" : return ((IConstructionCallMessage)_message).ContextProperties;
				case "__ActivationTypeName" : return ((IConstructionCallMessage)_message).ActivationTypeName;
				default : return base.GetMethodProperty (key);
			}
		}
			
		protected override void SetMethodProperty (string key, object value)
		{
			switch (key)
			{
				case "__Activator": ((IConstructionCallMessage)_message).Activator = (IActivator) value; break;

				case "__CallSiteActivationAttributes":
				case "__ActivationType": 
				case "__ContextProperties": 
				case "__ActivationTypeName": throw new ArgumentException ("key was invalid");

				default: base.SetMethodProperty (key, value); break;
			}
		}
	}
}
