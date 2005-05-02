'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError:  BC30068
REM ErrorMessage: Expression is a value and therefore cannot be the target of an assignment.

Module F
	Sub fun()
		fun = 10 
	End Sub
	Sub Main()	
		Dim i as Integer = fun()		
	End Sub
End Module
