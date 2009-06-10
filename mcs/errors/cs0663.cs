// CS0663: Overloaded method `WrongInterface.Test(ref int)' cannot differ on use of parameter modifiers only
// Line: 6

public interface WrongInterface {
        int Test(out int obj);
        int Test(ref int obj);
}
