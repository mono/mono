'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionXOrOperator

Sub Main()
	Dim A As Integer = 0
	Dim B As Integer = 2 
	Dim R As Integer
	R = A Xor B		'00 Or 10 
	if R <> 2 Then
		throw new Exception("#Error With XOr Operator")
	End if
End Sub
End Module
