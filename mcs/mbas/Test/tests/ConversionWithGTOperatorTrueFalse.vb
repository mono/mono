'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperator1

   Sub main()
	Dim T As Boolean = True
	Dim F As Boolean = False
	if T > F Then
		Throw New Exception ("Exception occured Value of True Should be less than False ")
	End if
    End Sub
End Module




