'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC30672
REM ErrorMessage: Explicit initialization is not permitted for arrays declared with explicit bounds.

Imports System

Class A
End Class

Module Test
    Public Sub Main()
		dim i(2) as A = {1,1}
    End Sub
End Module
