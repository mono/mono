'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ImpConversionStringtoCharA
	Sub Main()
			Dim a() as Char
			Dim b as String= "Program"
			a = b
			if a <> "Program"
				Throw new System.Exception("Conversion of String to Char not working. Expected Program but got " &a) 
			End if		
	End Sub
End Module
