'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Method pointer reclassified.

Imports System
Delegate Sub A(ByVal c As Integer) 
Module Test 
	Sub B(ByVal c As Integer)
		if c <> 100 then 
			Throw New Exception ("Unexpected Behavior C should be equal to 100 but got c =" &c)
		End If
	End Sub
	Sub Main() 
		Dim delg As A 
		delg = New A(AddressOf B) 
		delg.Invoke(100)
	End Sub 
End Module
