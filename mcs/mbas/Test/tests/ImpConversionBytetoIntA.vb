'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBytetoIntA
	Sub Main()
		Dim a as Byte = 123 
		Dim b as Integer
		b = a
		if b <> 123
			Throw new System.Exception("Byte to Int Conversion is not working properly. Expected 123 but got " &b)
		End if	
	End Sub
End Module
