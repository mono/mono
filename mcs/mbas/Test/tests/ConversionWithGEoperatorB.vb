'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperatorDecimal

   Sub main()
	Dim a As Decimal = 55.5
	Dim b As Decimal = 66.6
	if a >= b Then
		Throw New Exception ("# >= operators: Failed")
	End if
    End Sub
End Module

