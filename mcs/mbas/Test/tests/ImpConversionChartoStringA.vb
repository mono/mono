'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ImpConversionChartoStringA
	Sub Main()
			Dim a as Char = "T"C
			Dim b as String= a
			if b <> "T"
				Throw new System.Exception("Conversion of Char to String not working. Expected T but got " &b) 
			End if		
	End Sub
End Module
