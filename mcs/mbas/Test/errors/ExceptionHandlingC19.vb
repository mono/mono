'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30381
REM ErrorMessage: 'Finally' can only appear once in a 'Try' statement.

Module Test
    Sub Main()
		Try
		Finally
		Finally
		End Try
    End Sub
End Module
