' Positive Test
' Test labels in functions
Imports System

Module labelB


    Function Abs(ByVal x As Integer) As Integer

        If x >= 0 Then
            GoTo x
        End If

        x = -x
x:      Return x

    End Function


    Sub Main()
        Dim x As Integer, y As Integer
        x = Abs(-1)
        y = Abs(1)

        If x <> 1 Then
            Throw New Exception("#Lbl1")
        End If
        If y <> 1 Then
            Throw New Exception("#Lbl2")
        End If


    End Sub


End Module
