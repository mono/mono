' System.InvalidCastException: Cast from string to integer not valid

Imports System

Module Array2

    Sub Main()
        Dim arr As Integer(,) = {{1, "Hello"}, {3, 4}}
        If arr(0, 0) <> 1 Then
            Throw New Exception("#A1")
        End If
    End Sub

End Module

