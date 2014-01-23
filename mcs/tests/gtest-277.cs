using System;
using System.Collections.Generic;

public interface INode<K> : IComparable<K> where K : IComparable<K>
{
        K Key {
		get;
        }
}

public interface IBTNode<C> where C : IBTNode<C> 
{
        C Parent {
		get;
		set;
        }

        C Left {
		get;
		set;
        }

        C Right {
		get;
		set;
        }
}

public interface IBSTNode<K, C> : IBTNode<C>, INode<K> 
	where C : IBSTNode<K, C> where K : IComparable<K>
{
}

public interface IAVLNode<K, C> : IBSTNode<K, C> 
	where C : IAVLNode<K, C> where K : IComparable<K>
{
        int Balance {
		get;
		set;
        }
}

class X
{
	public static void Main ()
	{ }
}
