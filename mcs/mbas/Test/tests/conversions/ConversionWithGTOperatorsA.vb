'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperator1

   Sub main()
	Dim a As Integer = 5
	Dim b As Double = 5.7
	if a > b Then
		Throw New System.Exception ("#Exception occured as a should be less that b ")
	End if 
    End Sub
End Module
