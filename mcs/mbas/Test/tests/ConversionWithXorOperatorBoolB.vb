'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionXorOperator

Sub Main()
	Dim A As Integer = 124
	Dim B As Byte = 254   
	Dim R As Boolean
	R = (B < A) Xor (A < B) 
	if R = False Then
		throw new Exception("#Error With Xor Operator")
	End if
End Sub
End Module
