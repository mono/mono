Imports System
Module LongTypeCharTest
    Sub Main()
            Dim m As Long
            m = f(20)
            If m <> 20 Then
                Throw New Exception("LongTypeCharTest: failed")
            End If
            Exit Sub
    End Sub

    Function f&(ByVal param%)
        f& = param
    End Function
End Module
