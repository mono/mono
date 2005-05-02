Imports System
Module StringTypeCharTest
    Sub Main()
            Dim m As String
            m = f(20)
            If m <> 20 Then
                Throw New Exception("StringTypeCharTest: failed")
            End If
            Exit Sub
    End Sub

    Function f$(ByVal param$)
        f = param
    End Function
End Module
