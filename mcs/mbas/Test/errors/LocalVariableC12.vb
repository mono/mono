'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError:  BC30451
REM ErrorMessage: Name 'i' is not declared.

Option Explicit On
Module F
	Sub Main()	
		i = 10		
	End Sub
End Module
