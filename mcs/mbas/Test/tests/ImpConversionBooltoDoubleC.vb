'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBooleantoDoubleC
	Sub Main()
			Dim a as Boolean = True
			Dim b as Double = 111.9 + a
			if b <> "110.9"
				Throw new System.Exception("Addition of Boolean & Double not working. Expected 110 but got " &b) 
			End if		
	End Sub
End Module
