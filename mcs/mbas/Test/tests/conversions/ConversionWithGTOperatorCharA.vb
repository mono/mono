'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperatorChar

   Sub main()
	Dim A As Char = "a"
	Dim B As Char = "b"
	if A > B Then
		Throw New Exception ("Exception occured Value of a Should be less than b ")
	End if
    End Sub
End Module
