'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC31082
REM ErrorMessage: 'i' is not a local variable or parameter, and so cannot be used as a 'Catch' variable.

Module Test
Dim i as System.Exception
    Sub Main()
		Try		    
			Catch i
		End Try
    End Sub
End Module
