'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ImpConversionStringtoByteA
	Sub Main()
			Dim a as Byte
			Dim b as String= "123"
			a = b
			if a <> 123
				Throw new System.Exception("Conversion of String to Byte not working. Expected 123 but got " &a) 
			End if		
	End Sub
End Module
