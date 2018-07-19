//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{

    // ASN.1 DER. DER refers to Distinguished Encoding Rules.
    internal static class DEREncoding
    {
        // OID=1.2.840.113554.1.2.2 (Kerberos V5) { 0x06, 0x09, 0x2a, 0x86, 
        //      0x48, 0x86, 0xf7, 0x12, 0x01, 0x02, 0x02 }

        // 1. 0x60 -- Tag for [APPLICATION 0] SEQUENCE; indicates that
        // -- constructed form, definite length encoding follows.
        // 2. Token length octets, specifying length of subsequent data
        // (i.e., the summed lengths of elements 3-5 in this list, and of the
        // mechanism-defined token object following the tag).  This element
        // comprises a variable number of octets:
        // 2a. If the indicated value is less than 128, it shall be
        // represented in a single octet with bit 8 (high order) set to
        // "0" and the remaining bits representing the value.
        // 2b. If the indicated value is 128 or more, it shall be
        // represented in two or more octets, with bit 8 of the first
        // octet set to "1" and the remaining bits of the first octet
        // specifying the number of additional octets.  The subsequent
        // octets carry the value, 8 bits per octet, most significant
        // digit first.  The minimum number of octets shall be used to
        // encode the length (i.e., no octets representing leading zeros
        // shall be included within the length encoding).

        // 3. 0x06 -- Tag for OBJECT IDENTIFIER

        // 4. Object identifier length -- length (number of octets) of
        // -- the encoded object identifier contained in element 5,
        // -- encoded per rules as described in 2a. and 2b. above.

        // 5. Object identifier octets -- variable number of octets,
        // -- encoded per ASN.1 BER rules:

        // 5a. The first octet contains the sum of two values: (1) the
        // top-level object identifier component, multiplied by 40
        // (decimal), and (2) the second-level object identifier
        // component.  This special case is the only point within an
        // object identifier encoding where a single octet represents
        // contents of more than one component.

        // 5b. Subsequent octets, if required, encode successively-lower
        // components in the represented object identifier.  A component's
        // encoding may span multiple octets, encoding 7 bits per octet
        // (most significant bits first) and with bit 8 set to "1" on all
        // but the final octet in the component's encoding.  The minimum
        // number of octets shall be used to encode each component (i.e.,
        // no octets representing leading zeros shall be included within a
        // component's encoding).

        // (Note: In many implementations, elements 3-5 may be stored and
        // referenced as a contiguous string constant.)


        static byte[] mech = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x12, 0x01, 0x02, 0x02 };
        static byte[] type = new byte[] { 0x01, 0x00 };

        private static bool BufferIsEqual(byte[] arrayOne, int offsetOne, byte[] arrayTwo, int offsetTwo, int length)
        {
            if (length > arrayOne.Length - offsetOne)
            {
                return false;
            }

            if (length > arrayTwo.Length - offsetTwo)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (arrayOne[offsetOne + i] != arrayTwo[offsetTwo + i])
                {
                    return false;
                }
            }

            return true;
        }

        //
        // length is assumed to be non negative
        //

        public static int LengthSize(int length)
        {

            if (length < (1 << 7))
            {

                return 1;
            }
            else if (length < (1 << 8))
            {

                return 2;
            }
            else if (length < (1 << 16))
            {

                return 3;
            }
            else if (length < (1 << 24))
            {

                return 4;
            }
            else
            {

                return 5;
            }
        }

        //
        // fills in a buffer with the token header.  The buffer is assumed 
        // to be the right size.  buffer is advanced past the token header 
        //
        // bodySize includes TokenId
        // 

        public static void MakeTokenHeader(int bodySize, byte[] buffer, ref int offset, ref int len)
        {

            buffer[offset++] = 0x60;
            len--;

            WriteLength(buffer, ref offset, ref len, 1 + LengthSize(mech.Length) + mech.Length + type.Length + bodySize);

            buffer[offset++] = 0x06; // OID
            len--;

            WriteLength(buffer, ref offset, ref len, mech.Length);

            System.Buffer.BlockCopy(mech, 0, buffer, offset, mech.Length);
            offset += mech.Length;
            len -= mech.Length;

            System.Buffer.BlockCopy(type, 0, buffer, offset, type.Length);
            offset += type.Length;
            len -= type.Length;
        }

        //
        // returns decoded length, or < 0 on failure.  Advances buffer and
        // decrements length
        //

        public static int ReadLength(byte[] buffer, ref int offset, ref int length)
        {

            int tmp;
            int ret = 0;

            if (length < 1)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            tmp = buffer[offset++];

            length--;

            if ((tmp & 0x80) != 0)
            {

                if ((tmp &= 0x7f) > (length - 1))
                {

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
                }

                if (tmp > 4)
                { // 4 == sizeof(int)

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
                }

                for (; tmp != 0; tmp--)
                {

                    ret = (ret << 8) + buffer[offset++];
                    length--;
                }

            }
            else
            {

                ret = tmp;
            }

            return ret;
        }

        //
        // returns the length of a token, given the mech oid and the body
        // size 
        //

        public static int TokenSize(int bodySize)
        {

            // set body size to sequence contents size, 2 for token id
            bodySize += 2 + mech.Length + LengthSize(mech.Length) + 1;

            return (1 + LengthSize(bodySize) + bodySize);
        }

        //
        // given a buffer containing a token, reads and verifies the token,
        // leaving buffer advanced past the token header, and setting body_size
        // to the number of remaining bytes
        //

        public static void VerifyTokenHeader(byte[] buffer, ref int offset, ref int len)
        {

            if ((len -= 1) < 0)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            if (buffer[offset++] != 0x60)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            int seqSize = ReadLength(buffer, ref offset, ref len);

            if (seqSize != len)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            if ((len -= 1) < 0)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            if (buffer[offset++] != 0x06)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            int oidLength = ReadLength(buffer, ref offset, ref len); // (byte) buffer[offset++];

            if ((oidLength & 0x7fffffff) != mech.Length)
            { // Overflow??? 

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            if ((len -= oidLength) < 0)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            if (!BufferIsEqual(mech, 0, buffer, offset, mech.Length))
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            offset += oidLength;

            if ((len -= type.Length) < 0)
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            if (!BufferIsEqual(type, 0, buffer, offset, type.Length))
            {

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }

            offset += type.Length;
        }

        public static void WriteLength(byte[] buffer, ref int offset, ref int bufferLength, int length)
        {

            if (length < (1 << 7))
            { // one byte

                // *(*buffer)++ = (unsigned char) length;
                buffer[offset++] = (byte)length;
                bufferLength--;
            }
            else
            {

                // *(*buffer)++ = (unsigned char) (der_length_size(length) + 127);
                buffer[offset++] = (byte)(LengthSize(length) + 127);

                if (length >= (1 << 24))
                {

                    // *(*buffer)++ = (unsigned char) (length >> 24);
                    buffer[offset++] = (byte)(length >> 24);
                    bufferLength--;
                }

                if (length >= (1 << 16))
                {

                    // *(*buffer)++ = (unsigned char) ((length >> 16) & 0xff);
                    buffer[offset++] = (byte)((length >> 16) & 0xFF);
                    bufferLength--;
                }

                if (length >= (1 << 8))
                {

                    // *(*buffer)++ = (unsigned char) ((length >> 8) & 0xff);
                    buffer[offset++] = (byte)((length >> 8) & 0xFF);
                    bufferLength--;
                }

                // *(*buffer)++ = (unsigned char) (length & 0xff);
                buffer[offset++] = (byte)(length & 0xFF);
                bufferLength--;
            }
        }
    }
}

