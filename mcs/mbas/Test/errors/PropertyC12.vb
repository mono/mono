'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30210
REM ErrorMessage: Option Strict On requires all function and property declarations to have an 'As' clause.

option strict
Imports System

Module Test
    Public Property Prop()
		get
		end get
		set
		end set
    End Property	
    Public Sub Main()		
    End Sub
End Module

