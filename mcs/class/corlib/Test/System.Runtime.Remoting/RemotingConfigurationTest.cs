// created on 05/03/2003 at 13:18
// RemotingConfigurationTest.cs unit test class
// for System.Runtime.Remoting.RemotingConfiguration
//
// Author: Jean-Marc ANDRE <jean-marc.andre@polymtl.ca>
// 

using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using NUnit.Framework;

// internal namespace with classes I'll use during the unit tests
namespace MonoTests.System.Runtime.RemotingConfigurationInternal
{
	public class MarshalObject: MarshalByRefObject
	{
		private bool _method1Called = false;
		
		public bool Method1Called
		{
			get{ return _method1Called;}
		}
		
		public void Method1()
		{
			_method1Called = true;
		}
		
	}
	
	public class DerivedMarshalObject: MarshalObject
	{
		
	}
	
	public class WellKnownObject: MarshalByRefObject
	{
		private bool _method1Called = false;
		
		public bool Method1Called
		{
			get{ return _method1Called;}
		}
		
		public void Method1()
		{
			_method1Called = true;
		}		
	}
	
	public class DerivedWellKnownObject: WellKnownObject
	{
		
	}
	
	public class ActivatedObject: MarshalByRefObject
	{
		
	}
	
	public class DerivedActivatedObject: ActivatedObject
	{
		
	}
	
	public class AppNameTest: MarshalByRefObject
	{
		
	}
}

namespace MonoTests.System.Runtime.Remoting
{
	using MonoTests.System.Runtime.RemotingConfigurationInternal;
	
	// The unit test class
	[TestFixture]
	public class RemotingConfigurationTest
	{
		
		[Test]
		public void ApplicationId()
		{
			string _Id = RemotingConfiguration.ApplicationId;
			
			Assertion.Assert("#A01", _Id != null);
		}
		
		// tests and set the ApplicationName
		[Test]
		public void ApplicationName()
		{
			TcpChannel chn = null;
			AppNameTest objAppNameTest = null;
			try
			{
				chn = new TcpChannel(1234);
				ChannelServices.RegisterChannel(chn);
				
				// the URL of the application's marshaled objects should be 
				// tcp://localhost:1234/RemotingConfigurationTest/<object>
				RemotingConfiguration.ApplicationName = "RemotingConfigurationTest";
				
				objAppNameTest = new AppNameTest();
				RemotingServices.Marshal(objAppNameTest, "AppNameTest.rem");
				
				AppNameTest remAppNameTest = (AppNameTest) Activator.GetObject(typeof(AppNameTest), "tcp://localhost:1234/" + RemotingConfiguration.ApplicationName + "AppNameTest.rem");
				
				Assertion.Assert("#B01", remAppNameTest != null);
			}
			catch(Exception e)
			{
				Assertion.Assert("#B02: " + e.Message, false);
			}
			finally
			{
				RemotingServices.Disconnect(objAppNameTest);
				ChannelServices.UnregisterChannel(chn);
			}
		}
		
		// tests related to the SAO
		[Test]
		public void RegisterWellKnownType()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1234);
				ChannelServices.RegisterChannel(chn);
				
