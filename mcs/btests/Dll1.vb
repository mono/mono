REM LineNo: 4
REM ExpectedError: BC30420
REM ErrorMessage: 'Sub Main' was not found in 'Dll1'.

Imports System

Public Class Dll1

Public Shared Sub OutInt(ByRef i As Integer)
    i = 123
End Sub

End Class


