//
// MonoTests.Remoting.ServerObject.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	// A list of ServerObject instances

	[ContextHook("x", false)]
	public class ServerList: 
		ContextBoundObject, 
		IDisposable
	{
		ArrayList values = new ArrayList();
		public int NumVal = 0;
		public string StrVal = "val";

		public ServerList()
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			CallSeq.Add ("List created");
		}

		public void Dispose()
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			CallSeq.Add ("List disposed");

		}

		public void Add (ServerObject v)
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			values.Add (v);
			CallSeq.Add ("Added " + v.Name);
		}

		public void ProcessItems ()
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			CallSeq.Add ("Processing");

			int total = 0;
			foreach (ServerObject ob in values)
				total += ob.GetValue();

			CallSeq.Add ("Total: " + total);
		}

		public void Clear()
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			CallSeq.Add ("Clearing");
			values.Clear();
		}

		public void ParameterTest1 (int a, out string b)
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			b = "adeu " + a;
		}
		
		public void ParameterTest2 (int a, out int b)
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			b = a+1;
		}
		
		public ServerObject NewItem(string name)
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			ServerObject obj = new ServerObject(name);
			Add (obj);
			return obj;
		}

		public ServerObject CreateItem(string name, int val)
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			ServerObject obj = new ServerObject(name);
			obj.SetValue (val);
			return obj;
		}

		public ComplexData SetComplexData (ComplexData data)
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			CallSeq.Add ("Showing content of ComplexData");
			data.Dump ();
			return data;
		}

		public override ObjRef CreateObjRef (Type type)
		{
			Assert.IsTrue (RemotingServices.IsTransparentProxy(this));
			CallSeq.Add ("### ServerList.CreateObjRef");
			return base.CreateObjRef (type);
		}
	}
		
	// A remotable object

	public class ServerObject: 
//		ContextBoundObject
		MarshalByRefObject
	{
		int _value;
		string _name;

		public ServerObject (string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}

		public void SetValue (int v)
		{
			CallSeq.Add ("ServerObject " + _name + ": setting " + v);
			_value = v;
		}

		public int GetValue ()
		{
			CallSeq.Add ("ServerObject " + _name + ": getting " + _value);
			return _value;
		}

		public override ObjRef CreateObjRef (Type type)
		{
			CallSeq.Add ("### ServerObject.CreateObjRef");
			return base.CreateObjRef (type);
		}
	}

	// Some complex data for testing serialization

	public enum AnEnum { a,b,c,d,e };

	[Serializable]
	public class ComplexData
	{
		public AnEnum Val = AnEnum.a;

		public object[] Info;

		public ComplexData (AnEnum va, object[] info)
		{
			Info = info;
			Val = va;
		}

		public void Dump ()
		{
			CallSeq.Add ("Content:");
			CallSeq.Add ("Val: " + Val);
			foreach (object ob in Info)
				CallSeq.Add ("Array item: " + ob);
		}
	}
}