				// register the SAO
				if(RemotingConfiguration.ApplicationName == null) RemotingConfiguration.ApplicationName = "RemotingConfigurationTest";
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(DerivedWellKnownObject), "WellKnownObject.rem", WellKnownObjectMode.Singleton);
				
				// get the registered services
				WellKnownServiceTypeEntry[] ast = RemotingConfiguration.GetRegisteredWellKnownServiceTypes();
				
				bool IsServerRegistered = false;
				foreach(WellKnownServiceTypeEntry aste in ast)
				{
					if(aste.ObjectType == typeof(DerivedWellKnownObject))
					{
						IsServerRegistered = true;
						break;
					}
				}
				
				Assertion.Assert("#A02", IsServerRegistered);
				
				// register the client
				RemotingConfiguration.RegisterWellKnownClientType(typeof(WellKnownObject), "tcp://localhost:1234/"+RemotingConfiguration.ApplicationName+"/WellKnownObject.rem");
				
				// get the registered client
				WellKnownClientTypeEntry[] act = RemotingConfiguration.GetRegisteredWellKnownClientTypes();
				
				bool IsClientRegistered = false;
				foreach(WellKnownClientTypeEntry acte in act)
				{
					if(acte.ObjectType == typeof(WellKnownObject))
					{
						IsClientRegistered = true;
						break;
					}
				}
				
				Assertion.Assert("#A03", IsClientRegistered);
				
				WellKnownObject objWellKnown = new WellKnownObject();
				
				
				Assertion.Assert("#A04", objWellKnown != null);
				Assertion.Assert("#A05", RemotingServices.IsTransparentProxy(objWellKnown));
				objWellKnown.Method1();
				Assertion.Assert("#A06", objWellKnown.Method1Called);
			}
			finally
			{
				ChannelServices.UnregisterChannel(chn);
			}
			
		}
		
		// tests the CAO related methods
		[Test]
		[ExpectedException(typeof(RemotingException))]
		public void RegisterActivatedType()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1234);
				ChannelServices.RegisterChannel(chn);
				
				// register the CAO
				RemotingConfiguration.RegisterActivatedServiceType(typeof(ActivatedObject));
				
				// get the registered CAO
				ActivatedServiceTypeEntry[] ast = RemotingConfiguration.GetRegisteredActivatedServiceTypes();
				
				bool IsServerRegistered = false;
				foreach(ActivatedServiceTypeEntry aste in ast)
				{
					if(aste.ObjectType == typeof(ActivatedObject))
					{
						IsServerRegistered = true;
						break;
					}
				}
				
				Assertion.Assert("#A07", IsServerRegistered);
				
				RemotingConfiguration.RegisterActivatedClientType(typeof(DerivedActivatedObject), "tcp://localhost:1234");
				
				ActivatedClientTypeEntry[] act = RemotingConfiguration.GetRegisteredActivatedClientTypes();
				
				bool IsClientRegistered = false;
				foreach(ActivatedClientTypeEntry acte in act)
				{
					if(acte.ObjectType == typeof(DerivedActivatedObject))
					{
						IsClientRegistered = true;
						break;
					}
				}
				
				Assertion.Assert("#A08", IsClientRegistered);				
				
				// This will send a RemotingException since there is no service named DerivedActivatedObject
				// on the server
				DerivedActivatedObject objDerivedActivated = new DerivedActivatedObject();
			}
			finally
			{
				ChannelServices.UnregisterChannel(chn);
			}
			
		}
		
		// Get the process ID
		[Test]
		public void ProcessId()
		{
			string strProcessId = null;
			strProcessId = RemotingConfiguration.ProcessId;
			Assertion.Assert("#AO9", strProcessId != null);
		}
		
		[Test]
		public void IsActivationAllowed()
		{
			// ActivatedObject was previously registered as a CAO on the server
			// so IsActivationAllowed() should return TRUE
			Assertion.Assert("#A10", RemotingConfiguration.IsActivationAllowed(typeof(ActivatedObject)));
		}
		
		[Test]
		public void IsRemotelyActivatedClientType()
		{
			Assembly ass = Assembly.GetExecutingAssembly();
			AssemblyName assName = ass.GetName();
			
			ActivatedClientTypeEntry acte = null;
			// DerivedActivatedObject was registered as a CAO on the client
			acte = RemotingConfiguration.IsRemotelyActivatedClientType(typeof(DerivedActivatedObject));
			
			Assertion.Assert("#A11", acte != null);
			Assertion.AssertEquals("#A12", typeof(DerivedActivatedObject), acte.ObjectType);
			
			acte = RemotingConfiguration.IsRemotelyActivatedClientType(typeof(DerivedActivatedObject).ToString(), assName.Name);
			Assertion.Assert("#A13", acte != null);
			Assertion.AssertEquals("#A14", typeof(DerivedActivatedObject), acte.ObjectType);
		}
		
		[Test]
		public void IsWellKnownClientType()
		{
			Assembly ass = Assembly.GetExecutingAssembly();
			AssemblyName assName = ass.GetName();
			
			WellKnownClientTypeEntry acte = null;
			// WellKnownObject was registered as a SAO on th client
			acte = RemotingConfiguration.IsWellKnownClientType(typeof(WellKnownObject));
			
			Assertion.Assert("#A11", acte != null);
			Assertion.AssertEquals("#A12", typeof(WellKnownObject), acte.ObjectType);
			
			acte = RemotingConfiguration.IsWellKnownClientType(typeof(WellKnownObject).ToString(), assName.Name);
			Assertion.Assert("#A13", acte != null);
			Assertion.AssertEquals("#A14", typeof(WellKnownObject), acte.ObjectType);
			
		}
	}
}
