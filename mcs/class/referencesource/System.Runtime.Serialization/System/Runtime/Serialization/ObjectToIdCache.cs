//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Security;

    class ObjectToIdCache
    {
        internal int m_currentCount;
        internal int[] m_ids;
        internal Object[] m_objs;
        bool[] m_isWrapped;

        public ObjectToIdCache()
        {
            m_currentCount = 1;
            m_ids = new int[GetPrime(1)];
            m_objs = new Object[m_ids.Length];
            m_isWrapped = new bool[m_ids.Length];
        }

        public int GetId(object obj, ref bool newId)
        {
            bool isEmpty, isWrapped;
            int position = FindElement(obj, out isEmpty, out isWrapped);
            if (!isEmpty)
            {
                newId = false;
                return m_ids[position];
            }
            if (!newId)
                return -1;

            int id = m_currentCount++;
            m_objs[position] = obj;
            m_ids[position] = id;
            m_isWrapped[position] = isWrapped;
            if (m_currentCount >= (m_objs.Length - 1))
                Rehash();
            return id;
        }

#if NotUsed
        public bool Remove(object obj)
        {
            bool isEmpty;
            int position = FindElement(obj, out isEmpty);
            if(isEmpty)
                return false;
            RemoveAt(position);
            return true;
        }
#endif

        // (oldObjId, oldObj-id, newObj-newObjId) => (oldObj-oldObjId, newObj-id, newObjId )
        public int ReassignId(int oldObjId, object oldObj, object newObj)
        {
            bool isEmpty, isWrapped;
            int position = FindElement(oldObj, out isEmpty, out isWrapped);
            if (isEmpty)
                return 0;
            int id = m_ids[position];
            if (oldObjId > 0)
                m_ids[position] = oldObjId;
            else
                RemoveAt(position);
            position = FindElement(newObj, out isEmpty, out isWrapped);
            int newObjId = 0;
            if (!isEmpty)
                newObjId = m_ids[position];
            m_objs[position] = newObj;
            m_ids[position] = id;
            m_isWrapped[position] = isWrapped;
            return newObjId;
        }

        private int FindElement(object obj, out bool isEmpty, out bool isWrapped)
        {
            isWrapped = false;
            int position = ComputeStartPosition(obj);
            for (int i = position; i != (position - 1); i++)
            {
                if (m_objs[i] == null)
                {
                    isEmpty = true;
                    return i;
                }
                if (m_objs[i] == obj)
                {
                    isEmpty = false;
                    return i;
                }
                if (i == (m_objs.Length - 1))
                {
                    isWrapped = true;
                    i = -1;
                }
            }
            // m_obj must ALWAYS have atleast one slot empty (null).
            Fx.Assert("Object table overflow");
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ObjectTableOverflow)));
        }

        private void RemoveAt(int position)
        {
            int cacheSize = m_objs.Length;
            int lastVacantPosition = position;
            for (int next = (position == cacheSize - 1) ? 0 : position + 1; next != position; next++)
            {
                if (m_objs[next] == null)
                {
                    m_objs[lastVacantPosition] = null;
                    m_ids[lastVacantPosition] = 0;
                    m_isWrapped[lastVacantPosition] = false;
                    return;
                }
                int nextStartPosition = ComputeStartPosition(m_objs[next]);
                // If we wrapped while placing an object, then it must be that the start position wasn't wrapped to begin with
                bool isNextStartPositionWrapped = next < position && !m_isWrapped[next];
                bool isLastVacantPositionWrapped = lastVacantPosition < position;

                // We want to avoid moving objects in the cache if the next bucket position is wrapped, but the last vacant position isn't
                // and we want to make sure to move objects in the cache when the last vacant position is wrapped but the next bucket position isn't
                if ((nextStartPosition <= lastVacantPosition && !(isNextStartPositionWrapped && !isLastVacantPositionWrapped)) ||
                    (isLastVacantPositionWrapped && !isNextStartPositionWrapped))
                {
                    m_objs[lastVacantPosition] = m_objs[next];
                    m_ids[lastVacantPosition] = m_ids[next];
                    // A wrapped object might become unwrapped if it moves from the front of the array to the end of the array
                    m_isWrapped[lastVacantPosition] = m_isWrapped[next] && next > lastVacantPosition;
                    lastVacantPosition = next;
                }
                if (next == (cacheSize - 1))
                {
                    next = -1;
                }
            }
            // m_obj must ALWAYS have atleast one slot empty (null).
            Fx.Assert("Object table overflow");
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ObjectTableOverflow)));
        }

        private int ComputeStartPosition(object o)
        {
            return (RuntimeHelpers.GetHashCode(o) & 0x7FFFFFFF) % m_objs.Length;
        }

        private void Rehash()
        {
            int size = GetPrime(m_objs.Length * 2);
            int[] oldIds = m_ids;
            object[] oldObjs = m_objs;
            m_ids = new int[size];
            m_objs = new Object[size];
            m_isWrapped = new bool[size];

            for (int j = 0; j < oldObjs.Length; j++)
            {
                object obj = oldObjs[j];
                if (obj != null)
                {
                    bool found, isWrapped;
                    int position = FindElement(obj, out found, out isWrapped);
                    m_objs[position] = obj;
                    m_ids[position] = oldIds[j];
                    m_isWrapped[position] = isWrapped;
                }
            }
        }

        static int GetPrime(int min)
        {

            for (int i = 0; i < primes.Length; i++)
            {
                int prime = primes[i];
                if (prime >= min) return prime;
            }

            //outside of our predefined table. 
            //compute the hard way. 
            for (int i = (min | 1); i < Int32.MaxValue; i += 2)
            {
                if (IsPrime(i))
                    return i;
            }
            return min;
        }

        static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return (candidate == 2);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        internal static readonly int[] primes = 
        {
            3, 7, 17, 37, 89, 197, 431, 919, 1931, 4049, 8419, 17519, 36353, 
            75431, 156437, 324449, 672827, 1395263, 2893249, 5999471, 
        };

    }
}

