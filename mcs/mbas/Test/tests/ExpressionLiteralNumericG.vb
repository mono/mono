'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim BigDec As Decimal = 9223372036854775808D
		Dim BigDoub As Double 
		BigDoub =BigDec
		Dim BigNew = BigDoub
		If BigNew <> BigDec Then
			Throw New Exception ("Error With Given Expression. BigNew should be Equal to BigDec")
		End If
	End Sub
End Module

