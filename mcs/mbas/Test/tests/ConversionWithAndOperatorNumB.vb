'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionAndOperator

Sub Main()
	Dim A As Integer = 3
	Dim B As Boolean = False 
	Dim R As Boolean
	R = A And B 	'0101 And 0 = 0000 
	if R = True Then
		throw new Exception("#Error With And Operator")
	End if
End Sub
End Module

