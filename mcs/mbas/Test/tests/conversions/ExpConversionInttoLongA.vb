'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionofInttoLongA
	Sub Main()
		Dim a as Integer = 123
		Dim b as Long = CLng(a)
		if b<>123 then 
			Throw New System.Exception("Explicit Conversion of Int to Long has Failed")
		End if		
	End Sub
End Module
