'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionAndOperator

Sub Main()
	Dim A As Integer = 444
	Dim B As Long = 333.333
	Dim C As Double =222.222
	Dim D As Short = 111.111
	Dim R As Boolean
	R = ((A > B) And (C < A))And ((B > C) And (D < C))
	if R = False Then
		Console.WriteLine("#Error With And Operator")
	End if
End Sub
End Module
