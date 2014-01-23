//
// System.Runtime.Remoting.Messaging.ConstructionCall.cs
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
using System.Threading;
using System.Collections;
using System.Runtime.Remoting.Activation;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable] [CLSCompliant (false)]
	[System.Runtime.InteropServices.ComVisible (true)]
	public class ConstructionCall: MethodCall, IConstructionCallMessage
	{
		IActivator _activator;
		object[] _activationAttributes;
		IList _contextProperties;
		Type _activationType;
		string _activationTypeName;
		bool _isContextOk;
		[NonSerialized] RemotingProxy _sourceProxy;

		public ConstructionCall (IMessage m): base (m)
		{
			_activationTypeName = TypeName;
			_isContextOk = true;
		}

		internal ConstructionCall (Type type)
		{
			_activationType = type;
			_activationTypeName = type.AssemblyQualifiedName;
			_isContextOk = true;
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

		internal bool IsContextOk
		{
			get { return _isContextOk; }
			set { _isContextOk = value; }
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

		internal void SetActivationAttributes (object [] attributes)
		{
			_activationAttributes = attributes;
		}

		public IList ContextProperties 
		{
			get 
			{
				if (_contextProperties == null) _contextProperties = new ArrayList ();
				return _contextProperties; 
			}
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

			IList props = _contextProperties;
			if (props != null && props.Count == 0) props = null;

			info.AddValue ("__Activator", _activator);
			info.AddValue ("__CallSiteActivationAttributes", _activationAttributes);
			info.AddValue ("__ActivationType", null);
			info.AddValue ("__ContextProperties", props);
			info.AddValue ("__ActivationTypeName", _activationTypeName);
		} 
		
		public override IDictionary Properties 
		{
			get { return base.Properties; }
		}	

		internal RemotingProxy SourceProxy
		{
			get { return _sourceProxy; }
			set {_sourceProxy = value; }
		}
	}
}
