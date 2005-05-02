'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30379
REM ErrorMessage: 'Catch' cannot appear after 'Finally' within a 'Try' statement.

Module Test
    Sub Main()
		Try
		Finally
		Catch e as System.Exception
		End Try
    End Sub
End Module
