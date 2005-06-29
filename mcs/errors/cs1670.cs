// cs1670.cs: The `params' modifier is not allowed in anonymous method declaration
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
