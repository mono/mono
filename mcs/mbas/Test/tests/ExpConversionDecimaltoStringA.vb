'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ExpConversionDecimaltoStringA
	Sub Main()
			Dim a as Decimal= 123052
			Dim b as String= a.toString()
			if b <> "123052"
				Throw new System.Exception("Conversion of Decimal to String not working. Expected 123052 but got " &b) 
			End if		
	End Sub
End Module
