// cs0528:  `IComparable' is already listed in interface list
// Line: 6

using System;

public interface IX: IComparable, IComparable {
    int CompareTo (object obj);
}
