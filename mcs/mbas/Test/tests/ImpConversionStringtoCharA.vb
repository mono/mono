'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ImpConversionStringtoCharA
	Sub Main()
			Dim a as Char
			Dim b as String= "This is a program"
			a = b
			if a <> "T"
				Throw new System.Exception("Conversion of String to Char not working. Expected T but got " &a) 
			End if		
	End Sub
End Module
