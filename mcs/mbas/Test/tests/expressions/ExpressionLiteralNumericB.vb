'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Numeric Literals IntegerLiteral ::= IntegralLiteralValue [ IntegralTypeCharacter ] 

Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim A  = 5e2
		If A <> 500
			Throw New Exception (" Unexpected Result for the Expression. A = 500 was expected  ")
		End if
	End Sub
End Module

