REM LineNo: 8
REM ExpectedError: BC31087
REM ErrorMessage: Array modifiers cannot be specified on both a variable and its type.

Imports System

Module M
    Dim a() as Long()

    Sub Main ()
    End Sub
End Module

