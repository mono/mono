'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionChartoStringB
	Sub Main()
			Dim a() as Char = "Program"
			Dim b as String= a
			if b <> "Program"
				Throw new System.Exception("Conversion of Char to String not working. Expected Program but got " &b) 
			End if		
	End Sub
End Module
