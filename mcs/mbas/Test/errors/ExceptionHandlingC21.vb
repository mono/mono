'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC30101
REM ErrorMessage: Branching out of a 'Finally' is not valid.

Module Test
Public i as integer
    Sub Main()
		Try		    
		Finally
		    Goto a
		End Try
		a:
		if i<>10 then
			Throw new System.Exception("Finally block not working... Expected 10 but got "&i)
		End if
    End Sub
End Module
