// cs01670.cs: params is not valid in this context
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
