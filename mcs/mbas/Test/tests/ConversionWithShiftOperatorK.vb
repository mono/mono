'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionLeftShiftOperator

Sub Main()
	Dim A As Byte = 10
	Dim B As Integer = 2 
	Dim R As Integer
	R = A << B 	
	if R <> 40 Then
		Console.WriteLine("#Error With << Shift Operator")
	End if
End Sub
End Module


