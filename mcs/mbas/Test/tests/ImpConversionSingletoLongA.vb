'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoLongA
	Sub Main()
		Dim a as Single = 123.5 
		Dim b as Long
		b = a
		if b <> 124
			Throw new System.Exception("Single to Long Conversion is not working properly. Expected 124 but got " &b)
		End if	
	End Sub
End Module
