'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 31
REM ExpectedError: BC30526
REM ErrorMessage:  Property 'Prop' is 'ReadOnly'.

option strict
Imports System

Class A
	Public writeonly Property Prop(a as Integer) as Integer
		set 
		End set
      End Property
End Class

Class B
    Inherits A
    Public shadows readonly Property Prop(a as Integer) as Integer
		get
		end get
    End property
End Class

Module Test
    Public Sub Main()		
	Dim a as B = new B
	B.Prop = 10
    End Sub
End Module

