'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionLeftShiftOperator

Sub Main()
	Dim A As Long = 10.666
	Dim B As Integer = 2 
	Dim R As Integer
	R = A << B 	
	if R <> 44 Then
		throw new Exception("#Error With << Shift Operator")
	End if
End Sub
End Module
