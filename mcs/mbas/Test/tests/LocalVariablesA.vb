Imports System

Module LocalVariablesA

    Function swap(ByVal a As Integer, ByVal b As Integer) As Integer
        Dim c As Integer
        c = a
        a = b
        b = c
        Return 0
    End Function

    ' Local variable having same name as Sub containing it
    Sub f2()
        Dim f2 As Integer = 1
        f2 = f2 + 1
        Console.WriteLine(f2)
    End Sub

    Sub main()
        Dim a, b As Integer
        a = 10 : b = 32
        Console.WriteLine("a: {0}  b: {1}", a, b)
        swap(a, b)
        Console.WriteLine("a: {0}  b: {1}", a, b)
        f2()
    End Sub

End Module