'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
	
Module ExpressionDBNull

   Sub main()
	Dim A as String = "Something"
	Dim B as System.DBNull
	Dim C  = B+A
	Dim D= "Something"
	if D <> C Then
		Throw New Exception (" Unexpected Behavior.System.DBNull should return a literal Nothing. Expected C = Something ")
	End if
	End Sub
End Module
