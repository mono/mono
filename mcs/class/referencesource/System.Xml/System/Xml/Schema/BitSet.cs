//------------------------------------------------------------------------------
// <copyright file="BitSet.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Text;
    using System.Diagnostics;

    internal sealed class BitSet {
        private const int bitSlotShift = 5;
        private const int bitSlotMask = (1 << bitSlotShift) - 1;

        private int      count;
        private uint[]    bits;

    	private BitSet() {
    	}

        public BitSet(int count) {
            this.count = count;
            bits = new uint[Subscript(count + bitSlotMask)];
        }

        public int Count {
            get { return count; }
        }

        public bool this[int index] {
            get {
                return Get(index);
            }
         }

        public void Clear() {
            int bitsLength = bits.Length;
            for (int i = bitsLength; i-- > 0 ;) {
                bits[i] = 0;
            }
        }

        public void Clear(int index) {
            int nBitSlot = Subscript(index);
            EnsureLength(nBitSlot + 1);
            bits[nBitSlot] &= ~((uint)1 << (index & bitSlotMask));
        }

        public void Set(int index) {
            int nBitSlot = Subscript(index);
            EnsureLength(nBitSlot + 1);
            bits[nBitSlot] |= (uint)1 << (index & bitSlotMask);
        }


        public bool Get(int index) {
            bool fResult = false;
            if (index < count) {
                int nBitSlot = Subscript(index);
                fResult = ((bits[nBitSlot] & (1 << (index & bitSlotMask))) != 0);
            }
            return fResult;
        }

        public int NextSet(int startFrom) {
            Debug.Assert(startFrom >= -1 && startFrom <= count);
            int offset = startFrom + 1;
            if (offset == count) {
                return -1;
            }
            int nBitSlot = Subscript(offset);
            offset &= bitSlotMask;
            uint word = bits[nBitSlot] >> offset;
            // locate non-empty slot
            while (word == 0) {
                if ((++ nBitSlot) == bits.Length ) {
                    return -1;
                }
                offset = 0;
                word = bits[nBitSlot];
            }
            while ((word & (uint)1) == 0) {
                word >>= 1;
                offset ++;
            }
            return (nBitSlot << bitSlotShift) + offset;
        }

        public void And(BitSet other) {
            /*
             * Need to synchronize  both this and other->
             * This might lead to deadlock if one thread grabs them in one order
             * while another thread grabs them the other order.
             * Use a trick from Doug Lea's book on concurrency,
             * somewhat complicated because BitSet overrides hashCode().
             */
            if (this == other) {
                return;
            }
            int bitsLength = bits.Length;
            int setLength = other.bits.Length;
            int n = (bitsLength > setLength) ? setLength : bitsLength;
            for (int i = n ; i-- > 0 ;) {
                bits[i] &= other.bits[i];
            }
            for (; n < bitsLength ; n++) {
                bits[n] = 0;
            }
        }


        public void Or(BitSet other) {
            if (this == other) {
                return;
            }
            int setLength = other.bits.Length;
            EnsureLength(setLength);
            for (int i = setLength; i-- > 0 ;) {
                bits[i] |= other.bits[i];
            }
        }

        public override int GetHashCode() {
            int h = 1234;
            for (int i = bits.Length; --i >= 0;) {
                h ^= (int)bits[i] * (i + 1);
            }
            return(int)((h >> 32) ^ h);
        }


        public override bool Equals(object obj) {
            // assume the same type
            if (obj != null) {
                if (this == obj) {
                    return true;
                }
                BitSet other = (BitSet) obj;

                int bitsLength = bits.Length;
                int setLength = other.bits.Length;
                int n = (bitsLength > setLength) ? setLength : bitsLength;
                for (int i = n ; i-- > 0 ;) {
                    if (bits[i] != other.bits[i]) {
                        return false;
                    }
                }
                if (bitsLength > n) {
                    for (int i = bitsLength ; i-- > n ;) {
                        if (bits[i] != 0) {
                            return false;
                        }
                    }
                }
                else {
                    for (int i = setLength ; i-- > n ;) {
                        if (other.bits[i] != 0) {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public BitSet Clone() {
            BitSet newset = new BitSet();
            newset.count = count;
            newset.bits = (uint[])bits.Clone();
            return newset;
        }


        public bool IsEmpty {
            get {
                uint k = 0;
                for (int i = 0; i < bits.Length; i++) {
                    k |= bits[i];
                }
                return k == 0;
            }
        }

        public bool Intersects(BitSet other) {
            int i = Math.Min(this.bits.Length, other.bits.Length);
            while (--i >= 0) {
                if ((this.bits[i] & other.bits[i]) != 0) {
                    return true;
                }
            }
            return false;
        }

        private int Subscript(int bitIndex) {
            return bitIndex >> bitSlotShift;
        }

        private void EnsureLength(int nRequiredLength) {
            /* Doesn't need to be synchronized because it's an internal method. */
            if (nRequiredLength > bits.Length) {
                /* Ask for larger of doubled size or required size */
                int request = 2 * bits.Length;
                if (request < nRequiredLength)
                    request = nRequiredLength;
                uint[] newBits = new uint[request];
                Array.Copy(bits, newBits, bits.Length);
                bits = newBits;
            }
        }

#if DEBUG
        public void Dump(StringBuilder bb) {
            for (int i = 0; i < count; i ++) {
                bb.Append( Get(i) ? "1" : "0");
            }
        }
#endif
    };

}

