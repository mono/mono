'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionLongtoDoubleA
	Sub Main()
		Dim a as Long = 124
		Dim b as Double = a
		if b<>124 then 
			Throw New System.Exception("Implicit Conversion of Long to Double has Failed. Expected 124 but got " &b)
		End if		
	End Sub
End Module
