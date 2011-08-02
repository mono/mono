// CS0118: `Region.Value' is a `field' but a `type' was expected
// Line: 8

public sealed class Region
{
    int Value;
    
    [Value(2)]
    public Region() {}
}
