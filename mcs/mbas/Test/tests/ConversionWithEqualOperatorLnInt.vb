'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperator1

   Sub main()
	Dim L As Long = 100.555
	Dim I As Integer = 100 
	if L = I  Then
		Throw New Exception ("# Error L can't be equal to I ")
	End if
    End Sub
End Module
