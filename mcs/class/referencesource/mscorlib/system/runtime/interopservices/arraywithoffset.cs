// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Runtime.InteropServices {

    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public struct ArrayWithOffset
    {
        //private ArrayWithOffset()
        //{
        //    throw new Exception();
        //}
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        public ArrayWithOffset(Object array, int offset)
        {
            m_array  = array;
            m_offset = offset;
            m_count  = 0;
            m_count  = CalculateCount();
        }
    
        public Object GetArray()
        {
            return m_array;
        }
    
        public int GetOffset()
        {
            return m_offset;
        }
    
        public override int GetHashCode()
        {
            return m_count + m_offset;
        }
        
        public override bool Equals(Object obj)
        {
            if (obj is ArrayWithOffset)
                return Equals((ArrayWithOffset)obj);
            else
                return false;
        }

        public bool Equals(ArrayWithOffset obj)
        {
            return obj.m_array == m_array && obj.m_offset == m_offset && obj.m_count == m_count;
        }
    
        public static bool operator ==(ArrayWithOffset a, ArrayWithOffset b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(ArrayWithOffset a, ArrayWithOffset b)
        {
            return !(a == b);
        }

#if MONO
        int CalculateCount()
        {
            Array a = m_array as Array;
            if (a == null)
                throw new ArgumentException ();

            var total = a.Rank * a.Length;
            return total - m_offset;
        }
#else
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern int CalculateCount();
#endif
    
        private Object m_array;
        private int    m_offset;
        private int    m_count;
    }

}
