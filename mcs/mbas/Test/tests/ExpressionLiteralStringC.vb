'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

imports Microsoft.Visualbasic
Imports System
Module ExpressionLiteralString
	Sub Main() 
		Dim A as Object = "Test"
		Dim B as Object = "Test"
		if  B <> A
			Throw New Exception ("Unexpected Behavior. B should be Equal to A as string literals refer to the same string instance ")
		End If
	End Sub 
End Module
