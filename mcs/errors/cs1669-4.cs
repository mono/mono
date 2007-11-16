// CS1669: __arglist is not valid in this context
// Line: 10

delegate object D (object o);

class C
{
   public void Test ()
   {
      D d = delegate (__arglist) {
		return this;
	  };
   }
}
