// CS8147: `X.this[int]': property and indexer which return by reference cannot have set accessors
// Line: 6

public class X
{
	ref string this [int arg] { 
		set {

		}
		get {

		}
	}
}