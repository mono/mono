'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30642
REM ErrorMessage: 'Optional' and 'ParamArray' cannot be combined.

Imports System

Module MethodDeclarationA
	Sub A1(ParamArray ParamArray args() as Integer)			
	End Sub
	Sub Main()
		A1(New Integer(){1,2,3})
	End Sub
End Module
 
