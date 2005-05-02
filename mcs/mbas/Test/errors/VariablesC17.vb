'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC30611
REM ErrorMessage: Array dimensions cannot have a negative size.

Imports System

Class A
End Class

Module Test
    Public Sub Main()
		dim i(1,2,-2) as A 
    End Sub
End Module
