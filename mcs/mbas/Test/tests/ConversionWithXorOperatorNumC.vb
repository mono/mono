'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionXorOperator

Sub Main()
	Dim A As Integer = 0
	Dim B As Integer = 2 
	Dim R As Boolean
	R = A Xor B		'00 XOr 10 
	if R = False Then
		throw new Exception("#Error With Xor Operator")
	End if
End Sub
End Module
