'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionAndOperator

Sub Main()
	Dim A As Integer = 0
	Dim B As Integer = 2 
	Dim R As Boolean
	R = A And B		'00 And 10 
	if R = True Then
		throw new Exception("#Error With And Operator")
	End if
End Sub
End Module
