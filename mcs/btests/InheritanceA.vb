Imports System

' if a class is not derived from any class 
' means it is derived from Object class

Public Class C1
    dim t1 as Type = GetType (C1)

    Function f1 (name as string)
        If t1.name <> name Then
            Throw New System.Exception("#A1 Unexpected result")
       End If
    End Function

    Function f1 (name as string, t as type)
        If t.name <> name Then
            Throw New System.Exception("#A2 Unexpected result")
       End If
    End Function

    Function fn() As Integer
        Return 5
    End Function
End Class

Public Class C2
    Inherits C1
    dim t as Type = GetType (C2)

    Function f2(name as string)
        f1(name, t)
    End Function


End Class

Public Class C3
    Inherits C2

End Class

Module Inheritance
    Sub Main()
        Dim c1 As New C1()
        c1.f1("C1")

        Dim c2 As New C2()
        c2.f1("C1")
        c2.f2("C2")
        c2.fn()

        Dim c3 As New c3()
        c3.f1("C1")
        c3.f2("C2")
        c3.fn()
    End Sub
End Module
