Module Scope
    Dim i As Integer = 5

    Function f1()
        If i <> 5 Then
            Throw New System.Exception("#A1, value of i is not correct")
        End If
    End Function

    Function f2()
        Dim i As Integer = 10
        If i <> 10 Then
            Throw New System.Exception("#A2, value of i is not correct")
        End If
        If Scope.i <> 5 Then
            Throw New System.Exception("#A3, value of i is not correct")
        End If
    End Function

    Sub Main()
        f1()
        f2()
    End Sub
End Module
