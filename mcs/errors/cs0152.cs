// cs0152.cs: The label `case X:' already occurs in this switch statement
// Line: 9
class X {
	void f (int i)
	{
		switch (i){
		case 1:
			break;
		case 1:	
			break;
		}
	}
}
