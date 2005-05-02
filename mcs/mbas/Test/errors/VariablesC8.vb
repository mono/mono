'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30247
REM ErrorMessage:  'ReadOnly' is not valid on a local variable declaration

Module M
	Sub Main()
		Readonly a as integer = 1
	End Sub
End Module
