//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;
    using System.Collections.Generic;

    struct ObjectReferenceStack
    {
        const int MaximumArraySize = 16;
        const int InitialArraySize = 4;

        int count;
        object[] objectArray;
        bool[] isReferenceArray;
        Dictionary<object, object> objectDictionary;

        internal void Push(object obj)
        {
            if (objectArray == null)
            {
                objectArray = new object[InitialArraySize];
                objectArray[count++] = obj;
            }
            else if (count < MaximumArraySize)
            {
                if (count == objectArray.Length)
                    Array.Resize<object>(ref objectArray, objectArray.Length * 2);
                objectArray[count++] = obj;
            }
            else
            {
                if (objectDictionary == null)
                    objectDictionary = new Dictionary<object, object>();

                objectDictionary.Add(obj, null);
                count++;
            }
        }

        internal void EnsureSetAsIsReference(object obj)
        {
            if (count == 0)
                return;
            if (count > MaximumArraySize)
            {
                if (objectDictionary == null)
                {
                    Fx.Assert("Object reference stack in invalid state");
                }
                objectDictionary.Remove(obj);
            }
            else
            {
                if ((objectArray != null) && objectArray[count - 1] == obj)
                {
                    if (isReferenceArray == null)
                    {
                        isReferenceArray = new bool[InitialArraySize];
                    }
                    else if (count == isReferenceArray.Length)
                    {

                        Array.Resize<bool>(ref isReferenceArray, isReferenceArray.Length * 2);
                    }
                    isReferenceArray[count - 1] = true;
                }
            }
        }

        internal void Pop(object obj)
        {
            if (count > MaximumArraySize)
            {
                if (objectDictionary == null)
                {
                    Fx.Assert("Object reference stack in invalid state");
                }
                objectDictionary.Remove(obj);
            }
            count--;
        }

        internal bool Contains(object obj)
        {
            int currentCount = count;
            if (currentCount > MaximumArraySize)
            {
                if (objectDictionary != null && objectDictionary.ContainsKey(obj))
                    return true;
                currentCount = MaximumArraySize;
            }
            for (int i = (currentCount - 1); i >= 0; i--)
            {
                if (Object.ReferenceEquals(obj, objectArray[i]) && isReferenceArray != null && !isReferenceArray[i])
                    return true;
            }
            return false;
        }

        internal int Count
        {
            get { return count; }
        }

    }

}

