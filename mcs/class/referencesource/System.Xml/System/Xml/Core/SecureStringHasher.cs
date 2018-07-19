//------------------------------------------------------------------------------
// <copyright file="SecureStringHasher.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security;
#if !SILVERLIGHT
using System.Security.Permissions;
#endif

namespace System.Xml {

    // SecureStringHasher is a hash code provider for strings. The hash codes calculation starts with a seed (hasCodeRandomizer) which is usually
    // different for each instance of SecureStringHasher. Since the hash code depend on the seed, the chance of hashtable DoS attack in case when 
    // someone passes in lots of strings that hash to the same hash code is greatly reduced.
    // The SecureStringHasher implements IEqualityComparer for strings and therefore can be used in generic IDictionary.
    internal class SecureStringHasher : IEqualityComparer<String> {
        [SecurityCritical]
        delegate int HashCodeOfStringDelegate(string s, int sLen, long additionalEntropy);
        
        // Value is guaranteed to be null by the spec.
        // No explicit assignment because it will require adding SecurityCritical on .cctor
        // which could hurt the performance
        [SecurityCritical]
        static HashCodeOfStringDelegate hashCodeDelegate;

        int hashCodeRandomizer;

        public SecureStringHasher() {
            this.hashCodeRandomizer = Environment.TickCount;
        }

#if false // This is here only for debugging of hashing issues
        public SecureStringHasher( int hashCodeRandomizer ) {
            this.hashCodeRandomizer = hashCodeRandomizer;
        }
#endif

        public bool Equals( String x, String y ) {
            return String.Equals( x, y, StringComparison.Ordinal );
        }

        [SecuritySafeCritical]
        public int GetHashCode( String key ) {
            if (hashCodeDelegate == null) {
                hashCodeDelegate = GetHashCodeDelegate();
            }
            return hashCodeDelegate(key, key.Length, hashCodeRandomizer);
        }
        
        [SecurityCritical]
        private static int GetHashCodeOfString( string key, int sLen, long additionalEntropy ) {
            int hashCode = unchecked((int)additionalEntropy);
            // use key.Length to eliminate the rangecheck
            for ( int i = 0; i < key.Length; i++ ) {
                hashCode += ( hashCode << 7 ) ^ key[i];
            }
            // mix it a bit more
            hashCode -= hashCode >> 17; 
            hashCode -= hashCode >> 11; 
            hashCode -= hashCode >> 5;
            return hashCode;
        }
        
        [SecuritySafeCritical]
#if !SILVERLIGHT
        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
#endif
        private static HashCodeOfStringDelegate GetHashCodeDelegate() {
            // If we find the Marvin hash method, we use that
            // Otherwise, we use the old string hashing function.
 
            MethodInfo getHashCodeMethodInfo = typeof(String).GetMethod("InternalMarvin32HashString", BindingFlags.NonPublic | BindingFlags.Static);
            if (getHashCodeMethodInfo != null) {
                return (HashCodeOfStringDelegate)Delegate.CreateDelegate(typeof(HashCodeOfStringDelegate), getHashCodeMethodInfo);
            }
     
            // This will fall through and return a delegate to the old hash function
            Debug.Assert(false, "Randomized hashing is not supported.");

            return new HashCodeOfStringDelegate(GetHashCodeOfString);
        }
    }
}
