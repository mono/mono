//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;

    class EmptyArray<T>
    {
        static T[] instance;

        EmptyArray()
        {
        }

        internal static T[] Instance
        {
            get
            {
                if (instance == null)
                    instance = new T[0];
                return instance;
            }
        }

        internal static T[] Allocate(int n)
        {
            if (n == 0)
                return Instance;
            else
                return new T[n];
        }

        internal static T[] ToArray(IList<T> collection)
        {
            if (collection.Count == 0)
            {
                return EmptyArray<T>.Instance;
            }
            else
            {
                T[] array = new T[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
        }

        internal static T[] ToArray(SynchronizedCollection<T> collection)
        {
            lock (collection.SyncRoot)
            {
                return EmptyArray<T>.ToArray((IList<T>)collection);
            }
        }
    }

    class EmptyArray
    {
        static object[] instance = new object[0];

        EmptyArray()
        {
        }

        internal static object[] Instance
        {
            get
            {
                return instance;
            }
        }

        internal static object[] Allocate(int n)
        {
            if (n == 0)
                return Instance;
            else
                return new object[n];
        }
    }
}
