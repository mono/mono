'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30754
REM ErrorMessage: 'GoTo a' is not valid because 'a' is inside a 'Try', 'Catch' or 'Finally' statement that does not contain this statement.

Module Test
    Sub Main()
		Try		 
			goto a   
			Catch 
			a:
		End Try
    End Sub
End Module
