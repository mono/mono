'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionLeftShiftOperatorLRByte

Sub Main()
	Dim A As Integer = 10
	Dim B As Integer = 11 
	Dim R As Integer
	R = A << B
	if R <> 20480 Then
		throw new Exception("#Error With << Shift Operator")
	End if
End Sub
End Module

