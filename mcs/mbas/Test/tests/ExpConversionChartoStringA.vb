'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionChartoStringA
	Sub Main()
			Dim a as Char = "T"C
			Dim b as String= a.toString()
			if b <> "T"
				Throw new System.Exception("Conversion of Char to String not working. Expected T but got " &b) 
			End if		
	End Sub
End Module
