'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30210
REM ErrorMessage: Option Strict On requires all function and property declarations to have an 'As' clause.

Option Strict On
Imports System

Module MethodA
	Function A()
		return 10
	End Function
	Sub Main()
		Dim a as Object = A()		
	End Sub
End Module
 
