'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC30451
REM ErrorMessage: Name 'j' is not declared.

REM LineNo: 19
REM ExpectedError: BC30451
REM ErrorMessage: Name 'k2' is not declared.

Module M
	Sub Main()
		Dim i as Integer
		j as integer
		dim k
		k2 
	End Sub
End Module
