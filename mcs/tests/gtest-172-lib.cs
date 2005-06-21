// Compiler options: -t:library
public class A <T> {
  public class Nil : A <T> {
     public static Nil _N_constant_object = new Nil ();
  }
}
