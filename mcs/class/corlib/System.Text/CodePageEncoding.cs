//
// System.Text.CodePageEncoding.cs
//
// Author:
//   Kornél Pál <http://www.kornelpal.hu/>
//
// Copyright (C) 2006 Kornél Pál
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//
// These proxy classes implement IObjectReference.GetRealObject() that returns
// an instance of the appropriate Encoding, Encoder or Decoder class.
// As a result serialized objects of these types will transparently be
// deserialized to instances of the above described classes.
//
// Use SerializationInfo.SetType() in ISerializable.GetObjectData() method of
// serializable classes to serialize their instances using a proxy class.
//
// All of these proxy classes are non-public thus they only have to be
// serialization compatible with .NET Framework.
//

//
// .NET Framework 1.x uses this class for internal encodings and
// .NET Framework 2.0 serializes internal encodings using a proxy.
// This class supports serialization compatibility.
//

using System;
using System.Runtime.Serialization;

namespace System.Text
{
	[Serializable]
	internal sealed class CodePageEncoding : ISerializable, IObjectReference
	{
		//
		// .NET Framework 1.x uses this class for internal decoders and
		// .NET Framework 2.0 can deserialize them using a proxy.
		// This class supports serialization compatibility.
		//

		[Serializable]
		private sealed class Decoder : ISerializable, IObjectReference
		{
			private Encoding encoding;
			private System.Text.Decoder realObject;

			private Decoder (SerializationInfo info, StreamingContext context)
			{
				if (info == null)
					throw new ArgumentNullException ("info");

				this.encoding = (Encoding) info.GetValue ("encoding", typeof (Encoding));
			}

			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				throw new ArgumentException ("This class cannot be serialized.");
			}

			public object GetRealObject (StreamingContext context)
			{
				if (this.realObject == null)
					this.realObject = this.encoding.GetDecoder ();

				return this.realObject;
			}
		}

		private int codePage;
#if NET_2_0
		private bool isReadOnly;
		private EncoderFallback encoderFallback;
		private DecoderFallback decoderFallback;
#endif
		private Encoding realObject;

		private CodePageEncoding (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			this.codePage = (int) info.GetValue ("m_codePage", typeof (int));

#if NET_2_0
			try {
				this.isReadOnly = (bool) info.GetValue ("m_isReadOnly", typeof (bool));
				this.encoderFallback = (EncoderFallback) info.GetValue ("encoderFallback", typeof (EncoderFallback));
				this.decoderFallback = (DecoderFallback) info.GetValue ("decoderFallback", typeof (DecoderFallback));
			} catch (SerializationException) {
				// .NET Framework 1.x has no fallbacks
				this.isReadOnly = true;
			}
#endif
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new ArgumentException ("This class cannot be serialized.");
		}

		public object GetRealObject (StreamingContext context)
		{
			if (this.realObject == null) {
				Encoding encoding = Encoding.GetEncoding (this.codePage);

#if NET_2_0
				if (!this.isReadOnly) {
					encoding = (Encoding) encoding.Clone ();
					encoding.EncoderFallback = this.encoderFallback;
					encoding.DecoderFallback = this.decoderFallback;
				}
#endif

				this.realObject = encoding;
			}

			return this.realObject;
		}
	}
}
