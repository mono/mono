'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionShiftOperator

Sub Main()
	Dim A As Byte = 10
	Dim B As Integer = 9
	Dim R As Integer
	R = A >> B
	if R <> 5 Then
		throw new Exception("#Error With >> Shift Operator")
	End if
End Sub
End Module

