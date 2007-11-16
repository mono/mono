// CS1670: The `params' modifier is not allowed in current context
// Line: 10

delegate object D (params object[] args);

class C
{
   public void Test ()
   {
      D d = delegate (params object[] args) {
		return this;
	  };
   }
}
