//
// MonoTests.Remoting.ContextsTest.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Net;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class ContextsTest
	{
		TcpChannel ch;
		
		[TestFixtureSetUp]
		public void Run()
		{
			CallSeq.CommonDomainId = 1;
			Context.RegisterDynamicProperty (new DynProperty("global"), null, null);

			ch = new TcpChannel(0);
			ChannelServices.RegisterChannel (ch);
		}

		[TestFixtureTearDown]
		public void End ()
		{
			Context.UnregisterDynamicProperty ("global", null, null);
			if (ch != null)
				ChannelServices.UnregisterChannel (ch);
		}

		[Test]
		public void TestSameContext ()
		{
			CallSeq.Init("TestSameContext");
			CallSeq.Add (">> TestSameContext");
			CallSeq.Add (">> Creating instance");
			ServerList list = new ServerList();
			CallSeq.Add ("<< Creating instance");
			RunTestObject (list);
			CallSeq.Add ("<< TestSameContext");
			CallSeq.Check (Checks.seqSameContext,1);
		}

		[Test]
		public void TestNewContext ()
		{
			try
			{
			CallSeq.Init("TestNewContext");
			CallSeq.Add (">> TestNewContext");
			object[] at = new object[] { new ContextHookAttribute ("1",true)};
			CallSeq.Add (">> Creating instance");
			ServerList list = (ServerList) Activator.CreateInstance (typeof (ServerList),null,at);
			CallSeq.Add ("<< Creating instance");
			RunTestObject (list);
			CallSeq.Add ("<< TestNewContext");
			CallSeq.Check (Checks.seqNewContext,1);
			}
			catch (Exception eX)
			{
				Console.WriteLine (eX);
			}
		}

		[Test]
		public void TestRemoteContext ()
		{
			AppDomain domain = AppDomain.CreateDomain ("test");
			DomainServer server = (DomainServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.Remoting.DomainServer");
			try
			{
				CallSeq.Init("TestRemoteContext");
				CallSeq.Add (">> TestRemoteContext");
				object[] at = new object[] { new ContextHookAttribute ("1",true), new UrlAttribute ("tcp://localhost:1122")};
				CallSeq.Add (">> Creating instance");
				ServerList list = (ServerList) Activator.CreateInstance (typeof (ServerList),null,at);
				CallSeq.Add ("<< Creating instance");
				RunTestObject (list);
				CallSeq.Add ("<< TestRemoteContext");
				CallSeq.Check (Checks.seqRemoteContext,1);

				CallSeq.Init ("TestRemoteContext Server");
				CallSeq.Seq = server.GetRemoteSeq ();
				CallSeq.Check (Checks.seqRemoteContext,2);
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
				throw ex;
			}
			finally
			{
				server.Stop ();
			}
//			AppDomain.Unload (domain);
		}

		void RunTestObject (ServerList list)
		{
			DynProperty prop1 = new DynProperty("defcontext");
			DynProperty prop2 = new DynProperty("proxy");

			try
			{
				Context.RegisterDynamicProperty (prop1, null, Context.DefaultContext);
				Context.RegisterDynamicProperty (prop2, list, null);

				CallSeq.Add(">> Clear");
				list.GetType().GetMethod ("Clear").Invoke (list, null);
				CallSeq.Add("<< Clear");

				CallSeq.Add(">> Set fields");
				list.NumVal = 4;
				list.StrVal = "hi";
				CallSeq.Add("<< Set fields");

				CallSeq.Add(">> Get fields");
				int nv = list.NumVal;
				string sv = list.StrVal;
				CallSeq.Add("<< Get fields");
				CallSeq.Add ("Get fields Result: " + nv + " / " + sv);

				CallSeq.Add(">> ParameterTest1");
				string b;
				list.ParameterTest1 (112, out b);
				CallSeq.Add("<< ParameterTest1");
				CallSeq.Add("ParameterTest1 Result: " + b);

				CallSeq.Add(">> ParameterTest2");
				int bn;
				list.ParameterTest2 (112, out bn);
				CallSeq.Add("<< ParameterTest2");
				CallSeq.Add("ParameterTest2 Result: " + bn);

				// These are remote calls that return references to remote objects

				CallSeq.Add (">> Creating two remote items");
				ServerObject item0 = list.CreateItem ("S0", 33);

				item0.SetValue (55);
				list.Add (item0);

				ServerObject item1 = list.NewItem ("S1");
				item1.SetValue (111);
				ServerObject item2 = list.NewItem ("S2");
				item2.SetValue (222);
				CallSeq.Add ("<< Creating two remote items");

				// Two objects created in this client app

				CallSeq.Add (">> Creating two client items");
				ServerObject item3 = new ServerObject ("C1");
				item3.SetValue (333);
				ServerObject item4 = new ServerObject ("C2");
				item4.SetValue (444);
				CallSeq.Add ("<< Creating two client items");

				// Object references passed to the remote list

				CallSeq.Add (">> Adding items");
				list.Add (item3);
				list.Add (item4);
				CallSeq.Add ("<< Adding items");

				// This sums all values of the ServerObjects in the list. The server
				// makes a remote call to this client to get the value of the
				// objects created locally

				CallSeq.Add (">> Processing items");
				list.ProcessItems ();
				CallSeq.Add ("<< Processing items");
			}
			catch (Exception ex)
			{
				Console.WriteLine ("ERR:" + ex.ToString());
				throw;
			}
			
			Context.UnregisterDynamicProperty ("defcontext", null, Context.DefaultContext);
			Context.UnregisterDynamicProperty ("proxy", list, null);
		}
	}

	class DomainServer: MarshalByRefObject
	{
		TcpChannel ch;
		
		public DomainServer()
		{
			CallSeq.CommonDomainId = 2;
			try
			{
				ch = new TcpChannel(1122);
				ChannelServices.RegisterChannel (ch);

				RemotingConfiguration.RegisterActivatedServiceType (typeof (ServerList));
				RemotingConfiguration.RegisterActivatedServiceType (typeof (ServerObject));
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex.ToString());
				throw;
			}
		}

		public ArrayList GetRemoteSeq()
		{
			return CallSeq.Seq;
		}

		public void Stop ()
		{
			if (ch != null)
				ChannelServices.UnregisterChannel (ch);
		}
	}

	class Checks
	{
		public static string[] seqSameContext = 
			{
				"001 (d1,c0) >> TestSameContext",
				"002 (d1,c0) >> Creating instance",
				"003 (d1,c0) ContextHookAttribute(x.d1).IsContextOK",
				"004 (d1,c0) List created",
				"005 (d1,c0) << Creating instance",
				"006 (d1,c0) IContributeDynamicSink(defcontext).GetDynamicSink",
				"007 (d1,c0) IContributeDynamicSink(proxy).GetDynamicSink",
				"008 (d1,c0) >> Clear",
				"009 (d1,c0) Clearing",
				"010 (d1,c0) << Clear",
				"011 (d1,c0) >> Set fields",
				"012 (d1,c0) << Set fields",
				"013 (d1,c0) >> Get fields",
				"014 (d1,c0) << Get fields",
				"015 (d1,c0) Get fields Result: 4 / hi",
				"016 (d1,c0) >> ParameterTest1",
				"017 (d1,c0) << ParameterTest1",
				"018 (d1,c0) ParameterTest1 Result: adeu 112",
				"019 (d1,c0) >> ParameterTest2",
				"020 (d1,c0) << ParameterTest2",
				"021 (d1,c0) ParameterTest2 Result: 113",
				"022 (d1,c0) >> Creating two remote items",
				"023 (d1,c0) ServerObject S0: setting 33",
				"024 (d1,c0) ServerObject S0: setting 55",
				"025 (d1,c0) Added S0",
				"026 (d1,c0) Added S1",
				"027 (d1,c0) ServerObject S1: setting 111",
				"028 (d1,c0) Added S2",
				"029 (d1,c0) ServerObject S2: setting 222",
				"030 (d1,c0) << Creating two remote items",
				"031 (d1,c0) >> Creating two client items",
				"032 (d1,c0) ServerObject C1: setting 333",
				"033 (d1,c0) ServerObject C2: setting 444",
				"034 (d1,c0) << Creating two client items",
				"035 (d1,c0) >> Adding items",
				"036 (d1,c0) Added C1",
				"037 (d1,c0) Added C2",
				"038 (d1,c0) << Adding items",
				"039 (d1,c0) >> Processing items",
				"040 (d1,c0) Processing",
				"041 (d1,c0) ServerObject S0: getting 55",
				"042 (d1,c0) ServerObject S1: getting 111",
				"043 (d1,c0) ServerObject S2: getting 222",
				"044 (d1,c0) ServerObject C1: getting 333",
				"045 (d1,c0) ServerObject C2: getting 444",
				"046 (d1,c0) Total: 1165",
				"047 (d1,c0) << Processing items",
				"048 (d1,c0) << TestSameContext",
		};

		public static string[] seqNewContext =
			{
				"001 (d1,c0) >> TestNewContext",
				"002 (d1,c0) >> Creating instance",
				"003 (d1,c0) ContextHookAttribute(1.d1).IsContextOK",
				"004 (d1,c0) IContextAttribute(1.d1).GetPropertiesForNewContext",
				"005 (d1,c0) IContextAttribute(x.d1).GetPropertiesForNewContext",
				"006 (d1,c0) <-> global DynamicSink Start .ctor client:True",
				"007 (d1,c0) ContextHookAttribute(1.d1).Freeze",
				"008 (d1,c0) ContextHookAttribute(x.d1).Freeze",
				"009 (d1,c0) ContextHookAttribute(1.d1).IsNewContextOK",
				"010 (d1,c0) ContextHookAttribute(x.d1).IsNewContextOK",
				"011 (d1,c1) IContributeServerContextSink(x.d1).GetServerContextSink",
				"012 (d1,c1) IContributeServerContextSink(1.d1).GetServerContextSink",
				"013 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage .ctor",
				"014 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage .ctor",
				"015 (d1,c1) List created",
				"016 (d1,c1) IContributeEnvoySink(1.d1).GetEnvoySink",
				"017 (d1,c1) IContributeEnvoySink(x.d1).GetEnvoySink",
				"018 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage .ctor",
				"019 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage .ctor",
				"020 (d1,c0) <-> global DynamicSink Finish .ctor client:True",
				"021 (d1,c0) << Creating instance",
				"022 (d1,c0) IContributeDynamicSink(defcontext).GetDynamicSink",
				"023 (d1,c0) IContributeDynamicSink(proxy).GetDynamicSink",
				"024 (d1,c0) >> Clear",
				"025 (d1,c0) <-> proxy DynamicSink Start Clear client:True",
				"026 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Clear",
				"027 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Clear",
				"028 (d1,c0) <-> global DynamicSink Start Clear client:True",
				"029 (d1,c0) <-> defcontext DynamicSink Start Clear client:True",
				"030 (d1,c1) <-> global DynamicSink Start Clear client:False",
				"031 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage Clear",
				"032 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage Clear",
				"033 (d1,c1) IContributeObjectSink(x.d1).GetObjectSink",
				"034 (d1,c1) IContributeObjectSink(1.d1).GetObjectSink",
				"035 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage Clear",
				"036 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage Clear",
				"037 (d1,c1) Clearing",
				"038 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage Clear",
				"039 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage Clear",
				"040 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Clear",
				"041 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Clear",
				"042 (d1,c1) <-> global DynamicSink Finish Clear client:False",
				"043 (d1,c0) <-> global DynamicSink Finish Clear client:True",
				"044 (d1,c0) <-> defcontext DynamicSink Finish Clear client:True",
				"045 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Clear",
				"046 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Clear",
				"047 (d1,c0) <-> proxy DynamicSink Finish Clear client:True",
				"048 (d1,c0) << Clear",
				"049 (d1,c0) >> Set fields",
				"050 (d1,c0) <-> proxy DynamicSink Start FieldSetter client:True",
				"051 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldSetter",
				"052 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"053 (d1,c0) <-> global DynamicSink Start FieldSetter client:True",
				"054 (d1,c0) <-> defcontext DynamicSink Start FieldSetter client:True",
				"055 (d1,c1) <-> global DynamicSink Start FieldSetter client:False",
				"056 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"057 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"058 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"059 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"060 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"061 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"062 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"063 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"064 (d1,c1) <-> global DynamicSink Finish FieldSetter client:False",
				"065 (d1,c0) <-> global DynamicSink Finish FieldSetter client:True",
				"066 (d1,c0) <-> defcontext DynamicSink Finish FieldSetter client:True",
				"067 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"068 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldSetter",
				"069 (d1,c0) <-> proxy DynamicSink Finish FieldSetter client:True",
				"070 (d1,c0) <-> proxy DynamicSink Start FieldSetter client:True",
				"071 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldSetter",
				"072 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"073 (d1,c0) <-> global DynamicSink Start FieldSetter client:True",
				"074 (d1,c0) <-> defcontext DynamicSink Start FieldSetter client:True",
				"075 (d1,c1) <-> global DynamicSink Start FieldSetter client:False",
				"076 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"077 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"078 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"079 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"080 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"081 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"082 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"083 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"084 (d1,c1) <-> global DynamicSink Finish FieldSetter client:False",
				"085 (d1,c0) <-> global DynamicSink Finish FieldSetter client:True",
				"086 (d1,c0) <-> defcontext DynamicSink Finish FieldSetter client:True",
				"087 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"088 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldSetter",
				"089 (d1,c0) <-> proxy DynamicSink Finish FieldSetter client:True",
				"090 (d1,c0) << Set fields",
				"091 (d1,c0) >> Get fields",
				"092 (d1,c0) <-> proxy DynamicSink Start FieldGetter client:True",
				"093 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldGetter",

				"094 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"095 (d1,c0) <-> global DynamicSink Start FieldGetter client:True",
				"096 (d1,c0) <-> defcontext DynamicSink Start FieldGetter client:True",
				"097 (d1,c1) <-> global DynamicSink Start FieldGetter client:False",
				"098 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"099 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"100 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"101 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldGetter",
				"102 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldGetter",
				"103 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"104 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"105 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"106 (d1,c1) <-> global DynamicSink Finish FieldGetter client:False",
				"107 (d1,c0) <-> global DynamicSink Finish FieldGetter client:True",
				"108 (d1,c0) <-> defcontext DynamicSink Finish FieldGetter client:True",
				"109 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"110 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldGetter",
				"111 (d1,c0) <-> proxy DynamicSink Finish FieldGetter client:True",
				"112 (d1,c0) <-> proxy DynamicSink Start FieldGetter client:True",
				"113 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldGetter",
				"114 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"115 (d1,c0) <-> global DynamicSink Start FieldGetter client:True",
				"116 (d1,c0) <-> defcontext DynamicSink Start FieldGetter client:True",
				"117 (d1,c1) <-> global DynamicSink Start FieldGetter client:False",
				"118 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"119 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"120 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"121 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldGetter",

				"122 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldGetter",
				"123 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"124 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"125 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"126 (d1,c1) <-> global DynamicSink Finish FieldGetter client:False",
				"127 (d1,c0) <-> global DynamicSink Finish FieldGetter client:True",
				"128 (d1,c0) <-> defcontext DynamicSink Finish FieldGetter client:True",
				"129 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"130 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldGetter",
				"131 (d1,c0) <-> proxy DynamicSink Finish FieldGetter client:True",
				"132 (d1,c0) << Get fields",
				"133 (d1,c0) Get fields Result: 4 / hi",
				"134 (d1,c0) >> ParameterTest1",
				"135 (d1,c0) <-> proxy DynamicSink Start ParameterTest1 client:True",
				"136 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage ParameterTest1",
				"137 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage ParameterTest1",
				"138 (d1,c0) <-> global DynamicSink Start ParameterTest1 client:True",
				"139 (d1,c0) <-> defcontext DynamicSink Start ParameterTest1 client:True",
				"140 (d1,c1) <-> global DynamicSink Start ParameterTest1 client:False",
				"141 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage ParameterTest1",
				"142 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage ParameterTest1",
				"143 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage ParameterTest1",
				"144 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage ParameterTest1",
				"145 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage ParameterTest1",
				"146 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage ParameterTest1",
				"147 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage ParameterTest1",
				"148 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage ParameterTest1",
				"149 (d1,c1) <-> global DynamicSink Finish ParameterTest1 client:False",
				"150 (d1,c0) <-> global DynamicSink Finish ParameterTest1 client:True",
				"151 (d1,c0) <-> defcontext DynamicSink Finish ParameterTest1 client:True",
				"152 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage ParameterTest1",
				"153 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage ParameterTest1",
				"154 (d1,c0) <-> proxy DynamicSink Finish ParameterTest1 client:True",
				"155 (d1,c0) << ParameterTest1",
				"156 (d1,c0) ParameterTest1 Result: adeu 112",
				"157 (d1,c0) >> ParameterTest2",
				"158 (d1,c0) <-> proxy DynamicSink Start ParameterTest2 client:True",
				"159 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage ParameterTest2",
				"160 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage ParameterTest2",
				"161 (d1,c0) <-> global DynamicSink Start ParameterTest2 client:True",
				"162 (d1,c0) <-> defcontext DynamicSink Start ParameterTest2 client:True",
				"163 (d1,c1) <-> global DynamicSink Start ParameterTest2 client:False",
				"164 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage ParameterTest2",

				"165 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage ParameterTest2",
				"166 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage ParameterTest2",
				"167 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage ParameterTest2",
				"168 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage ParameterTest2",
				"169 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage ParameterTest2",
				"170 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage ParameterTest2",
				"171 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage ParameterTest2",
				"172 (d1,c1) <-> global DynamicSink Finish ParameterTest2 client:False",
				"173 (d1,c0) <-> global DynamicSink Finish ParameterTest2 client:True",
				"174 (d1,c0) <-> defcontext DynamicSink Finish ParameterTest2 client:True",
				"175 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage ParameterTest2",
				"176 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage ParameterTest2",
				"177 (d1,c0) <-> proxy DynamicSink Finish ParameterTest2 client:True",
				"178 (d1,c0) << ParameterTest2",
				"179 (d1,c0) ParameterTest2 Result: 113",
				"180 (d1,c0) >> Creating two remote items",
				"181 (d1,c0) <-> proxy DynamicSink Start CreateItem client:True",
				"182 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage CreateItem",
				"183 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage CreateItem",
				"184 (d1,c0) <-> global DynamicSink Start CreateItem client:True",
				"185 (d1,c0) <-> defcontext DynamicSink Start CreateItem client:True",
				"186 (d1,c1) <-> global DynamicSink Start CreateItem client:False",
				"187 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage CreateItem",
				"188 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage CreateItem",
				"189 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage CreateItem",
				"190 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage CreateItem",
				"191 (d1,c1) ServerObject S0: setting 33",
				"192 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage CreateItem",
				"193 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage CreateItem",
				"194 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage CreateItem",
				"195 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage CreateItem",
				"196 (d1,c1) <-> global DynamicSink Finish CreateItem client:False",
				"197 (d1,c0) <-> global DynamicSink Finish CreateItem client:True",
				"198 (d1,c0) <-> defcontext DynamicSink Finish CreateItem client:True",
				"199 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage CreateItem",
				"200 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage CreateItem",
				"201 (d1,c0) <-> proxy DynamicSink Finish CreateItem client:True",
				"202 (d1,c0) ServerObject S0: setting 55",
				"203 (d1,c0) <-> proxy DynamicSink Start Add client:True",
				"204 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Add",
				"205 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Add",
				"206 (d1,c0) <-> global DynamicSink Start Add client:True",
				"207 (d1,c0) <-> defcontext DynamicSink Start Add client:True",
				"208 (d1,c1) <-> global DynamicSink Start Add client:False",
				"209 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage Add",
				"210 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage Add",
				"211 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage Add",
				"212 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage Add",
				"213 (d1,c1) Added S0",
				"214 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage Add",
				"215 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage Add",
				"216 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Add",
				"217 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Add",
				"218 (d1,c1) <-> global DynamicSink Finish Add client:False",
				"219 (d1,c0) <-> global DynamicSink Finish Add client:True",
				"220 (d1,c0) <-> defcontext DynamicSink Finish Add client:True",
				"221 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Add",
				"222 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Add",
				"223 (d1,c0) <-> proxy DynamicSink Finish Add client:True",
				"224 (d1,c0) <-> proxy DynamicSink Start NewItem client:True",
				"225 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage NewItem",
				"226 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage NewItem",
				"227 (d1,c0) <-> global DynamicSink Start NewItem client:True",
				"228 (d1,c0) <-> defcontext DynamicSink Start NewItem client:True",
				"229 (d1,c1) <-> global DynamicSink Start NewItem client:False",
				"230 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"231 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"232 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage NewItem",
				"233 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage NewItem",
				"234 (d1,c1) Added S1",
				"235 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage NewItem",
				"236 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage NewItem",
				"237 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"238 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"239 (d1,c1) <-> global DynamicSink Finish NewItem client:False",
				"240 (d1,c0) <-> global DynamicSink Finish NewItem client:True",
				"241 (d1,c0) <-> defcontext DynamicSink Finish NewItem client:True",
				"242 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage NewItem",
				"243 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage NewItem",
				"244 (d1,c0) <-> proxy DynamicSink Finish NewItem client:True",
				"245 (d1,c0) ServerObject S1: setting 111",
				"246 (d1,c0) <-> proxy DynamicSink Start NewItem client:True",
				"247 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage NewItem",
				"248 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage NewItem",
				"249 (d1,c0) <-> global DynamicSink Start NewItem client:True",
				"250 (d1,c0) <-> defcontext DynamicSink Start NewItem client:True",
				"251 (d1,c1) <-> global DynamicSink Start NewItem client:False",
				"252 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"253 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"254 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage NewItem",
				"255 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage NewItem",
				"256 (d1,c1) Added S2",
				"257 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage NewItem",
				"258 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage NewItem",
				"259 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"260 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"261 (d1,c1) <-> global DynamicSink Finish NewItem client:False",
				"262 (d1,c0) <-> global DynamicSink Finish NewItem client:True",
				"263 (d1,c0) <-> defcontext DynamicSink Finish NewItem client:True",
				"264 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage NewItem",
				"265 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage NewItem",
				"266 (d1,c0) <-> proxy DynamicSink Finish NewItem client:True",
				"267 (d1,c0) ServerObject S2: setting 222",
				"268 (d1,c0) << Creating two remote items",
				"269 (d1,c0) >> Creating two client items",
				"270 (d1,c0) ServerObject C1: setting 333",
				"271 (d1,c0) ServerObject C2: setting 444",
				"272 (d1,c0) << Creating two client items",
				"273 (d1,c0) >> Adding items",
				"274 (d1,c0) <-> proxy DynamicSink Start Add client:True",
				"275 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Add",
				"276 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Add",
				"277 (d1,c0) <-> global DynamicSink Start Add client:True",
				"278 (d1,c0) <-> defcontext DynamicSink Start Add client:True",
				"279 (d1,c1) <-> global DynamicSink Start Add client:False",
				"280 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage Add",
				"281 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage Add",
				"282 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage Add",
				"283 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage Add",
				"284 (d1,c1) Added C1",
				"285 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage Add",
				"286 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage Add",
				"287 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Add",
				"288 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Add",
				"289 (d1,c1) <-> global DynamicSink Finish Add client:False",
				"290 (d1,c0) <-> global DynamicSink Finish Add client:True",
				"291 (d1,c0) <-> defcontext DynamicSink Finish Add client:True",
				"292 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Add",
				"293 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Add",
				"294 (d1,c0) <-> proxy DynamicSink Finish Add client:True",
				"295 (d1,c0) <-> proxy DynamicSink Start Add client:True",
				"296 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Add",
				"297 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Add",
				"298 (d1,c0) <-> global DynamicSink Start Add client:True",
				"299 (d1,c0) <-> defcontext DynamicSink Start Add client:True",
				"300 (d1,c1) <-> global DynamicSink Start Add client:False",
				"301 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage Add",
				"302 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage Add",

				"303 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage Add",
				"304 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage Add",
				"305 (d1,c1) Added C2",
				"306 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage Add",
				"307 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage Add",
				"308 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Add",
				"309 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Add",
				"310 (d1,c1) <-> global DynamicSink Finish Add client:False",
				"311 (d1,c0) <-> global DynamicSink Finish Add client:True",
				"312 (d1,c0) <-> defcontext DynamicSink Finish Add client:True",
				"313 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Add",
				"314 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Add",
				"315 (d1,c0) <-> proxy DynamicSink Finish Add client:True",
				"316 (d1,c0) << Adding items",
				"317 (d1,c0) >> Processing items",
				"318 (d1,c0) <-> proxy DynamicSink Start ProcessItems client:True",
				"319 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage ProcessItems",
				"320 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage ProcessItems",
				"321 (d1,c0) <-> global DynamicSink Start ProcessItems client:True",
				"322 (d1,c0) <-> defcontext DynamicSink Start ProcessItems client:True",
				"323 (d1,c1) <-> global DynamicSink Start ProcessItems client:False",
				"324 (d1,c1) --> ServerContextSink(1.d1) SyncProcessMessage ProcessItems",
				"325 (d1,c1) --> ServerContextSink(x.d1) SyncProcessMessage ProcessItems",
				"326 (d1,c1) --> ObjectSink(1.d1) SyncProcessMessage ProcessItems",
				"327 (d1,c1) --> ObjectSink(x.d1) SyncProcessMessage ProcessItems",
				"328 (d1,c1) Processing",
				"329 (d1,c1) ServerObject S0: getting 55",
				"330 (d1,c1) ServerObject S1: getting 111",
				"331 (d1,c1) ServerObject S2: getting 222",
				"332 (d1,c1) ServerObject C1: getting 333",
				"333 (d1,c1) ServerObject C2: getting 444",
				"334 (d1,c1) Total: 1165",
				"335 (d1,c1) <-- ObjectSink(x.d1) SyncProcessMessage ProcessItems",
				"336 (d1,c1) <-- ObjectSink(1.d1) SyncProcessMessage ProcessItems",
				"337 (d1,c1) <-- ServerContextSink(x.d1) SyncProcessMessage ProcessItems",
				"338 (d1,c1) <-- ServerContextSink(1.d1) SyncProcessMessage ProcessItems",
				"339 (d1,c1) <-> global DynamicSink Finish ProcessItems client:False",
				"340 (d1,c0) <-> global DynamicSink Finish ProcessItems client:True",
				"341 (d1,c0) <-> defcontext DynamicSink Finish ProcessItems client:True",

				"342 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage ProcessItems",
				"343 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage ProcessItems",
				"344 (d1,c0) <-> proxy DynamicSink Finish ProcessItems client:True",
				"345 (d1,c0) << Processing items",
				"346 (d1,c0) << TestNewContext",
		};

		public static string[] seqRemoteContext =
			{
				"001 (d1,c0) >> TestRemoteContext",
				"002 (d1,c0) >> Creating instance",
				"003 (d1,c0) IContextAttribute(1.d1).GetPropertiesForNewContext",
				"004 (d1,c0) IContextAttribute(x.d1).GetPropertiesForNewContext",
				"005 (d1,c0) <-> global DynamicSink Start .ctor client:True",
				"006 (d1,c0) <-> global DynamicSink Start Activate client:True",
				"001 (d2,c0) IContextAttribute(x.d2).GetPropertiesForNewContext",
				"002 (d2,c0) ContextHookAttribute(1.d1).Freeze",
				"003 (d2,c0) ContextHookAttribute(x.d1).Freeze",
				"004 (d2,c0) ContextHookAttribute(x.d2).Freeze",
				"005 (d2,c0) ContextHookAttribute(1.d1).IsNewContextOK",
				"006 (d2,c0) ContextHookAttribute(x.d1).IsNewContextOK",
				"007 (d2,c0) ContextHookAttribute(x.d2).IsNewContextOK",
				"008 (d2,c1) IContributeServerContextSink(x.d2).GetServerContextSink",
				"009 (d2,c1) IContributeServerContextSink(x.d1).GetServerContextSink",
				"010 (d2,c1) IContributeServerContextSink(1.d1).GetServerContextSink",
				"011 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage .ctor",
				"012 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage .ctor",
				"013 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage .ctor",	
				"014 (d2,c1) IContributeEnvoySink(1.d1).GetEnvoySink",
				"015 (d2,c1) IContributeEnvoySink(x.d1).GetEnvoySink",
				"016 (d2,c1) IContributeEnvoySink(x.d2).GetEnvoySink",
				"017 (d2,c1) List created",
				"018 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage .ctor",
				"019 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage .ctor",
				"020 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage .ctor",
				"#021 (d2,c0) IContributeObjectSink(x.d2).GetObjectSink",
				"#022 (d2,c0) IContributeObjectSink(x.d1).GetObjectSink",
				"#023 (d2,c0) IContributeObjectSink(1.d1).GetObjectSink",
				"024 (d2,c0) --> EnvoySink(x.d2) SyncProcessMessage InitializeLifetimeService",
				"025 (d2,c0) --> EnvoySink(x.d1) SyncProcessMessage InitializeLifetimeService",
				"026 (d2,c0) --> EnvoySink(1.d1) SyncProcessMessage InitializeLifetimeService",
				"027 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage InitializeLifetimeService",
				"028 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage InitializeLifetimeService",
				"029 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage InitializeLifetimeService",
				"030 (d2,c1) IContributeObjectSink(x.d2).GetObjectSink",
				"031 (d2,c1) IContributeObjectSink(x.d1).GetObjectSink",
				"032 (d2,c1) IContributeObjectSink(1.d1).GetObjectSink",
				"033 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage InitializeLifetimeService",
				"034 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage InitializeLifetimeService",
				"035 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage InitializeLifetimeService",
				"036 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage InitializeLifetimeService",
				"037 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage InitializeLifetimeService",
				"038 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage InitializeLifetimeService",
				"039 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage InitializeLifetimeService",
				"040 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage InitializeLifetimeService",
				"041 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage InitializeLifetimeService",
				"042 (d2,c0) <-- EnvoySink(1.d1) SyncProcessMessage InitializeLifetimeService",
				"043 (d2,c0) <-- EnvoySink(x.d1) SyncProcessMessage InitializeLifetimeService",
				"044 (d2,c0) <-- EnvoySink(x.d2) SyncProcessMessage InitializeLifetimeService",
				"007 (d1,c0) <-> global DynamicSink Finish Activate client:True",
				"008 (d1,c0) <-> global DynamicSink Finish .ctor client:True",
				"009 (d1,c0) << Creating instance",
				"010 (d1,c0) IContributeDynamicSink(defcontext).GetDynamicSink",
				"011 (d1,c0) IContributeDynamicSink(proxy).GetDynamicSink",
				"012 (d1,c0) >> Clear",
				"013 (d1,c0) <-> proxy DynamicSink Start Clear client:True",
				"014 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage Clear",
				"015 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Clear",
				"016 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Clear",
				"017 (d1,c0) <-> global DynamicSink Start Clear client:True",
				"018 (d1,c0) <-> defcontext DynamicSink Start Clear client:True",
				"045 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage Clear",
				"046 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage Clear",
				"047 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage Clear",
				"048 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage Clear",
				"049 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage Clear",
				"050 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage Clear",
				"051 (d2,c1) Clearing",
				"052 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage Clear",
				"053 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage Clear",
				"054 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage Clear",
				"055 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage Clear",
				"056 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Clear",
				"057 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Clear",
				"019 (d1,c0) <-> global DynamicSink Finish Clear client:True",
				"020 (d1,c0) <-> defcontext DynamicSink Finish Clear client:True",
				"021 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Clear",
				"022 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Clear",
				"023 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage Clear",
				"024 (d1,c0) <-> proxy DynamicSink Finish Clear client:True",
				"025 (d1,c0) << Clear",
				"026 (d1,c0) >> Set fields",
				"027 (d1,c0) <-> proxy DynamicSink Start FieldSetter client:True",
				"028 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage FieldSetter",
				"029 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldSetter",
				"030 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"031 (d1,c0) <-> global DynamicSink Start FieldSetter client:True",
				"032 (d1,c0) <-> defcontext DynamicSink Start FieldSetter client:True",
				"058 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"059 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"060 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage FieldSetter",
				"061 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"062 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"063 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage FieldSetter",
				"064 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage FieldSetter",
				"065 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"066 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"067 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage FieldSetter",
				"068 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"069 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"033 (d1,c0) <-> global DynamicSink Finish FieldSetter client:True",
				"034 (d1,c0) <-> defcontext DynamicSink Finish FieldSetter client:True",
				"035 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"036 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldSetter",

				"037 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage FieldSetter",

				"038 (d1,c0) <-> proxy DynamicSink Finish FieldSetter client:True",
				"039 (d1,c0) <-> proxy DynamicSink Start FieldSetter client:True",
				"040 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage FieldSetter",
				"041 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldSetter",
				"042 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"043 (d1,c0) <-> global DynamicSink Start FieldSetter client:True",
				"044 (d1,c0) <-> defcontext DynamicSink Start FieldSetter client:True",
				"070 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"071 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"072 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage FieldSetter",
				"073 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"074 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"075 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage FieldSetter",
				"076 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage FieldSetter",
				"077 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldSetter",
				"078 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldSetter",
				"079 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage FieldSetter",
				"080 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldSetter",
				"081 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldSetter",
				"045 (d1,c0) <-> global DynamicSink Finish FieldSetter client:True",
				"046 (d1,c0) <-> defcontext DynamicSink Finish FieldSetter client:True",
				"047 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldSetter",
				"048 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldSetter",
				"049 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage FieldSetter",
				"050 (d1,c0) <-> proxy DynamicSink Finish FieldSetter client:True",
				"051 (d1,c0) << Set fields",
				"052 (d1,c0) >> Get fields",
				"053 (d1,c0) <-> proxy DynamicSink Start FieldGetter client:True",
				"054 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage FieldGetter",
				"055 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldGetter",
				"056 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"057 (d1,c0) <-> global DynamicSink Start FieldGetter client:True",
				"058 (d1,c0) <-> defcontext DynamicSink Start FieldGetter client:True",
				"082 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"083 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"084 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage FieldGetter",
				"085 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"086 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldGetter",
				"087 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage FieldGetter",
				"088 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage FieldGetter",
				"089 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldGetter",
				"090 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"091 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage FieldGetter",
				"092 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"093 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"059 (d1,c0) <-> global DynamicSink Finish FieldGetter client:True",
				"060 (d1,c0) <-> defcontext DynamicSink Finish FieldGetter client:True",
				"061 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"062 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldGetter",
				"063 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage FieldGetter",
				"064 (d1,c0) <-> proxy DynamicSink Finish FieldGetter client:True",
				"065 (d1,c0) <-> proxy DynamicSink Start FieldGetter client:True",
				"066 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage FieldGetter",
				"067 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage FieldGetter",
				"068 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"069 (d1,c0) <-> global DynamicSink Start FieldGetter client:True",
				"070 (d1,c0) <-> defcontext DynamicSink Start FieldGetter client:True",
				"094 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"095 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"096 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage FieldGetter",
				"097 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"098 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage FieldGetter",
				"099 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage FieldGetter",
				"100 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage FieldGetter",
				"101 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage FieldGetter",
				"102 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage FieldGetter",
				"103 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage FieldGetter",
				"104 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage FieldGetter",
				"105 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage FieldGetter",
				"071 (d1,c0) <-> global DynamicSink Finish FieldGetter client:True",
				"072 (d1,c0) <-> defcontext DynamicSink Finish FieldGetter client:True",
				"073 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage FieldGetter",
				"074 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage FieldGetter",
				"075 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage FieldGetter",
				"076 (d1,c0) <-> proxy DynamicSink Finish FieldGetter client:True",
				"077 (d1,c0) << Get fields",
				"078 (d1,c0) Get fields Result: 4 / hi",
				"079 (d1,c0) >> ParameterTest1",
				"080 (d1,c0) <-> proxy DynamicSink Start ParameterTest1 client:True",
				"081 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage ParameterTest1",
				"082 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage ParameterTest1",
				"083 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage ParameterTest1",
				"084 (d1,c0) <-> global DynamicSink Start ParameterTest1 client:True",
				"085 (d1,c0) <-> defcontext DynamicSink Start ParameterTest1 client:True",
				"106 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage ParameterTest1",
				"107 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage ParameterTest1",
				"108 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage ParameterTest1",
				"109 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage ParameterTest1",
				"110 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage ParameterTest1",
				"111 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage ParameterTest1",
				"112 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage ParameterTest1",
				"113 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage ParameterTest1",
				"114 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage ParameterTest1",
				"115 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage ParameterTest1",
				"116 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage ParameterTest1",
				"117 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage ParameterTest1",
				"086 (d1,c0) <-> global DynamicSink Finish ParameterTest1 client:True",
				"087 (d1,c0) <-> defcontext DynamicSink Finish ParameterTest1 client:True",
				"088 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage ParameterTest1",
				"089 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage ParameterTest1",
				"090 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage ParameterTest1",
				"091 (d1,c0) <-> proxy DynamicSink Finish ParameterTest1 client:True",
				"092 (d1,c0) << ParameterTest1",
				"093 (d1,c0) ParameterTest1 Result: adeu 112",
				"094 (d1,c0) >> ParameterTest2",
				"095 (d1,c0) <-> proxy DynamicSink Start ParameterTest2 client:True",
				"096 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage ParameterTest2",
				"097 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage ParameterTest2",
				"098 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage ParameterTest2",
				"099 (d1,c0) <-> global DynamicSink Start ParameterTest2 client:True",
				"100 (d1,c0) <-> defcontext DynamicSink Start ParameterTest2 client:True",
				"118 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage ParameterTest2",
				"119 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage ParameterTest2",

				"120 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage ParameterTest2",
				"121 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage ParameterTest2",
				"122 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage ParameterTest2",
				"123 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage ParameterTest2",
				"124 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage ParameterTest2",
				"125 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage ParameterTest2",
				"126 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage ParameterTest2",
				"127 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage ParameterTest2",
				"128 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage ParameterTest2",
				"129 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage ParameterTest2",
				"101 (d1,c0) <-> global DynamicSink Finish ParameterTest2 client:True",
				"102 (d1,c0) <-> defcontext DynamicSink Finish ParameterTest2 client:True",
				"103 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage ParameterTest2",
				"104 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage ParameterTest2",
				"105 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage ParameterTest2",
				"106 (d1,c0) <-> proxy DynamicSink Finish ParameterTest2 client:True",
				"107 (d1,c0) << ParameterTest2",
				"108 (d1,c0) ParameterTest2 Result: 113",
				"109 (d1,c0) >> Creating two remote items",
				"110 (d1,c0) <-> proxy DynamicSink Start CreateItem client:True",
				"111 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage CreateItem",
				"112 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage CreateItem",
				"113 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage CreateItem",
				"114 (d1,c0) <-> global DynamicSink Start CreateItem client:True",
				"115 (d1,c0) <-> defcontext DynamicSink Start CreateItem client:True",
				"130 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage CreateItem",
				"131 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage CreateItem",
				"132 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage CreateItem",
				"133 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage CreateItem",
				"134 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage CreateItem",
				"135 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage CreateItem",
				"136 (d2,c1) ServerObject S0: setting 33",
				"137 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage CreateItem",
				"138 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage CreateItem",
				"139 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage CreateItem",
				"140 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage CreateItem",
				"141 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage CreateItem",
				"142 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage CreateItem",
				"143 (d2,c0) ### ServerObject.CreateObjRef",
				"116 (d1,c0) <-> global DynamicSink Finish CreateItem client:True",
				"117 (d1,c0) <-> defcontext DynamicSink Finish CreateItem client:True",
				"118 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage CreateItem",
				"119 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage CreateItem",
				"120 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage CreateItem",
				"121 (d1,c0) <-> proxy DynamicSink Finish CreateItem client:True",
				"122 (d1,c0) <-> global DynamicSink Start SetValue client:True",
				"123 (d1,c0) <-> defcontext DynamicSink Start SetValue client:True",
				"144 (d2,c0) ServerObject S0: setting 55",
				"124 (d1,c0) <-> global DynamicSink Finish SetValue client:True",
				"125 (d1,c0) <-> defcontext DynamicSink Finish SetValue client:True",
				"126 (d1,c0) <-> proxy DynamicSink Start Add client:True",
				"127 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage Add",
				"128 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Add",
				"129 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Add",
				"130 (d1,c0) <-> global DynamicSink Start Add client:True",
				"131 (d1,c0) <-> defcontext DynamicSink Start Add client:True",
				"145 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage Add",
				"146 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage Add",
				"147 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage Add",
				"148 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage Add",
				"149 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage Add",
				"150 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage Add",
				"151 (d2,c1) Added S0",
				"152 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage Add",
				"153 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage Add",
				"154 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage Add",
				"155 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage Add",
				"156 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Add",
				"157 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Add",
				"132 (d1,c0) <-> global DynamicSink Finish Add client:True",
				"133 (d1,c0) <-> defcontext DynamicSink Finish Add client:True",
				"134 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Add",
				"135 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Add",
				"136 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage Add",
				"137 (d1,c0) <-> proxy DynamicSink Finish Add client:True",
				"138 (d1,c0) <-> proxy DynamicSink Start NewItem client:True",
				"139 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage NewItem",
				"140 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage NewItem",
				"141 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage NewItem",
				"142 (d1,c0) <-> global DynamicSink Start NewItem client:True",
				"143 (d1,c0) <-> defcontext DynamicSink Start NewItem client:True",
				"158 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"159 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"160 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage NewItem",
				"161 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage NewItem",
				"162 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage NewItem",
				"163 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage NewItem",
				"164 (d2,c1) Added S1",
				"165 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage NewItem",
				"166 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage NewItem",
				"167 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage NewItem",
				"168 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage NewItem",
				"169 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"170 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"171 (d2,c0) ### ServerObject.CreateObjRef",
				"144 (d1,c0) <-> global DynamicSink Finish NewItem client:True",
				"145 (d1,c0) <-> defcontext DynamicSink Finish NewItem client:True",
				"146 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage NewItem",
				"147 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage NewItem",
				"148 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage NewItem",
				"149 (d1,c0) <-> proxy DynamicSink Finish NewItem client:True",
				"150 (d1,c0) <-> global DynamicSink Start SetValue client:True",
				"151 (d1,c0) <-> defcontext DynamicSink Start SetValue client:True",
				"172 (d2,c0) ServerObject S1: setting 111",
				"152 (d1,c0) <-> global DynamicSink Finish SetValue client:True",
				"153 (d1,c0) <-> defcontext DynamicSink Finish SetValue client:True",
				"154 (d1,c0) <-> proxy DynamicSink Start NewItem client:True",
				"155 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage NewItem",
				"156 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage NewItem",
				"157 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage NewItem",
				"158 (d1,c0) <-> global DynamicSink Start NewItem client:True",
				"159 (d1,c0) <-> defcontext DynamicSink Start NewItem client:True",
				"173 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"174 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"175 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage NewItem",
				"176 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage NewItem",
				"177 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage NewItem",
				"178 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage NewItem",
				"179 (d2,c1) Added S2",
				"180 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage NewItem",
				"181 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage NewItem",
				"182 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage NewItem",
				"183 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage NewItem",
				"184 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage NewItem",
				"185 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage NewItem",
				"186 (d2,c0) ### ServerObject.CreateObjRef",
				"160 (d1,c0) <-> global DynamicSink Finish NewItem client:True",
				"161 (d1,c0) <-> defcontext DynamicSink Finish NewItem client:True",
				"162 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage NewItem",
				"163 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage NewItem",
				"164 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage NewItem",
				"165 (d1,c0) <-> proxy DynamicSink Finish NewItem client:True",
				"166 (d1,c0) <-> global DynamicSink Start SetValue client:True",
				"167 (d1,c0) <-> defcontext DynamicSink Start SetValue client:True",
				"187 (d2,c0) ServerObject S2: setting 222",
				"168 (d1,c0) <-> global DynamicSink Finish SetValue client:True",
				"169 (d1,c0) <-> defcontext DynamicSink Finish SetValue client:True",
				"170 (d1,c0) << Creating two remote items",
				"171 (d1,c0) >> Creating two client items",
				"172 (d1,c0) ServerObject C1: setting 333",
				"173 (d1,c0) ServerObject C2: setting 444",
				"174 (d1,c0) << Creating two client items",
				"175 (d1,c0) >> Adding items",
				"176 (d1,c0) <-> proxy DynamicSink Start Add client:True",
				"177 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage Add",
				"178 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Add",
				"179 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Add",
				"180 (d1,c0) <-> global DynamicSink Start Add client:True",
				"181 (d1,c0) <-> defcontext DynamicSink Start Add client:True",
				"182 (d1,c0) ### ServerObject.CreateObjRef",
				"188 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage Add",
				"189 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage Add",
				"190 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage Add",
				"191 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage Add",
				"192 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage Add",
				"193 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage Add",
				"194 (d2,c1) IContributeClientContextSink(1.d1).GetClientContextSink",
				"195 (d2,c1) IContributeClientContextSink(x.d1).GetClientContextSink",
				"196 (d2,c1) IContributeClientContextSink(x.d2).GetClientContextSink",
				"197 (d2,c1) --> ClientContextSink(x.d2) SyncProcessMessage get_Name",
				"198 (d2,c1) --> ClientContextSink(x.d1) SyncProcessMessage get_Name",
				"199 (d2,c1) --> ClientContextSink(1.d1) SyncProcessMessage get_Name",
				"183 (d1,c0) <-> global DynamicSink Start get_Name client:False",
				"184 (d1,c0) <-> defcontext DynamicSink Start get_Name client:False",
				"185 (d1,c0) <-> global DynamicSink Finish get_Name client:False",
				"186 (d1,c0) <-> defcontext DynamicSink Finish get_Name client:False",
				"200 (d2,c1) <-- ClientContextSink(1.d1) SyncProcessMessage get_Name",
				"201 (d2,c1) <-- ClientContextSink(x.d1) SyncProcessMessage get_Name",
				"202 (d2,c1) <-- ClientContextSink(x.d2) SyncProcessMessage get_Name",
				"203 (d2,c1) Added C1",
				"204 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage Add",
				"205 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage Add",
				"206 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage Add",
				"207 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage Add",
				"208 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Add",
				"209 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Add",
				"187 (d1,c0) <-> global DynamicSink Finish Add client:True",
				"188 (d1,c0) <-> defcontext DynamicSink Finish Add client:True",
				"189 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Add",
				"190 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Add",
				"191 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage Add",
				"192 (d1,c0) <-> proxy DynamicSink Finish Add client:True",
				"193 (d1,c0) <-> proxy DynamicSink Start Add client:True",
				"194 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage Add",
				"195 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage Add",
				"196 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage Add",
				"197 (d1,c0) <-> global DynamicSink Start Add client:True",
				"198 (d1,c0) <-> defcontext DynamicSink Start Add client:True",
				"199 (d1,c0) ### ServerObject.CreateObjRef",
				"210 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage Add",
				"211 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage Add",
				"212 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage Add",
				"213 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage Add",
				"214 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage Add",
				"215 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage Add",
				"216 (d2,c1) --> ClientContextSink(x.d2) SyncProcessMessage get_Name",
				"217 (d2,c1) --> ClientContextSink(x.d1) SyncProcessMessage get_Name",
				"218 (d2,c1) --> ClientContextSink(1.d1) SyncProcessMessage get_Name",
				"200 (d1,c0) <-> global DynamicSink Start get_Name client:False",
				"201 (d1,c0) <-> defcontext DynamicSink Start get_Name client:False",
				"202 (d1,c0) <-> global DynamicSink Finish get_Name client:False",
				"203 (d1,c0) <-> defcontext DynamicSink Finish get_Name client:False",
				"219 (d2,c1) <-- ClientContextSink(1.d1) SyncProcessMessage get_Name",
				"220 (d2,c1) <-- ClientContextSink(x.d1) SyncProcessMessage get_Name",
				"221 (d2,c1) <-- ClientContextSink(x.d2) SyncProcessMessage get_Name",
				"222 (d2,c1) Added C2",
				"223 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage Add",
				"224 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage Add",
				"225 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage Add",
				"226 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage Add",
				"227 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage Add",
				"228 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage Add",
				"204 (d1,c0) <-> global DynamicSink Finish Add client:True",
				"205 (d1,c0) <-> defcontext DynamicSink Finish Add client:True",
				"206 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage Add",
				"207 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage Add",
				"208 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage Add",
				"209 (d1,c0) <-> proxy DynamicSink Finish Add client:True",
				"210 (d1,c0) << Adding items",
				"211 (d1,c0) >> Processing items",
				"212 (d1,c0) <-> proxy DynamicSink Start ProcessItems client:True",
				"213 (d1,c0) --> EnvoySink(x.d2) SyncProcessMessage ProcessItems",
				"214 (d1,c0) --> EnvoySink(x.d1) SyncProcessMessage ProcessItems",
				"215 (d1,c0) --> EnvoySink(1.d1) SyncProcessMessage ProcessItems",
				"216 (d1,c0) <-> global DynamicSink Start ProcessItems client:True",
				"217 (d1,c0) <-> defcontext DynamicSink Start ProcessItems client:True",
				"229 (d2,c1) --> ServerContextSink(1.d1) SyncProcessMessage ProcessItems",
				"230 (d2,c1) --> ServerContextSink(x.d1) SyncProcessMessage ProcessItems",
				"231 (d2,c1) --> ServerContextSink(x.d2) SyncProcessMessage ProcessItems",
				"232 (d2,c1) --> ObjectSink(1.d1) SyncProcessMessage ProcessItems",
				"233 (d2,c1) --> ObjectSink(x.d1) SyncProcessMessage ProcessItems",
				"234 (d2,c1) --> ObjectSink(x.d2) SyncProcessMessage ProcessItems",
				"235 (d2,c1) Processing",
				"236 (d2,c1) ServerObject S0: getting 55",
				"237 (d2,c1) ServerObject S1: getting 111",
				"238 (d2,c1) ServerObject S2: getting 222",
				"239 (d2,c1) --> ClientContextSink(x.d2) SyncProcessMessage GetValue",
				"240 (d2,c1) --> ClientContextSink(x.d1) SyncProcessMessage GetValue",
				"241 (d2,c1) --> ClientContextSink(1.d1) SyncProcessMessage GetValue",
				"218 (d1,c0) <-> global DynamicSink Start GetValue client:False",
				"219 (d1,c0) <-> defcontext DynamicSink Start GetValue client:False",
				"220 (d1,c0) ServerObject C1: getting 333",
				"221 (d1,c0) <-> global DynamicSink Finish GetValue client:False",
				"222 (d1,c0) <-> defcontext DynamicSink Finish GetValue client:False",
				"242 (d2,c1) <-- ClientContextSink(1.d1) SyncProcessMessage GetValue",
				"243 (d2,c1) <-- ClientContextSink(x.d1) SyncProcessMessage GetValue",
				"244 (d2,c1) <-- ClientContextSink(x.d2) SyncProcessMessage GetValue",
				"245 (d2,c1) --> ClientContextSink(x.d2) SyncProcessMessage GetValue",
				"246 (d2,c1) --> ClientContextSink(x.d1) SyncProcessMessage GetValue",
				"247 (d2,c1) --> ClientContextSink(1.d1) SyncProcessMessage GetValue",
				"223 (d1,c0) <-> global DynamicSink Start GetValue client:False",
				"224 (d1,c0) <-> defcontext DynamicSink Start GetValue client:False",
				"225 (d1,c0) ServerObject C2: getting 444",
				"226 (d1,c0) <-> global DynamicSink Finish GetValue client:False",
				"227 (d1,c0) <-> defcontext DynamicSink Finish GetValue client:False",
				"248 (d2,c1) <-- ClientContextSink(1.d1) SyncProcessMessage GetValue",
				"249 (d2,c1) <-- ClientContextSink(x.d1) SyncProcessMessage GetValue",
				"250 (d2,c1) <-- ClientContextSink(x.d2) SyncProcessMessage GetValue",
				"251 (d2,c1) Total: 1165",
				"252 (d2,c1) <-- ObjectSink(x.d2) SyncProcessMessage ProcessItems",
				"253 (d2,c1) <-- ObjectSink(x.d1) SyncProcessMessage ProcessItems",
				"254 (d2,c1) <-- ObjectSink(1.d1) SyncProcessMessage ProcessItems",
				"255 (d2,c1) <-- ServerContextSink(x.d2) SyncProcessMessage ProcessItems",
				"256 (d2,c1) <-- ServerContextSink(x.d1) SyncProcessMessage ProcessItems",
				"257 (d2,c1) <-- ServerContextSink(1.d1) SyncProcessMessage ProcessItems",
				"228 (d1,c0) <-> global DynamicSink Finish ProcessItems client:True",
				"229 (d1,c0) <-> defcontext DynamicSink Finish ProcessItems client:True",
				"230 (d1,c0) <-- EnvoySink(1.d1) SyncProcessMessage ProcessItems",
				"231 (d1,c0) <-- EnvoySink(x.d1) SyncProcessMessage ProcessItems",
				"232 (d1,c0) <-- EnvoySink(x.d2) SyncProcessMessage ProcessItems",
				"233 (d1,c0) <-> proxy DynamicSink Finish ProcessItems client:True",
				"234 (d1,c0) << Processing items",
				"235 (d1,c0) << TestRemoteContext",
		};
	}
}
