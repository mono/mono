Imports System

Class cls
End Class

Module VariableC
    Dim a As Integer = 10
    Dim b As String = "abc"
    Dim c As cls = New cls()


    Dim e() As Integer = {1, 2, 3, 4, 5}

    Sub Main()
	Console.WriteLine(b)

        If a <> 10 Then
            Throw New Exception("#A1, value mismatch")
        End If
        If b <> "abc" Then
            Throw New Exception("#A2, value mismatch")
        End If



    End Sub
End Module
