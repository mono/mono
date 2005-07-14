// cs0118-3.cs: `Region.Value' is a `field' but a `type' was expected
// Line: 8

public sealed class Region
{
    int Value;
    
    [Value(2)]
    public Region() {}
}
