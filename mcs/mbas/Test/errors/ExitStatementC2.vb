'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Module retstmt
	Sub fun()
		Exit sub 10
	End Sub
	Sub Main()
		fun()		
	End Sub
End Module 
