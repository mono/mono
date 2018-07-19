//------------------------------------------------------------------------------
// <copyright file="EventValidationStore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Web.Security.Cryptography;
    using System.Web.Util;

    // Represents a store of all of the event validation (target, argument) tuples
    // that are valid for a given WebForms page.

    internal sealed class EventValidationStore {

        // We don't want to use a full SHA-256 hash since it produces an unacceptable increase in the size
        // of the __EVENTVALIDATION field. Instead, we truncate the SHA-256 hash to 128 bits. This is
        // acceptable according to the Crypto SDL v5.2.
        private const int HASH_SIZE_IN_BYTES = 128 / 8;

        // contains all cryptographic hashes which are known to this event validation instance
        private readonly HashSet<byte[]> _hashes = new HashSet<byte[]>(HashEqualityComparer.Instance);

        public int Count {
            get {
                return _hashes.Count;
            }
        }

        public void Add(string target, string argument) {
            _hashes.Add(Hash(target, argument));
        }

        // Creates a duplicate store seeded with the same hashes as the current store.
        public EventValidationStore Clone() {
            EventValidationStore newStore = new EventValidationStore();
            newStore._hashes.UnionWith(this._hashes);
            return newStore;
        }

        public bool Contains(string target, string argument) {
            return _hashes.Contains(Hash(target, argument));
        }

        // Stores a string in a buffer at the specified offset. The string is stored as the
        // 32-bit character count (big-endian) followed by the string data as UTF-16BE.
        // Null strings are treated as equal to empty string. When the method completes, the
        // 'offset' parameter will be updated to point *after* the string in the buffer.
        private static void CopyStringToBuffer(string s, byte[] buffer, ref int offset) {
            int stringLength = (s != null) ? s.Length : 0;

            buffer[offset++] = (byte)(stringLength >> 24);
            buffer[offset++] = (byte)(stringLength >> 16);
            buffer[offset++] = (byte)(stringLength >> 8);
            buffer[offset++] = (byte)(stringLength);

            if (s != null) {
                for (int i = 0; i < s.Length; i++) {
                    char c = s[i];
                    buffer[offset++] = (byte)(c >> 8);
                    buffer[offset++] = (byte)(c);
                }
            }
        }

        public static EventValidationStore DeserializeFrom(Stream inputStream) {
            // don't need a 'using' block around this reader
            DeserializingBinaryReader reader = new DeserializingBinaryReader(inputStream);

            byte versionHeader = reader.ReadByte();
            if (versionHeader != (byte)0x00) {
                // the only version we support is v0; throw if unsupported
                throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
            }

            EventValidationStore store = new EventValidationStore();

            // 'numEntries' is the number of HASH_SIZE_IN_BYTES-sized entries
            // we should expect in the stream.
            int numEntries = reader.Read7BitEncodedInt();
            for (int i = 0; i < numEntries; i++) {
                byte[] entry = reader.ReadBytes(HASH_SIZE_IN_BYTES);
                if (entry.Length != HASH_SIZE_IN_BYTES) {
                    // bad data (EOF)
                    throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
                }
                store._hashes.Add(entry);
            }

            return store;
        }

        private static byte[] Hash(string target, string argument) {
            // This algorithm previously used MemoryStream and BinaryWriter, but this was causing a measurable
            // performance hit since Event Validation code might be run in a tight loop. We'll instead just
            // build up the buffer to be hashed manually.

            int targetStringLength = (target != null) ? target.Length : 0; // null and empty 'target' treated equally
            int argumentStringLength = (argument != null) ? argument.Length : 0; // null and empty 'argument' treated equally
            byte[] bufferToBeHashed = new byte[8 + (targetStringLength + argumentStringLength) * 2]; // for each string, 4 bytes length prefix + (2 * length) bytes for UTF-16 payload

            // copy strings into buffer
            int currentOffset = 0;
            CopyStringToBuffer(target, bufferToBeHashed, ref currentOffset);
            CopyStringToBuffer(argument, bufferToBeHashed, ref currentOffset);
            Debug.Assert(currentOffset == bufferToBeHashed.Length, "Should have populated the entire buffer.");

            // hash the buffer
            byte[] fullHash;
            using (SHA256 hashAlgorithm = CryptoAlgorithms.CreateSHA256()) {
                fullHash = hashAlgorithm.ComputeHash(bufferToBeHashed);
            }

            // truncate to desired size; SHA evenly distributes entropy throughout the generated hash,
            // so for simplicity we'll just chop off the last several bytes
            byte[] truncatedHash = new byte[HASH_SIZE_IN_BYTES];
            Buffer.BlockCopy(fullHash, 0, truncatedHash, 0, HASH_SIZE_IN_BYTES);
            return truncatedHash;
        }

        public void SerializeTo(Stream outputStream) {
            // don't need a 'using' block around this writer
            SerializingBinaryWriter writer = new SerializingBinaryWriter(outputStream);

            writer.Write((byte)0x00); // version header
            writer.Write7BitEncodedInt(_hashes.Count); // number of entries
            foreach (byte[] entry in _hashes) {
                writer.Write(entry);
            }
        }

        private sealed class HashEqualityComparer : IEqualityComparer<byte[]> {
            internal static readonly HashEqualityComparer Instance = new HashEqualityComparer();

            private HashEqualityComparer() { }

            public bool Equals(byte[] x, byte[] y) {
                // The lengths of 'x' and 'y' are checked before the values are added to the HashSet.
                // Add a debug assert here just to check it if we ever change the algorithm from SHA256.
                Debug.Assert(x.Length == HASH_SIZE_IN_BYTES);
                Debug.Assert(y.Length == HASH_SIZE_IN_BYTES);

                // We're not too concerned about timing attacks here since the event validation
                // hashes are all public knowledge.
                for (int i = 0; i < HASH_SIZE_IN_BYTES; i++) {
                    if (x[i] != y[i]) { return false; }
                }
                return true;
            }

            public int GetHashCode(byte[] obj) {
                // Since the incoming byte[] represents a cryptographic hash code, entropy should be
                // approximately uniformly distributed throughout the entire array, so we can just
                // treat the high 32 bits as the hash code for simplicity.
                return BitConverter.ToInt32(obj, 0);
            }
        }

        private sealed class DeserializingBinaryReader : BinaryReader {
            public DeserializingBinaryReader(Stream input) : base(input) { }

            protected override void Dispose(bool disposing) {
                // Don't call base.Dispose(), since it disposes of the underlying stream,
                // a behavior we don't want.
            }

            public new int Read7BitEncodedInt() {
                return base.Read7BitEncodedInt();
            }
        }

        private sealed class SerializingBinaryWriter : BinaryWriter {
            public SerializingBinaryWriter(Stream input) : base(input) { }

            protected override void Dispose(bool disposing) {
                // Don't call base.Dispose(), since it disposes of the underlying stream,
                // a behavior we don't want.
            }

            public new void Write7BitEncodedInt(int value) {
                base.Write7BitEncodedInt(value);
            }
        }

    }
}
