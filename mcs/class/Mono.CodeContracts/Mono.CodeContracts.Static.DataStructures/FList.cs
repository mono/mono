using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures
{
	public class FList<T>
	{
    	private int count;
		private T item;
    	private FList<T> line, buffer;
		
		private FList(T item, FList<T> line)
	    {
	      this.item = item;
	      this.line = line;
	      this.count = FList<T>.Length(line) + 1;
	    }
		
		public T FirstElement
		{
			get{return this.item;}
		}
		
		public FList<T> SequenceElement
		{
			get{return this.line;}
		}
		
		public static FList<T> Reverse(FList<T> list)
	    {
	      if (list == null || list.SequenceElement == null)
	        return list;
	      FList<T> reversed = (FList<T>) null;
	      for (; list != null; list = list.line)
	        reversed  = FList.Cons<T>(reversed , list.item);
	      return reversed ;
	    }
		
		static IEnumerable<T> GetEnumerable(FList<T> list)
	    {
	      FList<T> current = list;
	      while (current != null)
	      {
	        T next = current.FirstElement;
	        current = current.SequenceElement;
	        yield return next;
	      }
	    }
		
		public static bool Contains(FList<T> list, T obj)
	    {
	      if (list == null)
	        return false;
	      if ((object) obj is IEquatable<T>)
	      {
	        if (((IEquatable<T>) (object) obj).Equals(list.item))
	          return true;
	      }
	      else if (obj.Equals((object) l.item))
	        return true;
	      return FList<T>.Contains(list.line, obj);
	    }
		
		public static int Length(FList<T> list)
	    {
	      if (list == null)
	        return 0;
	      else
	        return l.count;
	    }
		
		public static FList<T> Sort(FList<T> list)
	    {
	      return FList<T>.Sort(list, (FList<T>) null);
	    }

	    private static FList<T> Sort(FList<T> list, FList<T> sequence)
	    {
	      if (list == null)
	        return sequence;
	      T first = list.First;
	      FList<T> less;
	      FList<T> more;
	      FList<T>.Partition(list.SequenceElement, head, out less, out more);
	      return FList<T>.Sort(less, FList.Cons<T>(FList<T>.Sort(more, sequence), head));
	    }
		
		public static T[] ToArray<T>(this FList<T> list)
	    {
	      T[] array = new T[FList.Length<T>(list)];
	      int i = 0;
	      for (; list != null; list = list.SequenceElement)
	        array[i++] = list.FirstElement;
	      return array;
	    }

	}
}

