'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'With End With


Imports System
Module NewTest
	Public Structure Point
		Dim x As Integer
		Dim y As Integer
	End Structure
	
	Sub Main()
		Dim udtPt As POINT
		With udtPt
			.x = 10
			.y = 100
		End With
		if udtpt.x <> 10 then
			Throw New Exception("Unexpected Behavior udtpt.x should be equal to 10 but got " & udtpt.x)
		end if
	End Sub	
End Module
