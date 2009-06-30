// MarshalByRefObjectTest.cs Test class for
// System.MarshalByRefObject class
// 
// Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

// Just an internal test namespace for
// the MarshalByRefObjectTest class
namespace MonoTests.System.MarshalByRefObjectTestInternal
  {

  // Object from this class can be marshaled
  public class MarshalObject: MarshalByRefObject
    {
    public MarshalObject(){}

    public MarshalObject(int id)
      {
      this.id = id;
      }

    // This method override the default one
    // so we can set some properties of the lifetime
    // of the remot object
    public override object InitializeLifetimeService()
      {
      ILease lease = (ILease) base.InitializeLifetimeService();

      // By default InitialLeaseTime is set to 5 minutes
      // we set it at 10 seconds
      if(lease.CurrentState == LeaseState.Initial)
	{
	lease.InitialLeaseTime = TimeSpan.FromSeconds(10);
	}
      return lease;
      }

    public int Id
      {
      get { return id;}
      }

    private int id = 0;
    } // MarshalObject

  } // MonoTests.System.MarshalByRefObjectTestInternal


namespace MonoTests.System
  {
  using MonoTests.System.MarshalByRefObjectTestInternal;
  using NUnit.Framework;

  // The main test class
  [TestFixture]
  public class MarshalByRefObjectTest
    {
    public MarshalByRefObjectTest()
      {

      }

    // This method test the CreateObjRef
    [Test]
      public void CreateObjRef()
      {
      MarshalObject objMarshal = new MarshalObject();

      RemotingServices.SetObjectUriForMarshal(objMarshal, "MarshalByRefObjectTest.objMarshal1");
      RemotingServices.Marshal(objMarshal);

      ObjRef objRef = objMarshal.CreateObjRef(typeof(MarshalObject));
      Assert.AreEqual(objRef.URI, RemotingServices.GetObjectUri(objMarshal), "#A01");

      // TODO: When implemented in the mono RemotingServices class
      //RemotingServices.Disconnect(objMarshal);
      }

    [Test]
      [ExpectedException(typeof(RemotingException))]
      public void CreateObjRefThrowException()
      {
      MarshalObject objMarshal = new MarshalObject();

      ObjRef objRef = objMarshal.CreateObjRef(typeof(MarshalObject));
      }

    // This method both tests InitializeLifetimeService()
    // and GetLifetimeService()
    [Test]
      public void LifetimeService()
      {
      MarshalObject objMarshal = new MarshalObject();

      RemotingServices.SetObjectUriForMarshal(objMarshal, "MarshalByRefObjectTest.objMarshal2");
      RemotingServices.Marshal(objMarshal);
      
      objMarshal.InitializeLifetimeService();
      ILease lease = (ILease) objMarshal.GetLifetimeService();
      Assert.AreEqual(lease.InitialLeaseTime, TimeSpan.FromSeconds(10), "#A02");
      
      // TODO: When implemented in the mono RemotingServices class
      //RemotingServices.Disconnect(objMarshal);
      }

    // Here we test if we a published object can be get 
    // through a TcpChannel
    [Test]
      public void GetObject()
      {
      MarshalObject objMarshal = new MarshalObject(1);

      RemotingServices.SetObjectUriForMarshal(objMarshal, "MarshalByRefObjectTest.objMarshal3");
      RemotingServices.Marshal(objMarshal);

      TcpChannel chn = new TcpChannel(1294);
      ChannelServices.RegisterChannel(chn);
      
      object objRem = Activator.GetObject(typeof(MarshalObject), "tcp://localhost:1294/MarshalByRefObjectTest.objMarshal3");

      MarshalObject objMarshalRem = (MarshalObject) objRem;

      Assert.AreEqual(1, objMarshalRem.Id, "#A03");

      // TODO: When implemented in the mono RemotingServices class
      //RemotingServices.Disconnect(objMarshal);
//      chn.StopListening(null);
      ChannelServices.UnregisterChannel(chn);

      }
    }
  }
