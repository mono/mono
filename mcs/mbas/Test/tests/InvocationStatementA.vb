Imports Microsoft.VisualBasic
Imports System

Module InvocationStatementA
    Dim i As Integer = 0

    Delegate Function Df(ByVal a As Integer)

    Sub f1()
        i += 1
        f2()
    End Sub

    Function f2() As Integer
        i += 2
    End Function

    Function f2(ByVal i As Integer) As Integer
        i += 5
        Return i
    End Function

    Function f2(ByVal o As Object) As Boolean
        Return True
    End Function

    Function f3(ByVal j As Integer)
        i += j
    End Function

    Function f4(ByVal j As Integer)
        i += j * 10
    End Function

    Sub main()

        Call f1()
        If i <> 3 Then
            Throw New Exception("#ISB1 - Invocation Statement failed")
        End If

        If f2(i) <> 8 Then
            Throw New Exception("#ISB2 - Invocation Statement failed")
        End If

        If Not f2("Hello") Then
            Throw New Exception("#ISB3 - Invocation Statement failed")
        End If

        If Not f2(2.3D) Then
            Throw New Exception("#ISB4 - Invocation Statement failed")
        End If

        Dim d1, d2 As Df
        d1 = New Df(AddressOf f3)
        d2 = New Df(AddressOf f4)

        d1.Invoke(2) : d2.Invoke(2)
        If i <> 25 Then
            Throw New Exception("#ISB5 - Invocation Statement failed")
        End If

    End Sub

End Module
