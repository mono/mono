'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ImpConversionIntegertoStringA
	Sub Main()
			Dim a as Integer = 1234
			Dim b as String= a
			if b <> "1234"
				Throw new System.Exception("Conversion of Integer to String not working. Expected 1234 but got " &b) 
			End if		
	End Sub
End Module
