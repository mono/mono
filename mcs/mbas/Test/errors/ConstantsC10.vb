'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC30209
REM ErrorMessage:  Option Strict On requires all variable declarations to have an 'As' clause.

Option Strict on		
Imports System

Module M
	Sub Main()
		Const a = 10
	End sub
End Module
