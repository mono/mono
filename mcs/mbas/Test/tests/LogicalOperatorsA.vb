
Imports System

Module LogicalOperatorsA

    Sub Main()
        Dim a1, a2 As Integer
        a1 = f1() AndAlso f2()
        a2 = a1 OrElse f1()
        Console.WriteLine("{0}  {1}", a1, a2)
    End Sub

    Function f1() As Integer
        Return 1
    End Function

    Function f2() As Boolean
        Return False
    End Function

End Module