' if a class is not derived from any class 
' means it is derived from Object class

Public Class C1
    Function f1()
        If GetType(C1).ToString <> "C1" Then
            Throw New System.Exception("#A1 Unexpected result")
        End If
    End Function

    Function fn() As Integer
        Return 5
    End Function
End Class

Public Class C2
    Inherits C1
    Function f2()
        f1()
    End Function


End Class

Public Class c3
    Inherits C2

End Class

Module Inheritance
    Sub Main()
        Dim c1 As New C1()
        c1.f1()

        Dim c2 As New C2()
        c2.f1()
        c2.f2()
        c2.fn()

        Dim c3 As New c3()
        c3.f1()
        c3.f2()
        c3.fn()
    End Sub
End Module