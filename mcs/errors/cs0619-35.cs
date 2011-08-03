// CS0619-35: `A.MyEvent' is obsolete: `Do not use it'
// Line: 11

public delegate void MyDelegate();

class A {
    [System.Obsolete("Do not use it", true)]    
    event MyDelegate MyEvent;
    
    public A () {
        MyEvent += new MyDelegate(f);
    }
    
    void f () {}
}