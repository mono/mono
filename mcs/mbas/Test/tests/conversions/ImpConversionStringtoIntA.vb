'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoIntegerA
	Sub Main()
			Dim a as Integer
			Dim b as String= "1234"
			a = b
			if a <> 1234
				Throw new System.Exception("Conversion of String to Integer not working. Expected 1234 but got " &a) 
			End if		
	End Sub
End Module
