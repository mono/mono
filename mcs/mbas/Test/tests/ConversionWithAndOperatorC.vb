'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionAndOperator

Sub Main()
	Dim A As Integer = 333
	Dim B As Long = 555.555   
	Dim R As Boolean
	R = (B < A) And (A < B) 
	if R = True Then
		Console.WriteLine("#Error With And Operator")
	End if
End Sub
End Module
