REM CompilerOptions: /r:Dll1.dll

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
