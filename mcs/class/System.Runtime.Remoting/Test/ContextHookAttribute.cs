//
// MonoTests.Remoting.ContextHookAttribute.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting;
using System.Threading;

namespace MonoTests.Remoting
{
	[Serializable,AttributeUsage(AttributeTargets.Class)]
	public class ContextHookAttribute: Attribute, IContextAttribute, IContextProperty, IContributeObjectSink, IContributeServerContextSink, IContributeEnvoySink, IContributeClientContextSink
	{
		bool newContext = false;

		string id = "";
		public ContextHookAttribute()
		{
		}

		public ContextHookAttribute(string idp, bool newContext)
		{
			id = idp;
			if (id != "") id += ".";
			id += "d" + CallSeq.CommonDomainId;
			this.newContext = newContext;
		}

		public override object TypeId
		{

			get { return "ContextHook"; }
		}

		bool IContextAttribute.IsContextOK(Context ctx, IConstructionCallMessage ctor)
		{
			CallSeq.Add("ContextHookAttribute(" + id + ").IsContextOK");
			return !newContext;
		}

		public bool IsNewContextOK(Context ctx)
		{
			CallSeq.Add("ContextHookAttribute(" + id + ").IsNewContextOK");
			return true;
		}

		public void Freeze(Context ctx)
		{
			CallSeq.Add("ContextHookAttribute(" + id + ").Freeze");
		}

		public String Name
		{
			get { return "ContextHook(" + id + ")"; }
		}
	

		void IContextAttribute.GetPropertiesForNewContext(IConstructionCallMessage ctor)
		{
			CallSeq.Add("IContextAttribute(" + id + ").GetPropertiesForNewContext");
			ctor.ContextProperties.Add(this);
		}	

		IMessageSink IContributeObjectSink.GetObjectSink(MarshalByRefObject o, IMessageSink next)
		{
			CallSeq.Add("IContributeObjectSink(" + id + ").GetObjectSink");
			return new GenericMessageSink(o,next,"ObjectSink(" + id + ")");
		}
		
		IMessageSink IContributeServerContextSink.GetServerContextSink(IMessageSink next)
		{
			CallSeq.Add("IContributeServerContextSink(" + id + ").GetServerContextSink");
			return new GenericMessageSink(null,next,"ServerContextSink(" + id + ")");
		}

		IMessageSink IContributeEnvoySink.GetEnvoySink(MarshalByRefObject obj, IMessageSink nextSink)
		{
			CallSeq.Add("IContributeEnvoySink(" + id + ").GetEnvoySink");
			return new GenericMessageSink(obj,nextSink,"EnvoySink(" + id + ")");
		}

		IMessageSink IContributeClientContextSink.GetClientContextSink (IMessageSink nextSink )
		{
			CallSeq.Add("IContributeClientContextSink(" + id + ").GetClientContextSink");
			return new GenericMessageSink(null,nextSink,"ClientContextSink(" + id + ")");
		}
	}

	[Serializable]
	class GenericMessageSink: IMessageSink
	{
		IMessageSink _next;
		string _type;

		public GenericMessageSink(MarshalByRefObject obj, IMessageSink nextSink, string type)
		{
			_type = type;
			_next = nextSink;
		}

		public IMessageSink NextSink 
		{	
			get { return _next; }
		}

		public IMessage SyncProcessMessage(IMessage imCall)
		{
			CallSeq.Add("--> " + _type + " SyncProcessMessage " + imCall.Properties["__MethodName"]);
			IMessage ret = _next.SyncProcessMessage(imCall);
			CallSeq.Add("<-- " + _type + " SyncProcessMessage " + imCall.Properties["__MethodName"]);
			return ret;
		}

		public IMessageCtrl AsyncProcessMessage(IMessage im, IMessageSink ims)
		{
			CallSeq.Add("--> " + _type + " AsyncProcessMessage " + im.Properties["__MethodName"]);
			IMessageCtrl ret = _next.AsyncProcessMessage(im, ims);
			CallSeq.Add("<-- " + _type + " AsyncProcessMessage " + im.Properties["__MethodName"]);
			return ret;
		}
	}

	[Serializable]
	class GenericDynamicSink: IDynamicMessageSink
	{
		string _name;
		
		public GenericDynamicSink (string name)
		{
			_name = name;
		}

		void IDynamicMessageSink.ProcessMessageFinish(IMessage replyMsg, bool bCliSide, bool bAsync)
		{
			CallSeq.Add("<-> " + _name + " DynamicSink Finish " + replyMsg.Properties["__MethodName"] + " client:" + bCliSide);
		}

		void IDynamicMessageSink.ProcessMessageStart(IMessage replyMsg, bool bCliSide, bool bAsync)
		{
			CallSeq.Add("<-> " + _name + " DynamicSink Start " + replyMsg.Properties["__MethodName"] + " client:" + bCliSide);
		}
	}

	public class DynProperty: IDynamicProperty, IContributeDynamicSink
	{
		string _name;
		public DynProperty (string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}

		public IDynamicMessageSink GetDynamicSink()
		{
			CallSeq.Add("IContributeDynamicSink(" + _name + ").GetDynamicSink");
			return new GenericDynamicSink(_name);
		}
	}
}
