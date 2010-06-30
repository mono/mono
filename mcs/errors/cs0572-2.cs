// CS0572: `meth': cannot reference a type through an expression; try `test.meth' instead
// Line: 8

class test2 : test {
	int meth( bool b )
	{
		return 1;
		base.meth (true);
	}
}

abstract class test {
	public delegate void meth( bool b ) ;
} 
