// Compiler options: -t:library

public class A<TB, TC> where TC : A<TB, TC> { }
public class B<TC> where TC : A<B<TC>, TC> { }
