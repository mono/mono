'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError:  BC30801
REM ErrorMessage: : Labels that are numbers must be followed by colons.

Module F
	Function fun()
		10 = fun 
		fun = 10
	End Function
	Sub Main()	
		Dim i as Integer = fun()		
	End Sub
End Module
