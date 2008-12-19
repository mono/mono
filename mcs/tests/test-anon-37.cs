using System;

public class DelegateInit {
    public delegate void FooDelegate();

    public static readonly FooDelegate _print =
        delegate() {
            Console.WriteLine("delegate!");
        };

    public static void Main(string[] args) {
        _print();
    }
}