'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionShorttoLongA
	Sub Main()
		Dim a as Short = 123 
		Dim b as Long
		b = a
		if b <> 123
			Throw new System.Exception("Short to Long Conversion is not working properly. Expected 123 but got " &b)
		End if	
	End Sub
End Module
