'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ImpConversionDecimaltoStringA
	Sub Main()
			Dim a as Decimal= 123.052
			Dim b as String= a
			if b <> "123.052"
				Throw new System.Exception("Conversion of Decimal to String not working. Expected 123.052 but got " &b) 
			End if		
	End Sub
End Module
