'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionofLongtoIntA
	Sub Main()
		Dim a as Long = 123
		Dim b as Integer = a
		if b<>123 then 
			Throw New System.Exception("Implicit Conversion of Long to Int has Failed")
		End if		
	End Sub
End Module
