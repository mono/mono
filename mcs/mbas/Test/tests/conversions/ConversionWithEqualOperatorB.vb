'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperator1

   Sub main()
	Dim a As Short = 5.5
	Dim b As Integer = 6
	a += 1
	if a = b Then
		Throw New Exception ("# = operators: Failed")
	End if
    End Sub
End Module


