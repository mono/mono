'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30651
REM ErrorMessage:  Property parameters cannot be declared 'ByRef'.

option strict
Imports System

Module Test
    Public Property Prop(ByRef a as Integer) as Integer
		Get 
		End Get
		Set
		End set
    End property

    Public Sub Main()		
    End Sub
End Module

