// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Wrapper represeting an arbitrary property of a CNG key or provider
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public struct CngProperty : IEquatable<CngProperty> {
        private string m_name;
        private CngPropertyOptions m_propertyOptions;
        private byte[] m_value;
        private int? m_hashCode;

        public CngProperty(string name, byte[] value, CngPropertyOptions options) {
            if (name == null)
                throw new ArgumentNullException("name");
            // @


            m_name = name;
            m_propertyOptions = options;
            m_hashCode = null;

            if (value != null) {
                m_value = value.Clone() as byte[];
            }
            else {
                m_value = null;
            }
        }

        /// <summary>
        ///     Name of the property
        /// </summary>
        public string Name {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                return m_name;
            }
        }

        /// <summary>
        ///     Options used to set / get the property
        /// </summary>
        public CngPropertyOptions Options {
            get { return m_propertyOptions;  }
        }

        /// <summary>
        ///     Direct value of the property -- if the value will be returned to user code or modified, use
        ///     GetValue() instead.
        /// </summary>
        internal byte[] Value {
            get { return m_value; }
        }

        /// <summary>
        ///     Contents of the property
        /// </summary>
        /// <returns></returns>
        public byte[] GetValue() {
            byte[] value = null;

            if (m_value != null) {
                value = m_value.Clone() as byte[];
            }

            return value;
        }

        public static bool operator ==(CngProperty left, CngProperty right) {
            return left.Equals(right);
        }

        public static bool operator !=(CngProperty left, CngProperty right) {
            return !left.Equals(right);
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is CngProperty)) {
                return false;
            }

            return Equals((CngProperty)obj);
        }

        public bool Equals(CngProperty other) {
            //
            // We will consider CNG properties equal only if the name, options and value are all also equal
            //

            if (!String.Equals(Name, other.Name, StringComparison.Ordinal)) {
                return false;
            }

            if (Options != other.Options) {
                return false;
            }

            if (m_value == null) {
                return other.m_value == null;
            }
            if (other.m_value == null) {
                return false;
            }

            if (m_value.Length != other.m_value.Length) {
                return false;
            }

            for (int i = 0; i < m_value.Length; i++) {
                if (m_value[i] != other.m_value[i]) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() {
            if (!m_hashCode.HasValue) {
                int hashCode = Name.GetHashCode() ^ Options.GetHashCode();

                // The hash code for a byte is just the value of that byte. Since this will only modify the
                // lower bits of the hash code, we'll xor each byte into different sections of the hash code
                if (m_value != null) {
                    for (int i = 0; i < m_value.Length; i++) {
                        // Shift each byte forward by one byte, so that every 4 bytes has to potential to update
                        // each of the calculated hash code's bytes.
                        int shifted = (int)(m_value[i] << ((i % 4) * 8));
                        hashCode ^= shifted;
                    }
                }

                m_hashCode = hashCode;
            }

            return m_hashCode.Value;
        }
    }

    /// <summary>
    ///     Strongly typed collection of CNG properties
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class CngPropertyCollection : Collection<CngProperty> {
    }
}
