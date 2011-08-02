// CS0466: `Base.I.this[params int[]].set': the explicit interface implementation cannot introduce the params modifier
// Line: 10

interface I
{
	int this [int[] p] { set; }
}
class Base : I
{
	int I.this [params int[] p] { 
		set {
		}
	}
}
