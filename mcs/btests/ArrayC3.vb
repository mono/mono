REM LineNo: 11
REM ExpectedError: BC30415
REM ErrorMessage: 'ReDim' cannot change the number of dimensions of an array

Imports System

Module ArrayC3
    
    Sub Main()
        Dim arr As Integer(,) = {{1, 2}, {3, 4}}
        ReDim arr(2)
    End Sub

End Module

