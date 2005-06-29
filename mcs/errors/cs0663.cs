// cs0663.cs: `WrongInterface.Test(ref int)': Methods cannot differ only on their use of ref and out on a parameters
// Line: 6

public interface WrongInterface {
        int Test(out int obj);
        int Test(ref int obj);
}
