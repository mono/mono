'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC30414
REM ErrorMessage: Value of type '1-dimensional array of Integer' cannot be converted to '2-dimensional array of Integer' because the array types have different numbers of dimensions.

Imports System

Class A
End Class

Module Test
    Public Sub Main()
		Dim a as integer(,) = New Integer() {1,2}
    End Sub
End Module
