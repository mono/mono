using System;
using System.IO;
using NUnit.Framework;
using System.Runtime.Serialization.Formatters.Soap;

namespace SoapShared
{
	/// <summary>
	/// Summary description for InternalSoapValuesTest.
	/// </summary>
	[TestFixture]
	public class InternalSoapValuesTest
	{
		private MemoryStream ms;
		private SoapFormatter sf;
		

		public InternalSoapValuesTest()
		{
			ms = new MemoryStream();
			sf = new SoapFormatter();
		}

		[Test]
		public void WriteReadData()
		{
			SerializedClass c = new SerializedClass();
			
			SerializeDeserialize(c);
			SerializeDeserialize(new SerializedClass[]{c,c});
			SerializeDeserialize(c.str);
			SerializeDeserialize(c.m_bool);
			SerializeDeserialize(c.m_byte);
			SerializeDeserialize(c.m_bytes);
			SerializeDeserialize(c.m_decimal);
			SerializeDeserialize(c.m_double);
			SerializeDeserialize(c.m_float);
			SerializeDeserialize(c.m_int);
			SerializeDeserialize(c.m_long);
			SerializeDeserialize(c.m_object);
			SerializeDeserialize(c.m_sbyte);
			SerializeDeserialize(c.m_short);
			SerializeDeserialize(c.m_time);
			SerializeDeserialize(c.m_timeSpan);
			SerializeDeserialize(c.m_uint);
			SerializeDeserialize(c.m_ulong);
			SerializeDeserialize(c.m_ushort);
		}

		private void SerializeDeserialize(object obj)
		{
			ms = new MemoryStream();
			Serialize(obj, ms);
			ms.Position = 0;
			Object des = Deserialize(ms);
			Assertion.AssertEquals(obj.GetType(), des.GetType());
		}

		private void Serialize(object ob, Stream stream)
		{
			sf.Serialize(stream, ob);
		}

		private object Deserialize(Stream stream)
		{
			Object obj = sf.Deserialize(stream);
			return obj;
		}
	}
	
	[Serializable]
	class SerializedClass
	{
		public string str = "rrr";
		public bool m_bool;
		public sbyte m_sbyte;
		public byte m_byte;
		public long m_long;
		public ulong m_ulong;
		public int m_int;
		public uint m_uint;
		public float m_float;
		public double m_double;
		public decimal m_decimal;
		public short m_short;
		public ushort m_ushort;
		public object m_object = new object();
		public TimeSpan m_timeSpan = TimeSpan.FromTicks(TimeSpan.TicksPerDay);
		public byte[] m_bytes = new byte[10];
		public DateTime m_time = DateTime.Now;
	}
}
