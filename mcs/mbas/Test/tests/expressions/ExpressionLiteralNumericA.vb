'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Numeric Literals IntegerLiteral ::= IntegralLiteralValue [ IntegralTypeCharacter ] 

Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim A  = 45S
		Dim B  = 45I 
		If A <> B Then
			Throw New Exception (" Unexpected Result for the Expression. Expected was A = B = 45 ")
		End if
	End Sub
End Module
