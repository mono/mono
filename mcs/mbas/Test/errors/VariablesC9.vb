'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30064
REM ErrorMessage:  'ReadOnly' variable cannot be the target of an assignment

Module M
	Readonly a as integer = 1
	Sub Main()		
		a = a+1
	End Sub
End Module
