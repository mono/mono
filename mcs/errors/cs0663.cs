// cs0663.cs: 'Test' cannot define overloaded methods which differ only on ref and out
// Line: 6

public interface WrongInterface {
        int Test(out int obj);
        int Test(ref int obj);
}
