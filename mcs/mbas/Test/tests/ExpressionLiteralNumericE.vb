'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Numeric Literals IntegerLiteral ::= IntegralLiteralValue [ IntegralTypeCharacter ] 

Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim A = 922337203685477L
		Dim B as Long
		B = A
		Dim C as Long = B 
			If C <> A Then
				Throw New Exception (" Unexpected Behavior of the Expression. C should be equal to B = A  ")
			End if
	End Sub
End Module

