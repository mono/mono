'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoDecimalA
	Sub Main()
			Dim a as Decimal
			Dim b as Single = -4.94065645841247e-100
			a = b
			if a<>-0 Then
				Throw New System.Exception("Single to Decimal Conversion is not working properly. Expected 0 but got " &a)		
		End if		
	End Sub
End Module
