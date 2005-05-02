'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30366
REM ErrorMessage:  Public ReadOnly Property Prop(a As Integer) As Integer' and 'Public WriteOnly Property Prop(a As Integer) As Integer' cannot overload each other because they differ only by 'ReadOnly' or 'WriteOnly'.

option strict
Imports System

Module Test
    Public readonly Property Prop(a as Integer) as Integer
		Get 
		End Get
    End Property
    Public writeonly Property Prop(a as Integer) as Integer
		set
		end set
    End property

    Public Sub Main()		
    End Sub
End Module

