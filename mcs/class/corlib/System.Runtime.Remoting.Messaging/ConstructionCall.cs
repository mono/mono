//
// System.Runtime.Remoting.Messaging.ConstructionCall.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting.Activation;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable] [CLSCompliant (false)]
	public class ConstructionCall: MethodCall, IConstructionCallMessage
	{
		IActivator _activator;
		object[] _activationAttributes;
		IList _contextProperties;
		Type _activationType;
		string _activationTypeName;

		public ConstructionCall(IMessage msg): base (msg)
		{
			_activationTypeName = TypeName;

/*			object[] attribs = ActivationType.GetCustomAttributes (typeof (IContextProperty), true);

			if (attribs.Length > 0)
				_contextProperties = new ArrayList (attribs);
*/
			_activationAttributes = null;	// FIXME: put something here
		}

		public ConstructionCall (Header[] headers): base (headers)
		{
		}

		internal ConstructionCall (SerializationInfo info, StreamingContext context): base (info, context)
		{
		}

		internal override void InitDictionary()
		{
			ConstructionCallDictionary props = new ConstructionCallDictionary (this);
			ExternalProperties = props;
			InternalProperties = props.GetInternalProperties();
		}

		public Type ActivationType 
		{
			get 
			{ 
				if (_activationType == null) _activationType = Type.GetType (_activationTypeName);
				return _activationType; 
			}
		}

		public string ActivationTypeName 
		{
			get { return _activationTypeName; }
		}

		public IActivator Activator 
		{
			get { return _activator; }
			set { _activator = value; }
		}

		public object [] CallSiteActivationAttributes 
		{
			get { return _activationAttributes; }
		}

		public IList ContextProperties 
		{
			get { return _contextProperties; }
		}

		internal override void InitMethodProperty(string key, object value)
		{
			switch (key)
			{
				case "__Activator" : _activator = (IActivator) value; return;
				case "__CallSiteActivationAttributes" : _activationAttributes = (object[]) value; return;
				case "__ActivationType" : _activationType = (Type) value; return;
				case "__ContextProperties" : _contextProperties = (IList) value; return;
				case "__ActivationTypeName" : _activationTypeName = (string) value; return;
				default: base.InitMethodProperty (key, value); return;
			}
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);

			info.AddValue ("__Activator", _activator);
			info.AddValue ("__CallSiteActivationAttributes", _activationAttributes);
			info.AddValue ("__ActivationType", null);
			info.AddValue ("__ContextProperties", _contextProperties);
			info.AddValue ("__ActivationTypeName", _activationTypeName);
		} 
	}
}
