'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDoubletoLongA
	Sub Main()
		Dim a as Double = 123.5 
		Dim b as Long
		b = CLng(a)
		if b <> 124
			Throw new System.Exception("Double to Long Conversion is not working properly. Expected 124 but got " &b)
		End if	
	End Sub
End Module
