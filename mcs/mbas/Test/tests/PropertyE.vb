'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

option Explicit on
Imports System

Class AB
	Public writeonly Property Prop(a as Integer) as Integer
		set
			value = 0
		End set
      End Property
End Class

Module Test
      Public Sub Main()		
	End Sub
End Module

 
