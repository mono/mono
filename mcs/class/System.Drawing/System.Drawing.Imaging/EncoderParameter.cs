//
// System.Drawing.Imaging.EncoderParameter.cs
//
// Author: 
//	Ravindra (rkumar@novell.com)
//
// (C) 2004 Novell, Inc.  http://www.novell.com
//

using System;
using System.Text;

namespace System.Drawing.Imaging {

	public sealed class EncoderParameter : IDisposable {

		Encoder encoder;
		Object value;
		int valuesCount;
		EncoderParameterValueType type;
		
		public EncoderParameter (Encoder encoder, byte value)
		{
			this.encoder = encoder;
			this.value = value;
			this.valuesCount = 1;
			this.type = EncoderParameterValueType.ValueTypeByte;
		}

		public EncoderParameter (Encoder encoder, byte[] value)
		{
			this.encoder = encoder;
			this.value = value;
			this.valuesCount = value.Length;
			this.type = EncoderParameterValueType.ValueTypeByte;
		}

		public EncoderParameter (Encoder encoder, short value)
		{
			this.encoder = encoder;
			this.value = value;
			this.valuesCount = 1;
			this.type = EncoderParameterValueType.ValueTypeShort;
		}

		public EncoderParameter (Encoder encoder, short[] value)
		{
			this.encoder = encoder;
			this.value = value;
			this.valuesCount = value.Length;
			this.type = EncoderParameterValueType.ValueTypeShort;
		}

		public EncoderParameter (Encoder encoder, long value)
		{
			this.encoder = encoder;
			this.value = (int) value;
			this.valuesCount = 1;
			this.type = EncoderParameterValueType.ValueTypeLong;
		}

		public EncoderParameter (Encoder encoder, long[] value)
		{
			this.encoder = encoder;
			this.value = convertToIntArr (value);
			this.valuesCount = value.Length;
			this.type = EncoderParameterValueType.ValueTypeLong;
		}

		public EncoderParameter (Encoder encoder, string value)
		{
			this.encoder = encoder;

			ASCIIEncoding ascii = new ASCIIEncoding ();
			int asciiByteCount = ascii.GetByteCount (value);
			byte[] bytes = new byte[asciiByteCount+1];
			ascii.GetBytes (value, 0, value.Length, bytes, 0);
			bytes[asciiByteCount] = (byte) '\0';

			this.value = ascii.GetString (bytes);
			this.valuesCount = bytes.Length;
			this.type = EncoderParameterValueType.ValueTypeAscii;
		}

		public EncoderParameter (Encoder encoder, byte value, bool undefined)
		{
			this.encoder = encoder;
			this.value = value;
			this.valuesCount = 1;
			if (undefined)
				this.type = EncoderParameterValueType.ValueTypeUndefined;
			else
				this.type = EncoderParameterValueType.ValueTypeByte;
		}

		public EncoderParameter (Encoder encoder, byte[] value, bool undefined)
		{
			this.encoder = encoder;
			this.value = value;
			this.valuesCount = value.Length;
			if (undefined)
				this.type = EncoderParameterValueType.ValueTypeUndefined;
			else
				this.type = EncoderParameterValueType.ValueTypeByte;
		}

		public EncoderParameter (Encoder encoder, int numerator, int denominator)
		{
			this.encoder = encoder;
			this.value = new int[] {numerator, denominator};
			this.valuesCount = 1;
			this.type = EncoderParameterValueType.ValueTypeRational;
		}

		public EncoderParameter (Encoder encoder, int[] numerator, int[] denominator)
		{
			this.encoder = encoder;
			this.value = new int[][] {numerator, denominator};
			this.valuesCount = numerator.Length;
			this.type = EncoderParameterValueType.ValueTypeRational;
		}

		public EncoderParameter (Encoder encoder, long rangebegin, long rangeend)
		{
			this.encoder = encoder;
			this.value = new int[] { (int) rangebegin, (int) rangeend};
			this.valuesCount = 1;
			this.type = EncoderParameterValueType.ValueTypeLongRange;
		}

		public EncoderParameter (Encoder encoder, long[] rangebegin, long[] rangeend)
		{
			this.encoder = encoder;
			int[] startRange = convertToIntArr (rangebegin);
			int[] endRange = convertToIntArr (rangeend);
			this.value = new int[][] {startRange, endRange};
			this.valuesCount = rangebegin.Length;
			this.type = EncoderParameterValueType.ValueTypeLongRange;
		}

		public EncoderParameter (Encoder encoder, int numberOfValues, int type, int value)
		{
			this.encoder = encoder;
			this.value = value;
			this.valuesCount = numberOfValues;
			this.type = (EncoderParameterValueType) type;
		}

		public EncoderParameter (Encoder encoder, int numerator1, int denominator1, int numerator2, int denominator2)
		{
			this.encoder = encoder;
			this.value = new int[] {numerator1, denominator1, numerator2, denominator2};
			this.valuesCount = 1;
			this.type = EncoderParameterValueType.ValueTypeRationalRange;
		}

		public EncoderParameter (Encoder encoder, int[] numerator1, int[] denominator1, int[] numerator2, int[] denominator2)
		{
			this.encoder = encoder;
			this.value = new int[][] {numerator1, denominator1, numerator2, denominator2};
			this.valuesCount = numerator1.Length;
			this.type = EncoderParameterValueType.ValueTypeRationalRange;
		}

		public Encoder Encoder {
			get {
				return encoder;
			}

			set {
				encoder = value;
			}
		}

		public int NumberOfValues {
			get {
				return valuesCount;
			}
		}

		public EncoderParameterValueType Type {
			get {
				return type;
			}
		}

		public EncoderParameterValueType ValueType {
			get {
				return type;
			}
		}

		void Dispose (bool disposing) {

			// release the resources
		}

		public void Dispose () {

			Dispose (true);		
		}

		~EncoderParameter () {

			Dispose (false);
		}

		internal int[] convertToIntArr (long[] arr)
		{
			int[] intArr = new int [arr.Length];

			for (int i=0; i<arr.Length; i++)
				intArr[i] = (int) arr[i];

			return intArr;
		}

	}
}
