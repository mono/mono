'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoDoubleC
	Sub Main()
			Dim a as Decimal = 111.9
			Dim b as Double = 111.9 + a
			if b <> 223.8
				Throw new System.Exception("Addition of Decimal & Double not working. Expected 223.8 but got " &b) 
			End if		
	End Sub
End Module
