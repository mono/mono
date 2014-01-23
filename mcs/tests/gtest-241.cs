abstract public class a
  {
    public abstract void func<T>(ref T arg);
  }
  public class b : a
  {
     public override void func<T>(ref T arg)
     {
     }
  }
class main {
	public static void Main () {}
}
