'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoLongA
	Sub Main()
			Dim a as Long
			Dim b as String= "123"
			a = b
			if a <> 123
				Throw new System.Exception("Conversion of String to Long not working. Expected 123 but got " &a) 
			End if		
	End Sub
End Module
