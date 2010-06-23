// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2010 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.Apache.Org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.Rabbitmq.Com/mpl.Html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd,
//   Cohesive Financial Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created before 22-Nov-2008 00:00:00 GMT by LShift Ltd,
//   Cohesive Financial Technologies LLC, or Rabbit Technologies Ltd
//   are Copyright (C) 2007-2008 LShift Ltd, Cohesive Financial
//   Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd are Copyright (C) 2007-2010 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2010 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2010 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace RabbitMQ.Util
{


  /**
   * A class for allocating integer IDs in a given range.
   */
  public class IntAllocator{

    private IntervalList Base;

    private readonly int[] unsorted;
    private int unsortedCount = 0;

    /**
     * A class representing a list of inclusive intervals
     */
    public class IntervalList{
      public IntervalList(int start, int end){
        this.Start = start;
        this.End = end;
      }

      public int Start;
      public int End;

      // Invariant: If Next != Null then Next.Start > this.End + 1
      public IntervalList Next;

      // Destructively merge two IntervalLists.
      // Invariant: None of the Intervals in the two lists may overlap
      // intervals in this list.
      public static IntervalList Merge(IntervalList x, IntervalList y)
      {
        if(x == null) return y;
        if(y == null) return x;

        if(x.End > y.Start) return Merge(y, x);

        Debug.Assert(x.End != y.Start);

        // We now have x, y non-null and x.End < y.Start.

        if(y.Start == x.End + 1)
        {
          // The two intervals adjoin. Merge them into one and then
          // merge the tails.
          x.End = y.End;
          x.Next = Merge(x.Next, y.Next);
          return x;
        }

        // y belongs in the tail of x.

        x.Next = Merge(y, x.Next);
        return x;
      }

      public static IntervalList FromArray(int[] xs, int length)
      {
        Array.Sort(xs, 0, length);

        IntervalList result = null;
        IntervalList current = null;

        int i = 0;
        while(i < length){
          int start = i;
          while((i < length - 1) && (xs[i + 1] == xs[i] + 1))
            i++;

          IntervalList interval = new IntervalList(xs[start], xs[i]);

          if(result == null)
          {
            result = interval;
            current = interval;
          }
          else
          {
            current.Next = interval;
            current = interval;
          }
          i++;
        }
        return result;
      }
    }

    /**
     * Creates an IntAllocator allocating integer IDs within the inclusive range [start, end]
     */
    public IntAllocator(int start, int end)
    {
      if(start > end) throw new ArgumentException("illegal range [" + start  +", " + end + "]");

      // Fairly arbitrary heuristic for a good size for the unsorted set.
      unsorted = new int[Math.Max(32, (int)Math.Sqrt(end - start))];
      Base = new IntervalList(start, end);
    }

    /**
     * Allocate a fresh integer from the range, or return -1 if no more integers
     * are available. This operation is guaranteed to run in O(1)
     */
    public int Allocate()
    {
      if(unsortedCount > 0){
        return unsorted[--unsortedCount];
      } else if (Base != null) {
        int result = Base.Start++;
        if(Base.Start == Base.End) Base = Base.Next;
        return result;
      } else {
        return -1;
      }
    }

    private void Flush()
    {
        if(unsortedCount > 0)
        {
            Base = IntervalList.Merge(Base, IntervalList.FromArray(unsorted, unsortedCount));
            unsortedCount = 0;
        }
    }


    /**
     * Make the provided integer available for allocation again. This operation
     * runs in amortized O(sqrt(range size)) time: About every sqrt(range size)
     * operations  will take O(range_size + number of intervals) to complete and
     * the rest run in constant time.
     *
     * No error checking is performed, so if you double Free or Free an integer
     * that was not originally Allocated the results are undefined. Sorry.
     */
    public void Free(int id)
    {
      if(unsortedCount >= unsorted.Length)
      {
        Flush();
      }
      unsorted[unsortedCount++] = id;
    }

    public bool Reserve(int id)
    {
      // We always flush before reserving because the only way to determine
      // if an ID is in the unsorted array is through a linear scan. This leads
      // us to the potentially expensive situation where there is a large unsorted
      // array and we reserve several IDs, incurring the cost of the scan each time.
      // Flushing makes sure the array is always empty and does no additional work if
      // reserve is called twice.
      Flush();

      IntervalList current = Base;

      while(current != null)
      {
        if(current.End < id)
        {
          current = current.Next;
          continue;
        }
        else if(current.Start > id)
        {
          return false;
        }
        else if(current.End == id)
        {
          current.End--;
        }
        else if(current.Start == id)
        {
          current.Start++;
        }
        else
        {
          // The ID is in the middle of this interval.
          // We need to split the interval into two.
          IntervalList rest = new IntervalList(id + 1, current.End);
          current.End = id - 1;
          rest.Next = current.Next;
          current.Next = rest;
        }
        return true;
      }
      return false;
    }

  }
}
