'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionofLongtoIntA
	Sub Main()
		Dim a as Long = 123456789
		Dim b as Short = a
		'if b<>123 then 
		'	Throw New System.Exception("Implicit Conversion of Long to Int has Failed")
		'End if		
	End Sub
End Module
