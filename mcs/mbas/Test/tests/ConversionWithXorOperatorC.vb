'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionXorOperator

Sub Main()
	Dim A As Integer = 3
	Dim B As Boolean = False 
	Dim R As Boolean
	R = A Xor B 	'0101 Or 0 = 0000 
	if R = False Then
		Console.WriteLine("#Error With Xor Operator")
	End if
End Sub
End Module

