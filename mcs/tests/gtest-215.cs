public class R {}
public class A<T> where T : R {}
public partial class D : A<R> {}
class MainClass { public static void Main () {} }
