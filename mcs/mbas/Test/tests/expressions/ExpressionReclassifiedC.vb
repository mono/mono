'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Property group expression A is first reclassified from a property group to a property access
'then reclassified from a property access to a value

Imports System
Module Test
	Sub B(ByVal i As Integer)
	If i <> 0 then 
		Throw New Exception ("Unexpected Behavior. i should be assigned default value of A = 0 but got i=" &i )
	End If
	End Sub
	ReadOnly Property A() As Integer
		Get
		End Get
	End Property
	
	Sub Main()
		B(A)
	End Sub
End Module
