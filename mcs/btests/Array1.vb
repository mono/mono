'Unhandled Exception: System.ArrayTypeMismatchException: 'ReDim' can 
' only change the rightmost dimension.

Imports System

Module Array1

    Sub Main()
        Dim arr As Integer(,) = {{1, 2}, {3, 4}}
        ReDim Preserve arr(3, 3)
        arr(2, 2) = 12
    End Sub
End Module

