'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionAndOperator

Sub Main()
	Dim A As Integer = 3
	Dim B As Boolean = True 
	Dim R As Boolean
	R = A And B 	'0000 And -1 = 1111 
	if R = False Then
		Console.WriteLine("#Error With And Operator")
	End if
End Sub
End Module
