//
// HighWaterStack.cs
//
// Authors:
// Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

namespace System.Xml {
	// This class represents a stack that minimizes object
	// allocation by using the "high water" method by allowing
	// objects to be reused over time. It would be much easier to do
	// with generics.
	//
	// To push an object one must use the following method:
	//	Scope current = (Scope)scopes.Push ();
	//	if (current == null) {
	//		current = new Scope ();
	//		scopes.AddToTop (current);
	//	}
	//	// set props of current
	
	internal class HighWaterStack : ICloneable {
		public HighWaterStack (int GrowthRate) : this (GrowthRate, int.MaxValue) {}
		
		public HighWaterStack (int GrowthRate, int limit)
		{
			growthrate = GrowthRate;
			used = 0;
			stack = new object [GrowthRate];
			size = GrowthRate;
			limit = limit;
		}
		
		public object Push ()
		{
			if (used == size) {
				if (limit <= used)
					throw new XmlException ("Xml Stack overflow!");
				
				size += growthrate;
				object [] newstack = new object [size];
				
				if (used > 0)
					Array.Copy (stack, 0, newstack, 0, used);
				
				stack = newstack;
				
			}
			return stack [used++];
		}
		
		public object Pop ()
		{
			if (used > 0)
				return stack [--used];
			
			return null;
		}
		
		public object Peek ()
		{
			return used > 0 ? stack[used - 1] : null;
		}
		
		public void AddToTop (object o)
		{
			if (used > 0)
				stack[used - 1] = o;
		}
		
		public object this [int index] {
			get {
				if (index >= 0 && index < used)
					return stack [index];
				
				throw new IndexOutOfRangeException ("index");
			}
			set {
				if (index >= 0 && index < used) stack [index] = value;
					
				throw new IndexOutOfRangeException ("index");
			}
		}
		
		public int Length {
			get { return used;}
		}
		
		public object Clone()
		{
			HighWaterStack ret = (HighWaterStack)this.MemberwiseClone ();
			ret.stack = (object [])this.stack.Clone ();
			return ret;
		}
		
		object [] stack;
		int growthrate;
		int used;
		int size;
		int limit;
	}
}
