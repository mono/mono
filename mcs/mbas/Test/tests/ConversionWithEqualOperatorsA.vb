'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperator1

   Sub main()
	Dim a As Boolean = True
	Dim b As Double = 5.7
	if a = b Then
		Throw New System.Exception ("# Exceptions occured Equal To operator not working ")
	End if 
    End Sub
End Module

