// Compiler options: -t:library
public class FP {
 public delegate U Mapping<T, U>(T obj);

 public static T identity<T>(T obj) { return obj; }
}

