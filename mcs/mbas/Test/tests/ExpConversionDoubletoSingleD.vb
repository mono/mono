'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDoubletoSingleD
	Sub Main()
			Dim c as Boolean = False
			Dim a as Single
			Dim b as Double = Double.NaN
			a = CSng(b)
			c = Single.IsNan(a)
			if (c = False)
				Throw New System.Exception("Double to Single Conversion is not working properly. Expected NaN but got " &a)		
			End if		
	End Sub
End Module
