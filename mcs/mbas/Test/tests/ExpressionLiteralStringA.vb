'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

imports Microsoft.Visualbasic
Imports System
Module ExpressionLiteralString
	Sub Main() 
		Dim A As String = "Check"
		Dim B As String = "Concat" + "enation"
		Dim C as String = "CheckConcatenation"
		Dim D As String = A+B
		if C <> D Then
			Throw New Exception (" Unexpected Behavior of the Expression. A + B should reflect CheckConcatenation ")
		End If
	End Sub 
End Module
