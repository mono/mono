'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionOrOperator

Sub Main()
	Dim A As Integer = 3
	Dim B As Boolean = True 
	Dim R As Boolean
	R = A Or B 	'0000 Or -1 = 1111 
	if R = False Then
		throw new Exception("#Error With Or Operator")
	End if
End Sub
End Module
