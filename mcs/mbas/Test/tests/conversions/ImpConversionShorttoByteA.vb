'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionShorttoByteA
	Sub Main()
		Dim a as Byte
		Dim b as Short = 123 
		a = b
		if a <> 123
			Throw new System.Exception("Byte to Short Conversion is not working properly. Expected 123 but got " &a)
		End if	
	End Sub
End Module
