'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
REM LineNo: 9
REM ExpectedError: BC30036 
REM ErrorMessage: Overflow. Dim BigDec As Decimal = 9223372036854775808 (Hence Define as Double - D)
Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim BigDec As Decimal= 9223372036854775808
		Console.WriteLine(BigDec)
	End Sub
End Module
