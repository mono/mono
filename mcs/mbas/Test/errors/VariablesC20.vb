'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC30568
REM ErrorMessage:  Array initializer has 1 too many elements.

Imports System

Class A
End Class

Module Test
    Public Sub Main()
		dim y() As Integer = New Integer(2) {0, 1, 2, 3}
    End Sub
End Module
