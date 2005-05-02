'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30647
REM ErrorMessage: 'Return' statement in a Sub or a Set cannot return a value.

Module retstmt
	Sub fun()
		return 10
	End Sub
	Sub Main()
		fun()		
	End Sub
End Module 
