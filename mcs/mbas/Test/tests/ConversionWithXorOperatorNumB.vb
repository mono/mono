'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionXorOperator

Sub Main()
	Dim A As Integer = 1
	Dim B As Integer = 3 
	Dim R As Boolean
	R = A Xor B 	'01 XOr 11 
	if R = False Then
		Console.WriteLine("#Error With Xor Operator")
	End if
End Sub
End Module
