'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'CharacterLiteral ::= DoubleQuoteCharacter StringCharacter DoubleQuoteCharacter C

Imports System
Imports Microsoft.Visualbasic
Module ExpressionLiteralsChar 
	Sub Main ( ) 
		Dim A as Char = """"c 	
		Dim B = Chr(34)
			if  B <> A
				Throw New Exception ("  Unexpected Result for the Expression :: A should be euqal to (") ")
			End If
	End Sub 
End Module
