using System;

[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public delegate uint MyDelegate();

[CLSCompliant(false)]
public interface IFake {
        [CLSCompliant(true)]
        long AA(long arg);
        [CLSCompliant(false)]
        ulong BB { get; }
        //[CLSCompliant(false)]
        //sbyte this[ulong I] { set; }
        [CLSCompliant(false)]
        event MyDelegate MyEvent;
}

[CLSCompliant(false)]
internal interface I {
        [CLSCompliant(false)]
        void Foo();

        [CLSCompliant(true)]
        ulong this[int indexA] { set; }
}

interface I2 {
        int Test(int arg1, bool arg2);
}

public class CLSClass {
        [CLSCompliant(false)]
        public delegate uint MyDelegate();    
    
        public static void Main() {}
}
public class CLSClass_2 {
    [CLSCompliant (false)]
    public CLSClass_2(int[,,] b) {
    }

    public CLSClass_2(int[,] b) {
    }

}

public class X1 {
    [CLSCompliant (false)]
    public void M2 (int i) {}
}

public class X2: X1 {
    public void M2 (ref int i) {}
}
