//
// System.Runtime.Remoting.Messaging.ConstructionCallDictionary.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
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
