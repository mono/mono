'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC31087
REM ErrorMessage: Array modifiers cannot be specified on both a variable and its type.

Imports System

Module Test
    Sub Main()
        dim b(,) As Integer(,) = {}
        dim b1(,) As Integer = {}
        dim b2 As Integer(,) = {}
    End Sub
End Module
