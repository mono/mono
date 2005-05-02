'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC30311
REM ErrorMessage: Value of type '1-dimensional array of Date' cannot be converted to 'Integer'.

Imports System

Module MethodDeclarationA
	Sub A1 ( ByVal ParamArray j() as Integer )			
	End Sub
	Sub Main()
		Dim ar as Date() = {}
		A1(ar) 
	End Sub
End Module
 
