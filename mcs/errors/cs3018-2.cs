// cs3018.cs: 'C1.I2' cannot be marked as CLS-Compliant because it is a member of non CLS-Compliant type 'C1'
// Line: 10

using System;
[assembly: CLSCompliant (true)]

[CLSCompliant (false)]
public class C1
{
    [CLSCompliant (true)]
    public interface I2 {}
}
