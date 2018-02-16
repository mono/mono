using System;
using System.Runtime.InteropServices;

public struct EmptyStruct {
}

[StructLayout(LayoutKind.Sequential)]
public struct EmptySequentialStruct {
}

[StructLayout(LayoutKind.Explicit)]
public struct EmptyExplicitStruct {
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct EmptySequentialPackStruct {
}

[StructLayout(LayoutKind.Explicit, Pack = 4)]
public struct EmptyExplicitPackStruct {
}

[StructLayout(LayoutKind.Explicit, Size = 0)]
public struct EmptyExplicitSize0Struct {
}

[StructLayout(LayoutKind.Explicit, Size = 1)]
public struct EmptyExplicitSize1Struct {
}

class Program {
    private static unsafe void CheckSize<T> (int expected, ref int exitCode) {
        var t = typeof(T);
        var actualSize = Marshal.SizeOf(t);

        Console.WriteLine($"Marshal.SizeOf({t.Name}) == {actualSize}, expected {expected}");

        if (actualSize != expected)
            exitCode += 1;

        var tempArray = new T[2];
        var pin = GCHandle.Alloc(tempArray, GCHandleType.Pinned);

        var offsetZero = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(tempArray, 0);
        var offsetOne = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(tempArray, 1);
        var distanceBetweenElements = offsetOne - offsetZero;

        pin.Free();

        Console.WriteLine($"{t.Name} (arr[1] - arr[0]) == {distanceBetweenElements}, expected {expected}");

        if (distanceBetweenElements != expected)
            exitCode += 1;
    }

    // https://bugzilla.xamarin.com/show_bug.cgi?id=18941
    // Marshal.SizeOf should never report 0, even for empty structs or structs with Size=0 attribute
    public static int Main () {
        int exitCode = 0;

        CheckSize<EmptyStruct>(1, ref exitCode);
        CheckSize<EmptySequentialStruct>(1, ref exitCode);
        CheckSize<EmptyExplicitStruct>(1, ref exitCode);
        CheckSize<EmptySequentialPackStruct>(1, ref exitCode);
        CheckSize<EmptyExplicitPackStruct>(1, ref exitCode);
        CheckSize<EmptyExplicitSize0Struct>(1, ref exitCode);
        CheckSize<EmptyExplicitSize1Struct>(1, ref exitCode);

        return exitCode;
    }
}