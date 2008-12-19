delegate IInterface testDelegate(concrete x);

interface IInterface {
}

class concrete : IInterface {
}

class Program {
   private concrete getConcrete(IInterface z) {
      return new concrete();
   }
   
   public static void Main(string[] args) {
      Program p = new Program();
      testDelegate x = new testDelegate(p.getConcrete);
   }
}