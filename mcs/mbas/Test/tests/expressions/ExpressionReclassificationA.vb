'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionReclassify

   Sub main()
		Dim A as Integer = 10 
		Dim B as Integer = 11
		B = A 
		If B <> 10 Then
			Throw New Exception (" Unexpected Behavior of the Expression. B should be reclassified as A. Expected B = 10 but got B=" &B)
		End if
	End Sub
End Module
