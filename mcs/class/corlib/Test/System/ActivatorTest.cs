using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Framework;

// The class in this namespace is used by the
// main test class
namespace MonoTests.System.ActivatorTestInternal
  {
  // We need a COM class to test the Activator class
  [ComVisible(true)]
    
  public class COMTest: MarshalByRefObject
    {
    public COMTest()
      {
      id = 0;
      }
    // This property is visible
    [ComVisible(true)]
      public int Id
      {
      get { return id; }
      set { id = value; }
      }
    
    public COMTest(int id)
      {
      this.id = id;
      }
    
    private int id;
    public bool constructorFlag = false;
    }
  } // MonoTests.System.ActivatorTestInternal namespace

namespace MonoTests.System
  {
  using MonoTests.System.ActivatorTestInternal;

  [TestFixture]
  public class ActivatorTest
    {
    public ActivatorTest()
      {}
    
    [Test]
      [Ignore("Activator.CreateComInstanceForm is not yet implemented")]
      // This test is ignored for the moment because 
      // CreateComInstanceFrom() is not implemented yet
      // by the mono Activator class
      public void CreateComInstanceFrom()
      {
      ObjectHandle objHandle = Activator.CreateComInstanceFrom(strAssembly ,
"COMTest");
      COMTest objCOMTest = (COMTest) objHandle.Unwrap();
      objCOMTest.Id = 10;
      Assertion.AssertEquals("#A01",10,objCOMTest.Id);
      }

    [Test]
      // This method tests CreateInstance()
      public void CreateInstance()
      {
      COMTest objCOMTest;
      // object CreateInstance(Type type)
      objCOMTest = (COMTest) Activator.CreateInstance(typeof(COMTest));
      Assertion.AssertEquals("#A02",
"MonoTests.System.ActivatorTestInternal.COMTest",
(objCOMTest.GetType()).ToString());
      // ObjectHandle CreateInstance(string, string) 
       ObjectHandle objHandle;
       objHandle = Activator.CreateInstance(null ,
"MonoTests.System.ActivatorTestInternal.COMTest");
       objCOMTest = (COMTest) objHandle.Unwrap();
       objCOMTest.Id = 2;
       Assertion.AssertEquals("#A03", 2, objCOMTest.Id);
      // object CreateInstance(Type, bool)
       objCOMTest = (COMTest) Activator.CreateInstance((typeof(COMTest)), false);
       Assertion.AssertEquals("#A04",
"MonoTests.System.ActivatorTestInternal.COMTest",
(objCOMTest.GetType()).ToString());
//       // object CreateInstance(Type, object[])
       object[] objArray = new object[1];
       objArray[0] = 7;
       objCOMTest = (COMTest) Activator.CreateInstance((typeof(COMTest)), objArray);
       Assertion.AssertEquals("#A05", 7, objCOMTest.Id);
       // Todo: Implemente the test methods for
       // all the overriden functions using activationAttribute
      }

    // This method tests GetObject from the Activator class
    [Test]
      public void GetObject()
      {
      // object GetObject(Type, string)
      
      // This will provide a COMTest object on  tcp://localhost:1234/COMTestUri
      COMTest objCOMTest = new COMTest(8);
      TcpChannel chnServer = new TcpChannel(1234);
      ChannelServices.RegisterChannel(chnServer);
      RemotingServices.SetObjectUriForMarshal(objCOMTest, "COMTestUri");
      RemotingServices.Marshal(objCOMTest);
      
      // This will get the remoting object
      object objRem = Activator.GetObject(typeof(COMTest),
"tcp://localhost:1234/COMTestUri");
      Assertion.Assert("#A07",objRem != null);
      COMTest remCOMTest = (COMTest) objRem;
      Assertion.AssertEquals("#A08", 8, remCOMTest.Id);

      ChannelServices.UnregisterChannel(chnServer);
       // Todo: Implemente the test methods for
       // all the overriden function using activationAttribute
      }

    // This method tests the CreateInstanceFrom methods
    // of the Activator class
    [Test]
      public void CreateInstanceFrom()
      {
      ObjectHandle objHandle;
      objHandle = Activator.CreateInstanceFrom(strAssembly ,
"MonoTests.System.ActivatorTestInternal.COMTest");
      Assertion.Assert("#A09", objHandle != null);
		objHandle.Unwrap();
       // Todo: Implement the test methods for
       // all the overriden function using activationAttribute
      }
    
    // The name of the assembly file is incorrect.
    // I used it to test these classes but you should
    // replace it with the name of the mono tests assembly file
    // The name of the assembly is used to get an object through
    // Activator.CreateInstance(), Activator.CreateComInstanceFrom()...
    private string strAssembly = "corlib_test.dll";
    
    }
  
  }
