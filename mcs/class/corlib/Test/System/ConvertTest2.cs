using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System {

	[TestFixture]
	public class ConvertTest : Assertion {

		//Bug: 481687
		[Test]
		public void TestConvertToType ()
		{
			BitmapStatus bitmapStatus = new BitmapStatus(1);
			Image i = System.Convert.ChangeType(bitmapStatus, typeof(Image)) as Image;
			AssertNotNull ("convert result", i);
		}
	}

	public class Image {
	}
	
	public class BitmapStatus : IConvertible
	{
		protected int m_Status;
	
		public BitmapStatus(int status)
		{
			m_Status = status;
		}
	
		TypeCode IConvertible.GetTypeCode()
		{
			return TypeCode.Int32;
		}
	
		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return (bool)((IConvertible)this).ToType(typeof(bool), provider);
		}
	
		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return (byte)((IConvertible)this).ToType(typeof(byte), provider);
		}
	
		char IConvertible.ToChar(IFormatProvider provider)
		{
			return (char)((IConvertible)this).ToType(typeof(char), provider);
		}
	
		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return (DateTime)((IConvertible)this).ToType(typeof(DateTime), provider);
		}
	
		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return (decimal)((IConvertible)this).ToType(typeof(decimal), provider);
		}
	
		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return (double)((IConvertible)this).ToType(typeof(double), provider);
		}
	
		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return (short)((IConvertible)this).ToType(typeof(short), provider);
		}
	
		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return (int)m_Status;
		}
	
		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return (long)((IConvertible)this).ToType(typeof(long), provider);
		}
	
		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return (sbyte)((IConvertible)this).ToType(typeof(sbyte), provider);
		}
	
		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return (float)((IConvertible)this).ToType(typeof(float), provider);
		}
	
		string IConvertible.ToString(IFormatProvider provider)
		{
			return (string)((IConvertible)this).ToType(typeof(string), provider);
		}
	
		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			if (conversionType == typeof(Image))
				return new Image ();
			
			else if (conversionType.IsAssignableFrom(typeof(int)))
				return Convert.ChangeType(1, conversionType, provider);
			return null;
		}
	
		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return (ushort)((IConvertible)this).ToType(typeof(ushort), provider);
		}
	
		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return (uint)((IConvertible)this).ToType(typeof(uint), provider);
		}
	
		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return (ulong)((IConvertible)this).ToType(typeof(ulong), provider);
		}
	
	}
		
}

