'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 20
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected

Imports System

Class A
End Class

Module Test
    Public Sub f(i() as integer)
    End Sub
    Public Sub Main()
		f( New Integer() {0, 1})
		f({0, 1})
    End Sub
End Module

