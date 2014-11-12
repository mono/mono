// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  UIntPtr
**
**
** Purpose: Platform independent integer
**
** 
===========================================================*/

namespace System {
    
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Diagnostics.Contracts;
    
    [Serializable]
    [CLSCompliant(false)] 
    [System.Runtime.InteropServices.ComVisible(true)]
    public struct UIntPtr : ISerializable
    {
        [SecurityCritical]
        unsafe private void* m_value;

        public static readonly UIntPtr Zero;

                
        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe UIntPtr(uint value)
        {
            m_value = (void *)value;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe UIntPtr(ulong value)
        {
#if WIN32
            m_value = (void*)checked((uint)value);
#else
            m_value = (void *)value;
#endif
        }

        [System.Security.SecurityCritical]
        [CLSCompliant(false)]
        public unsafe UIntPtr(void* value)
        {
            m_value = value;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private unsafe UIntPtr(SerializationInfo info, StreamingContext context) {
            ulong l = info.GetUInt64("value");

            if (Size==4 && l>UInt32.MaxValue) {
                throw new ArgumentException(Environment.GetResourceString("Serialization_InvalidPtrValue"));
            }

            m_value = (void *)l;
        }

#if FEATURE_SERIALIZATION
        [System.Security.SecurityCritical]
        unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();
            info.AddValue("value", (ulong)m_value);
        }
#endif

        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe override bool Equals(Object obj) {
            if (obj is UIntPtr) {
                return (m_value == ((UIntPtr)obj).m_value);
            }
            return false;
        }
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe override int GetHashCode() {
            return unchecked((int)((long)m_value)) & 0x7fffffff;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe uint ToUInt32() {
#if WIN32
            return (uint)m_value;
#else
            return checked((uint)m_value);
#endif
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe ulong ToUInt64() {
            return (ulong)m_value;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe override String ToString() {
            Contract.Ensures(Contract.Result<String>() != null);

#if WIN32
            return ((uint)m_value).ToString(CultureInfo.InvariantCulture);
#else
            return ((ulong)m_value).ToString(CultureInfo.InvariantCulture);
#endif
        }

        public static explicit operator UIntPtr (uint value) 
        {
            return new UIntPtr(value);
        }

        public static explicit operator UIntPtr (ulong value) 
        {
            return new UIntPtr(value);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe static explicit operator uint(UIntPtr value)
        {
#if WIN32
            return (uint)value.m_value;
#else
            return checked((uint)value.m_value);
#endif
        }   

        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe static explicit operator ulong (UIntPtr  value) 
        {
            return (ulong)value.m_value;
        }

        [System.Security.SecurityCritical]
        [CLSCompliant(false)]
        public static unsafe explicit operator UIntPtr (void* value)
        {
            return new UIntPtr(value);
        }

        [System.Security.SecurityCritical]
        [CLSCompliant(false)]
        public static unsafe explicit operator void* (UIntPtr value)
        {
            return value.ToPointer();
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe static bool operator == (UIntPtr value1, UIntPtr value2) 
        {
            return value1.m_value == value2.m_value;
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe static bool operator != (UIntPtr value1, UIntPtr value2) 
        {
            return value1.m_value != value2.m_value;
        }

        public static UIntPtr Add(UIntPtr pointer, int offset) {
            return pointer + offset;
        }

        public static UIntPtr operator +(UIntPtr pointer, int offset) {
            #if WIN32
                return new UIntPtr(pointer.ToUInt32() + (uint)offset);
            #else
                return new UIntPtr(pointer.ToUInt64() + (ulong)offset);
            #endif
        }

        public static UIntPtr Subtract(UIntPtr pointer, int offset) {
            return pointer - offset;
        }

        public static UIntPtr operator -(UIntPtr pointer, int offset) {
            #if WIN32
                return new UIntPtr(pointer.ToUInt32() - (uint)offset);
            #else
                return new UIntPtr(pointer.ToUInt64() - (ulong)offset);
            #endif
        }

        public static int Size
        {
            get
            {
#if WIN32
                return 4;
#else
                return 8;
#endif
            }
        }
       
        [System.Security.SecuritySafeCritical]  // auto-generated
        [CLSCompliant(false)]
        public unsafe void* ToPointer()
        {
            return m_value;
        }

     }
}


