'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.
Imports System
Module ImpConversionofInttoLongA
	Sub Main()
		Dim a as Int = 123
		Dim b as Long = a
		if b<>123 then 
			Throw New System.Exception("Implicit Conversion of Int to Long has Failed")
		End if		
	End Sub
End Module
