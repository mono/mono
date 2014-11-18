//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;

    [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview (Critical) - works on CodeGenerator objects, which require Critical access.")]
    class BitFlagsGenerator
    {
        int bitCount;
        CodeGenerator ilg;
        LocalBuilder[] locals;

        public BitFlagsGenerator(int bitCount, CodeGenerator ilg, string localName)
        {
            this.ilg = ilg;
            this.bitCount = bitCount;
            int localCount = (bitCount + 7) / 8;
            locals = new LocalBuilder[localCount];
            for (int i = 0; i < locals.Length; i++)
            {
                locals[i] = ilg.DeclareLocal(typeof(byte), localName + i, (byte) 0);
            }
        }

        public static bool IsBitSet(byte[] bytes, int bitIndex)
        {
            int byteIndex = GetByteIndex(bitIndex);
            byte bitValue = GetBitValue(bitIndex);
            return (bytes[byteIndex] & bitValue) == bitValue;
        }

        public static void SetBit(byte[] bytes, int bitIndex)
        {
            int byteIndex = GetByteIndex(bitIndex);
            byte bitValue = GetBitValue(bitIndex);
            bytes[byteIndex] |= bitValue;
        }

        public int GetBitCount()
        {
            return bitCount;
        }

        public LocalBuilder GetLocal(int i)
        {
            return locals[i];
        }

        public int GetLocalCount()
        {
            return locals.Length;
        }

        public void Load(int bitIndex)
        {
            LocalBuilder local = locals[GetByteIndex(bitIndex)];
            byte bitValue = GetBitValue(bitIndex);
            ilg.Load(local);
            ilg.Load(bitValue);
            ilg.And();
            ilg.Load(bitValue);
            ilg.Ceq();
        }

        public void LoadArray()
        {
            LocalBuilder localArray = ilg.DeclareLocal(Globals.TypeOfByteArray, "localArray");
            ilg.NewArray(typeof(byte), locals.Length);
            ilg.Store(localArray);
            for (int i = 0; i < locals.Length; i++)
            {
                ilg.StoreArrayElement(localArray, i, locals[i]);
            }
            ilg.Load(localArray);
        }

        public void Store(int bitIndex, bool value)
        {
            LocalBuilder local = locals[GetByteIndex(bitIndex)];
            byte bitValue = GetBitValue(bitIndex);
            if (value)
            {
                ilg.Load(local);
                ilg.Load(bitValue);
                ilg.Or();
                ilg.Stloc(local);
            }
            else
            {
                ilg.Load(local);
                ilg.Load(bitValue);
                ilg.Not();
                ilg.And();
                ilg.Stloc(local);
            }
        }

        static byte GetBitValue(int bitIndex)
        {
            return (byte)(1 << (bitIndex & 7));
        }

        static int GetByteIndex(int bitIndex)
        {
            return bitIndex >> 3;
        }

    }
}

