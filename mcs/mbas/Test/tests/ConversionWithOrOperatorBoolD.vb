'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionOrOperator

Sub Main()
	Dim A As Integer = 254
	Dim B As Byte = 255   
	Dim R As Boolean
	R = (B < A) Or (A > B) 
	if R = True Then
		Console.WriteLine("#Error With Or Operator")
	End if
End Sub
End Module
