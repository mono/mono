'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Numeric Literals IntegerLiteral ::= IntegralLiteralValue [ IntegralTypeCharacter ] 

Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim A  = &O07
		If A <> 7
			Throw New Exception (" Unexpected Result. A= 7 was expected but got A= " &A)
		nd if
	End Sub
End Module

