'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionOrOperator

Sub Main()
	Dim A As Integer = 10
	Dim B As Integer = 9 
	Dim R As Integer
	R = A Or B 	
	if R = False Then
		throw new Exception("#Error With Or Operator")
	End if
End Sub
End Module
