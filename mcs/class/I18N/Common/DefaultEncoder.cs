// This is partially copied source from referencesource/mscorlib/system/text/encoding.cs, modifying a bit.

// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==


using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace I18N.Common
{
        [Serializable]
        public class ReferenceSourceDefaultEncoder : Encoder, IObjectReference
        {
            private Encoding m_encoding;
            [NonSerialized] private bool m_hasInitializedEncoding;

            [NonSerialized] internal char charLeftOver;

            public ReferenceSourceDefaultEncoder(Encoding encoding)
            {
                m_encoding = encoding;
                m_hasInitializedEncoding = true;
            }

            // Constructor called by serialization, have to handle deserializing from Everett
            internal ReferenceSourceDefaultEncoder(SerializationInfo info, StreamingContext context)
            {
                if (info==null) throw new ArgumentNullException("info");
                Contract.EndContractBlock();

                // All we have is our encoding
                this.m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));

                try 
                {
                    //this.m_fallback     = (EncoderFallback) info.GetValue("m_fallback",   typeof(EncoderFallback));
                    this.charLeftOver   = (Char)            info.GetValue("charLeftOver", typeof(Char));
                }
                catch (SerializationException)
                {
                }
            }

            // Just get it from GetEncoding
            [System.Security.SecurityCritical]  // auto-generated
            public Object GetRealObject(StreamingContext context)
            {
                // upon deserialization since the DefaultEncoder implement IObjectReference the 
                // serialization code tries to do the fixup. The fixup returns another 
                // IObjectReference (the DefaultEncoder) class and hence so on and on. 
                // Finally the deserialization logics fails after following maximum references
                // unless we short circuit with the following
                if (m_hasInitializedEncoding)
                {
                    return this;
                }

                Encoder encoder = m_encoding.GetEncoder();
                /*
                if (m_fallback != null)
                    encoder.m_fallback = m_fallback;
                if (charLeftOver != (char) 0)
                {
                    EncoderNLS encoderNls = encoder as EncoderNLS;
                    if (encoderNls != null)
                        encoderNls.charLeftOver = charLeftOver;
                }
                */
                return encoder;
            }

#if FEATURE_SERIALIZATION
            // ISerializable implementation, get data for this object
            [System.Security.SecurityCritical]  // auto-generated_required
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                // Any info?
                if (info==null) throw new ArgumentNullException("info");
                Contract.EndContractBlock();

                // All we have is our encoding
                info.AddValue("encoding", this.m_encoding);
            }
#endif

            // Returns the number of bytes the next call to GetBytes will
            // produce if presented with the given range of characters and the given
            // value of the flush parameter. The returned value takes into
            // account the state in which the encoder was left following the last call
            // to GetBytes. The state of the encoder is not affected by a call
            // to this method.
            //

            public override int GetByteCount(char[] chars, int index, int count, bool flush)
            {
                return m_encoding.GetByteCount(chars, index, count);
            }

            [System.Security.SecurityCritical]  // auto-generated
            [SuppressMessage("Microsoft.Contracts", "CC1055")]  // Skip extra error checking to avoid *potential* AppCompat problems.
            public unsafe override int GetByteCount(char* chars, int count, bool flush)
            {
                return m_encoding.GetByteCount(chars, count);
            }

            // Encodes a range of characters in a character array into a range of bytes
            // in a byte array. The method encodes charCount characters from
            // chars starting at index charIndex, storing the resulting
            // bytes in bytes starting at index byteIndex. The encoding
            // takes into account the state in which the encoder was left following the
            // last call to this method. The flush parameter indicates whether
            // the encoder should flush any shift-states and partial characters at the
            // end of the conversion. To ensure correct termination of a sequence of
            // blocks of encoded bytes, the last call to GetBytes should specify
            // a value of true for the flush parameter.
            //
            // An exception occurs if the byte array is not large enough to hold the
            // complete encoding of the characters. The GetByteCount method can
            // be used to determine the exact number of bytes that will be produced for
            // a given range of characters. Alternatively, the GetMaxByteCount
            // method of the Encoding that produced this encoder can be used to
            // determine the maximum number of bytes that will be produced for a given
            // number of characters, regardless of the actual character values.
            //

            public override int GetBytes(char[] chars, int charIndex, int charCount,
                                          byte[] bytes, int byteIndex, bool flush)
            {
                return m_encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            }

            [System.Security.SecurityCritical]  // auto-generated
            [SuppressMessage("Microsoft.Contracts", "CC1055")]  // Skip extra error checking to avoid *potential* AppCompat problems.
            public unsafe override int GetBytes(char* chars, int charCount,
                                                 byte* bytes, int byteCount, bool flush)
            {
                return m_encoding.GetBytes(chars, charCount, bytes, byteCount);
            }
        }
}
