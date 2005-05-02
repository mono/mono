'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 18
REM ExpectedError: BC31062
REM ErrorMessage:  sub 'get_Prop' conflicts with a member implicitly declared for property 'Prop' in class 'AB'.

option strict
Imports System

Class AB
	Public readonly Property Prop(a as Integer) as Integer
		get 
		End get
      End Property
	public sub get_Prop(i as integer)
	end sub
End Class

Module Test
      Public Sub Main()		
	End Sub
End Module

