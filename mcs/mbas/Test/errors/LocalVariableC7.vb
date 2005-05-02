'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30260
REM ErrorMessage:  'main' is already declared as 'Private Dim main As Integer' in this module.

Module F
	Dim main as integer
	Sub Main()
	End Sub
End Module
