'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Numeric Literals IntegerLiteral ::= IntegralLiteralValue [ IntegralTypeCharacter ] 

Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim A  = &H8000S 
		If A <> -32768
			Throw New Exception (" Unexpected Result for the Expression. Value of A should be -32768 but got " &A)
		End if
	End Sub
End Module

