Imports System
Module DecimalTypeCharTest
    Sub Main()
            Dim m As Decimal
            m = f(20.2D)
            If m <> 20.2D Then
                Throw New Exception("DecimalTypeCharTest: failed")
            End If
            Exit Sub
    End Sub

    Function f@(ByVal param@)
        f = param
    End Function
End Module
