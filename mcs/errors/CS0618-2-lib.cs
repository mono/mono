using System;

[System.Obsolete ("Class is obsolete", false)]
public class ObsoleteDispose: IDisposable
{
    public void Dispose () {}
    public static ObsoleteDispose Factory {
        get {
            return new ObsoleteDispose ();
        }
    }
}
