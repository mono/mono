'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionXorOperator

Sub Main()
	Dim A As Integer = 333
	Dim B As Long = 555.555   
	Dim R As Integer
	R = (B > A) Xor (A < B) 
	if R = True Then
		throw new Exception("#Error With Xor Operator")
	End if
End Sub
End Module
