Imports System
Module DoubleTypeCharTest
    Sub Main()
            Dim m As Double
            m = f(20)
            If m <> 20 Then
                Throw New Exception("DoubleTypeCharTest: failed")
            End If
            Exit Sub
    End Sub

    Function f#(ByVal param#)
        f = param
    End Function
End Module
