'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ExpConversionBytetoStringA
	Sub Main()
			Dim a as Byte = 123
			Dim b as String= a.toString()
			if b <> "123"
				Throw new System.Exception("Conversion of Byte to String not working. Expected 123 but got " &b) 
			End if		
	End Sub
End Module
