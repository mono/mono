//
// System.Runtime.Serialization.SerializationCallbackTest.cs
//
// Author: Robert Jordan (robertj@gmx.net)
//

#if NET_2_0

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
        [TestFixture]
        public class SerializationCallbackTest
        {
                [Test]
                public void Test ()
                {
                        Log.Clear ();
                        Driver (new BinaryFormatter (), new A (new B()));
                        //Console.WriteLine (Log.Text);
                        Assert.AreEqual (Log.Text, "A1B1A2B2A3B3B4A4");
                }

                [Test]
                public void TestInheritance ()
                {
                        Log.Clear ();
                        Driver (new BinaryFormatter (), new C (new B()));
                        //Console.WriteLine (Log.Text);
                        Assert.AreEqual (Log.Text, "A1C1B1A2C2B2A3B3B4A4");
                }

                [Test]
                public void TestISerializable ()
                {
                        Log.Clear ();
                        Driver (new BinaryFormatter (), new A (new D()));
                        //Console.WriteLine (Log.Text);
                        Assert.AreEqual (Log.Text, "A1B1A2B2A3B3B4A4");
                }

                void Driver (IFormatter formatter, A a)
                {
                        MemoryStream stream = new MemoryStream();
                        formatter.Serialize(stream, a);
                        stream.Position = 0;

                        a.CheckSerializationStatus ();
                        a = (A) formatter.Deserialize (stream);
                        a.CheckDeserializationStatus ();
                }
        }

        class Log
        {
                static StringBuilder b = new StringBuilder ();

                public static void Write (string msg)
                {
                        b.Append (msg);
                }

                public static void Clear ()
                {
                        b = new StringBuilder ();
                }

                public static string Text {
                        get { return b.ToString (); }
                }
        }

        [Serializable]
        class A : IDeserializationCallback
        {
                public int Status = 0;
                B inner;

                public A (B inner)
                {
                        this.inner = inner;
                        this.inner.Outer = this;
                }

                public void CheckSerializationStatus ()
                {
                        Assert.AreEqual (2, Status, "#A01");
                }

                public void CheckDeserializationStatus ()
                {
                        Assert.AreEqual (2, Status, "#A01");
                }

                [OnSerializing]
                void OnSerializing (StreamingContext ctx)
                {
                        Log.Write ("A1");
                        Assert.AreEqual (0, Status, "#A01");
                        Assert.AreEqual (0, inner.Status, "#A02");
                        Status++;
                }

                [OnSerialized]
                void OnSerialized (StreamingContext ctx)
                {
                        Log.Write ("A2");
                        Assert.AreEqual (1, Status, "#A03");
                        Assert.AreEqual (1, inner.Status, "#A04");
                        // must have no effect after deserialization
                        Status++;
                }

                [OnDeserializing]
                void OnDeserializing (StreamingContext ctx)
                {
                        Log.Write ("A3");
                        Assert.IsNull (inner, "#A05");
                        Assert.AreEqual(0, Status, "#A06");
                        // must have no effect after deserialization
                        Status = 42;
                }

                [OnDeserialized]
                void OnDeserialized (StreamingContext ctx)
                {
                        Log.Write ("A4");
                        Assert.IsNotNull (inner, "#A07");
                        Assert.AreEqual(1, Status, "#A08");
                        Assert.AreEqual(1, inner.Status, "#A10");
                        Status++;
                }

                void IDeserializationCallback.OnDeserialization (object sender)
                {
                        // don't log the order because it's undefined
                        CheckDeserializationStatus ();
                }

        }

        [Serializable]
        class B : IDeserializationCallback
        {
                public int Status = 0;
                public A Outer;

                [OnSerializing]
                void OnSerializing (StreamingContext ctx)
                {
                        Log.Write ("B1");
                        Assert.AreEqual (0, Status, "#B01");
                        Assert.AreEqual (1, Outer.Status, "#B01.2");
                        Status++;
                }

                [OnSerialized]
                void OnSerialized (StreamingContext ctx)
                {
                        Log.Write ("B2");
                        Assert.AreEqual (1, Status, "#B02");
                        Assert.AreEqual (2, Outer.Status, "#B03");
                        // must have no effect after deserialization
                        Status++;
                }

                [OnDeserializing]
                void OnDeserializing (StreamingContext ctx)
                {
                        Log.Write ("B3");
                        Assert.IsNull (Outer, "#B05");
                        Assert.AreEqual (0, Status, "#B06");
                        // must have no effect after deserialization
                        Status = 42;
                }

                [OnDeserialized]
                void OnDeserialized (StreamingContext ctx)
                {
                        Log.Write ("B4");
                }

                void IDeserializationCallback.OnDeserialization (object sender)
                {
                        // don't log the order because it's undefined
                        Assert.AreEqual (1, Status);
                }
        }

        [Serializable]
        class C : A
        {
                public C (B inner) : base (inner)
                {
                }

                [OnSerializing]
                void OnSerializing (StreamingContext ctx)
                {
                        Log.Write ("C1");
                        Assert.AreEqual (1, Status, "#C01");
                }

                [OnSerialized]
                void OnSerialized (StreamingContext ctx)
                {
                        Log.Write ("C2");
                        Assert.AreEqual (2, Status, "#C02");
                }
        }

        [Serializable]
        class D : B, ISerializable
        {
                public D ()
                {
                }

                void ISerializable.GetObjectData (SerializationInfo info, StreamingContext ctx)
                {
                        info.AddValue ("Status", Status);
                        info.AddValue ("Outer", Outer);
                }

                D (SerializationInfo info, StreamingContext ctx)
                {
                        Status = info.GetInt32 ("Status");
                        Outer = (A) info.GetValue ("Outer", typeof (A));
                }
        }
}

#endif
