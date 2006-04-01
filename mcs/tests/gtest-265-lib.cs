// Compiler options: -t:library

public class A : X
{
        public override void Whoa<T> (object arg)
        {
        }
}

public abstract class X
{
        // virtual is also buggy
        public abstract void Whoa<T> (object arg);
}
