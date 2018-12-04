using System;
using MS.Internal.WindowsBase;

namespace MS.Utility
{
    //
    // ItemStructList<T>
    //

    [FriendAccessAllowed] // Built into Base, also used by Framework.
    internal struct ItemStructList<T>
    {
        public ItemStructList(int capacity)
        {
            List = new T[capacity];
            Count = 0;
        }


        //
        // Non-lock-required Read methods
        // (Always safe to call when locking "safe" write operations are used)
        //

        public T[] List;
        public int Count;

        public void EnsureIndex(int index)
        {   
            int delta = (index + 1) - Count;
            if (delta > 0)
            {
                Add(delta);
            }
        }

        public bool IsValidIndex(int index)
        {
            return (index >= 0 && index < Count);
        }

        public int IndexOf(T value)
        {
            int index = -1;
            
            for (int i = 0; i < Count; i++)
            {
                if (List[i].Equals(value))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public bool Contains(T value)
        {
            return (IndexOf(value) != -1);
        }


        //
        // Lock-required Write operations
        // "Safe" methods for Reader lock-free operation
        //

        // Increase size by one, new value is provided
        public void Add(T item)
        {
            // Add without Count adjustment (incr Count after valid item added)
            int index = Add(1, false);
            List[index] = item;
            Count++;
        }

        // Increase size by one, new value is provided
        public void Add(ref T item)
        {
            // Add without Count adjustment (incr Count after valid item added)
            int index = Add(1, false);
            List[index] = item;
            Count++;
        }

        // Increase size by one, new value is default value
        public int Add()
        {
            return Add(1, true);
        }

        // Increase size of array by delta, fill with default values
        public int Add(int delta)
        {
            return Add(delta, true);
        }
        
        // Increase size of array by delta, fill with default values
        // Allow disable of automatic Count increment so that cases where
        // non-default values are to be added to the list can be done before
        // count is changed. This is important for non-locking scenarios
        // (i.e. count is adjusted after array size changes)
        private int Add(int delta, bool incrCount)
        {
            if (List != null)
            {
                if ((Count + delta) > List.Length)
                {
                    T[] newList = new T[Math.Max(List.Length * 2, Count + delta)];
                    List.CopyTo(newList, 0);
                    List = newList;
                }
            }
            else
            {
                List = new T[Math.Max(delta, 2)];
            }

            // New arrays auto-initialized to default entry values
            // Any resued entried have already been cleared out by Remove or Clear

            int index = Count;

            // Optional adjustment of Count
            if (incrCount)
            {
                // Adjust count after resize so that array bounds and data
                // are never invalid (good for locking writes without synchronized reads)
                Count += delta;
            }

            return index;
        }

        public void Sort()
        {
            if (List != null)
            {
                Array.Sort(List, 0, Count);
            }
        }

        public void AppendTo(ref ItemStructList<T> destinationList)
        {
            for (int i = 0; i < Count; i++)
            {
                destinationList.Add(ref List[i]);
            }
        }

        public T[] ToArray()
        {
            T[] array = new T[Count];
            Array.Copy(List, 0, array, 0, Count);

            return array;
        }


        //
        // Lock-required Write operations
        // "UNSafe" methods for Reader lock-free operation
        //
        // If any of these methods are called, the entire class is considered
        // unsafe for Reader lock-free operation for that point on (meaning
        // Reader locks must be taken)
        //

        public void Clear()
        {
            // Return now unused entries back to default
            Array.Clear(List, 0, Count);

            Count = 0;
        }

        public void Remove(T value)
        {
            int index = IndexOf(value);
            if (index != -1)
            {
                // Shift entries down
                Array.Copy(List, index + 1, List, index, (Count - index - 1));                

                // Return now unused entries back to default
                Array.Clear(List, Count - 1, 1);
        
                Count--;
            }
        }
    }
}

