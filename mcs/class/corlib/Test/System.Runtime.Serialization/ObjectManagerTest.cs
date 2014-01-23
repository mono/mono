//
// System.Runtime.Serialization.ObjectManagerTest.cs
//
// Author: Martin Baulig (martin@ximian.com)
//
// (C) Novell
//

using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class ObjectManagerTest
	{
		[Test] // bug 76931
		public void TestSerialization ()
		{
			using (MemoryStream ms = new MemoryStream ()) {
				Bar bar = new Bar (8, 3, 5, 21);
				bar.Save (ms);

				ms.Position = 0;

				bar = Bar.Load (ms);
				
				Assert.AreEqual ("Bar [Foo (16),(Foo (6),Foo (10),Foo (42)]",
					bar.ToString (), "#1");
			}
		}
	}

	public class Foo
	{
		public int Data;

		public Foo (int data)
		{
			this.Data = data;
		}

		public override string ToString ()
		{
			return String.Format ("Foo ({0})", Data);
		}

		internal class SerializationSurrogate : ISerializationSurrogate
		{
			public void GetObjectData (object obj, SerializationInfo info, StreamingContext context)
			{
				Foo foo = (Foo) obj;

				info.AddValue ("data", foo.Data);
			}

			public object SetObjectData (object obj, SerializationInfo info,
							 StreamingContext context,
							 ISurrogateSelector selector)
			{
				Foo foo = (Foo) obj;

				foo.Data = info.GetInt32 ("data");

				return new Foo (2 * foo.Data);
			}
		}
	}

	[Serializable]
	public class Bar
	{
		public readonly Foo Foo;
		public readonly Foo[] Array;

		public Bar (int a, params int[] b)
		{
			Foo = new Foo (a);
			Array = new Foo[b.Length];
			for (int i = 0; i < b.Length; i++)
				Array[i] = new Foo (b[i]);
		}

		public void Save (Stream stream)
		{
			SurrogateSelector ss = new SurrogateSelector ();

			StreamingContext context = new StreamingContext (
				StreamingContextStates.Persistence, this);

			ss.AddSurrogate (typeof (Foo), context, new Foo.SerializationSurrogate ());

			BinaryFormatter formatter = new BinaryFormatter (ss, context);

			formatter.Serialize (stream, this);
		}

		public static Bar Load (Stream stream)
		{
			SurrogateSelector ss = new SurrogateSelector ();

			StreamingContext context = new StreamingContext (
				StreamingContextStates.Persistence, null);

			ss.AddSurrogate (typeof (Foo), context, new Foo.SerializationSurrogate ());

			BinaryFormatter formatter = new BinaryFormatter (ss, context);

			return (Bar) formatter.Deserialize (stream);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Bar [");
			sb.Append (Foo);
			sb.Append (",(");
			for (int i = 0; i < Array.Length; i++) {
				if (i > 0)
					sb.Append (",");
				sb.Append (Array[i]);
			}
			sb.Append ("]");
			return sb.ToString ();
		}
	}
}
