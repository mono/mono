'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoLongC
	Sub Main()
			Dim a as Decimal = 111.9
			Dim b as Long = 111.9 + a
			if b <> 224
				Throw new System.Exception("Addition of Decimal & Long not working. Expected 224 but got " &b) 
			End if		
	End Sub
End Module

