'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

imports Microsoft.Visualbasic
Imports System
Module ExpressionLiteralString
	Sub Main() 
		Dim A  = """"
		Dim B As Char = chr(34)
		if  B <> A
			Throw New Exception (" Unexpected Result for the Expression ")
		End If
	End Sub 
End Module
