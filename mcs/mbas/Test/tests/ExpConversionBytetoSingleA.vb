'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionBytetoSingleA
	Sub Main()
		Dim a as Byte = 123 
		Dim b as Single
		b = CSng(a)
		if b <> 123
			Throw new System.Exception("Byte to Single Conversion is not working properly. Expected 123 but got " &b)
		End if	
	End Sub
End Module
