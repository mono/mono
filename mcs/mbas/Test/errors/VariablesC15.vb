'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC30684
REM ErrorMessage:  'A' is a type and cannot be used as an expression.

Imports System

Class A
End Class

Module Test
    Public Sub Main()
		dim i as A = A()
    End Sub
End Module
