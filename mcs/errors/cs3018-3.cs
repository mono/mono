// cs3018-2.cs: `C1.I2' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type `C1'
// Line: 10

using System;
[assembly: CLSCompliant (true)]

public partial class C1
{
    [CLSCompliant (true)]
    public interface I2 {}
}

[CLSCompliant (false)]
public partial class C1
{
}