'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC30064
REM ErrorMessage:  'ReadOnly' variable cannot be the target of an assignment

Module M
	Class C
		Readonly Shared Public i as integer = 10
	End Class
	Sub Main()	
		C.i = 1
	End Sub
End Module
