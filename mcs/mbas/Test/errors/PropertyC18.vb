'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC31060
REM ErrorMessage: Property 'Prop' implicitly defines 'set_Prop', which conflicts with a member of the same name in class 'AB'.

option strict
Imports System

Class AB
	Public writeonly Property Prop(a as Integer) as Integer
		set 
		End set
      End Property
	public sub set_Prop(i as integer)
	end sub
End Class

Module Test
      Public Sub Main()		
	End Sub
End Module

