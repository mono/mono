namespace System.Data.Common
{
	partial class DbCommandBuilder
	{
		// open connection is required by OleDb/OdbcCommandBuilder.QuoteIdentifier and UnquoteIdentifier 
		// to get literals quotes from the driver
		internal DbConnection GetConnection()
		{
			DbDataAdapter adapter = DataAdapter;
			if (adapter != null)
			{
				DbCommand select = adapter.SelectCommand;
				if (select != null)
				{
					return select.Connection;
				}
			}

			return null;
		}

        static internal string[] ParseProcedureName(string name, string quotePrefix, string quoteSuffix) {
            // Procedure may consist of up to four parts:
            // 0) Server
            // 1) Catalog
            // 2) Schema
            // 3) ProcedureName
            //
            // Parse the string into four parts, allowing the last part to contain '.'s.
            // If less than four period delimited parts, use the parts from procedure backwards.
            //
            const string Separator = ".";

            string[] qualifiers = new string[4];
            if (!ADP.IsEmpty(name)) {
                bool useQuotes = !ADP.IsEmpty(quotePrefix) && !ADP.IsEmpty(quoteSuffix);

                int currentPos = 0, parts;
                for(parts = 0; (parts < qualifiers.Length) && (currentPos < name.Length); ++parts) {
                    int startPos = currentPos;

                    // does the part begin with a quotePrefix?
                    if (useQuotes && (name.IndexOf(quotePrefix, currentPos, quotePrefix.Length, StringComparison.Ordinal) == currentPos)) {
                        currentPos += quotePrefix.Length; // move past the quotePrefix

                        // search for the quoteSuffix (or end of string)
                        while (currentPos < name.Length) {
                            currentPos = name.IndexOf(quoteSuffix, currentPos, StringComparison.Ordinal);
                            if (currentPos < 0) {
                                // error condition, no quoteSuffix
                                currentPos = name.Length;
                                break;
                            }
                            else {
                                currentPos += quoteSuffix.Length; // move past the quoteSuffix

                                // is this a double quoteSuffix?
                                if ((currentPos < name.Length) && (name.IndexOf(quoteSuffix, currentPos, quoteSuffix.Length, StringComparison.Ordinal) == currentPos)) {
                                    // a second quoteSuffix, continue search for terminating quoteSuffix
                                    currentPos += quoteSuffix.Length; // move past the second quoteSuffix
                                }
                                else {
                                    // found the terminating quoteSuffix
                                    break;
                                }
                            }
                        }
                    }

                    // search for separator (either no quotePrefix or already past quoteSuffix)
                    if (currentPos < name.Length) {
                        currentPos = name.IndexOf(Separator, currentPos, StringComparison.Ordinal);
                        if ((currentPos < 0) || (parts == qualifiers.Length-1)) {
                            // last part that can be found
                            currentPos = name.Length;
                        }
                    }

                    qualifiers[parts] = name.Substring(startPos, currentPos-startPos);
                    currentPos += Separator.Length;
                }

                // allign the qualifiers if we had less than MaxQualifiers
                for(int j = qualifiers.Length-1; 0 <= j; --j) {
                    qualifiers[j] = ((0 < parts) ? qualifiers[--parts] : null);
                }
            }
            return qualifiers;
        }
	}
}