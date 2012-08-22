using System;

namespace Mono.CodeContracts.Static.DataStructures
{
	struct Tuple<One, Two, Three>
	{
		public readonly One one;
		public readonly Two two;
		public readonly Three three;
		
		public Tuple(One first, Two second, Three third)
	    {
	      this.one = first;
	      this.two = second;
	      this.three = third;
	    }
		
		public override string ToString()
	    {
	      return string.Format("({0},{1},{2})", 
			                     (object) this.one == null ? (object) "<null>" : (object) this.one.ToString(), 
			                     (object) this.two == null ? (object) "<null>" : (object) this.two.ToString(), 
			                     (object) this.three == null ? (object) "<null>" : (object) this.three.ToString());
	    }
		
		public bool Equals(CortegeForThreeParams<One, Two, Three> anotherItem)
		{
			bool trigger_1, trigger_2, trigger_3;
			trigger_1 = (object) this.one is IEquatable<One> ? ((IEquatable<One>) (object) this.one).Equals(anotherItem.One) : object.Equals((object) this.one, (object) anotherItem.One);
			
			if (!flag1)return trigger_1;
			else
			{
				trigger_2 = (object) this.two is IEquatable<Two> ? ((IEquatable<Two>) (object) this.two).Equals(anotherItem.Two) : object.Equals((object) this.two, (object) anotherItem.Two);
				if(!trigger_2) return trigger_2;
				else
				{
					return trigger_3 = (object) this.three is IEquatable<Tree> ? ((IEquatable<Three>) (object) this.three).Equals(anotherItem.Three) : object.Equals((object) this.three, (object) anotherItem.Three);
				}
			}
		}
	}
}

