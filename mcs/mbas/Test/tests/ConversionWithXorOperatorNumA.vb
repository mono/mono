'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionXorOperator

Sub Main()
	Dim A As Integer = 10
	Dim B As Integer = 9 
	Dim R As Integer
	R = A Xor B 	'1010 XOr 1001 
	if R <> 3 Then
		throw new Exception("#Error With Xor Operator")
	End if
End Sub
End Module
