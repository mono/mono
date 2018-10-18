namespace System.Globalization
{
	partial class DateTimeFormatInfo {
        //
        // Decimal separator used by positive TimeSpan pattern
        //
        [NonSerialized]
        private string m_decimalSeparator;
        internal string DecimalSeparator
        {
            get
            {
                if (m_decimalSeparator == null)
                {
                    CultureData cultureDataWithoutUserOverrides = m_cultureData.UseUserOverride ?
                        CultureData.GetCultureData(m_cultureData.CultureName, false) :
                        m_cultureData;
                    m_decimalSeparator = new NumberFormatInfo(cultureDataWithoutUserOverrides).NumberDecimalSeparator;
                }
                return m_decimalSeparator;
            }
        }
	}
}
