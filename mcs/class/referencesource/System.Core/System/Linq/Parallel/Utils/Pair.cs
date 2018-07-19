// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Pair.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// A pair just wraps two bits of data into a single addressable unit. This is a
    /// value type to ensure it remains very lightweight, since it is frequently used
    /// with other primitive data types as well.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    internal struct Pair<T, U>
    {

        // The first and second bits of data.
        internal T m_first;
        internal U m_second;

        //-----------------------------------------------------------------------------------
        // A simple constructor that initializes the first/second fields.
        //

        public Pair(T first, U second)
        {
            m_first = first;
            m_second = second;
        }

        //-----------------------------------------------------------------------------------
        // Accessors for the left and right data.
        //

        public T First
        {
            get { return m_first; }
            set { m_first = value; }
        }

        public U Second
        {
            get { return m_second; }
            set { m_second = value; }
        }

    }
}
