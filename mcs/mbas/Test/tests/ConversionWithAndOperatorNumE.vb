'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionAndOperator

Sub Main()
	Dim A As Integer = 1
	Dim B As Integer = 3 
	Dim R As Boolean
	R = A And B 	'01 And 11 
	if R = False Then
		throw new Exception("#Error With And Operator")
	End if
End Sub
End Module

