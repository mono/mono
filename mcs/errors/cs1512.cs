// CS1512: Keyword `base' is not available in the current context
// Line: 11

class Base
{
    private string B () { return "a"; }
}

class E
{
   private string B = base.B ();
}
