REM LineNo: 6
REM ExpectedError: BC30466
REM ErrorMessage: Namespace or type 'Dll1' for the Imports 'Dll1' cannot be found. Name 'Dll' is not declared.

Imports System
Imports Dll1

Module Test
Sub Main
    Dim i As Integer
    Dll1.OutInt(i)
    If i <> 123 Then
        Throw New Exception("#A1: Wrong value returned: " & i.ToString())
    End If
End Sub
End Module
