// cs0502.cs: 'Main.Test()' cannot be both abstract and sealed
// Line: 10

abstract class Base {
    public abstract void Test () {}
}

abstract class Main: Base
{
    public abstract sealed override void Test () {}
}
