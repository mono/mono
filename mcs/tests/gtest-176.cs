class lis<a> {}

abstract class fn <a,b,r> {
  public abstract r apply (a x,b y);
}

class fn1<a> : fn <lis<a>,lis<a>,lis<a>>
{
  public override lis<a> apply (lis<a> x,lis<a> y)
  {
    return M.RevAppend (x,y);
  }
}

class M {
    public static b FoldLeft<a, b> (a x, b acc, fn<a, b, b> f)
    {
        return f.apply (x, acc);
    }

    public static lis<a> RevAppend<a> (lis <a> x , lis <a> y)  {
      return x;
    }

    public static lis <lis <a>> Concat<a> (lis <lis <a>> l)
    {
      return FoldLeft<lis<lis<a>>, lis<lis<a>>> (l, new lis<lis<a>> (), new
fn1<lis<a>> ());
    }

        public static void Main ()
        {
          M.Concat (new lis<lis<string>> ());
        }
}
