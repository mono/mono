'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionAndOperator

Sub Main()
	Dim A As Integer = 10
	Dim B As Integer = 9 
	Dim R As Boolean
	R = A And B 	'1010 And 1001 
	if R = False Then
		Console.WriteLine("#Error With And Operator")
	End if
End Sub
End Module

