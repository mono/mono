using System;

public interface IA<T> where T : struct {

}

public class B<T> : IA<T> where T:struct {

}

public class MainClass {
        public static void Main () {}

}

