'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionofLongtoIntA
	Sub Main()
		Dim a as Long = 123
		Dim b as Integer = CInt(a)
		if b<>123 then 
			Throw New System.Exception("Explicit Conversion of Long to Int has Failed")
		End if		
	End Sub
End Module
