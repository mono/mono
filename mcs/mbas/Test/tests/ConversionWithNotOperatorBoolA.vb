'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionNotOperator

Sub Main()
	Dim A As Integer = 29
	Dim R As Integer
	R = Not(A) 
	if R <> - 30 Then
		throw new Exception("#Error With Not Operator")
	End if
End Sub
End Module

