// CS0019: Operator `??' cannot be applied to operands of type `null' and `anonymous method' 
// Line: 8

delegate void D ();

class F
{
	D d = null ?? delegate { };
}
