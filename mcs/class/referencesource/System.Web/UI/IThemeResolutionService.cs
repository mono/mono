namespace System.Web.UI {
    using System;

    /// <summary>
    /// Summary description for IThemeResolutionService.
    /// </summary>
    public interface IThemeResolutionService {
        ThemeProvider[] GetAllThemeProviders();

        ThemeProvider GetThemeProvider();
        ThemeProvider GetStylesheetThemeProvider();
    }
}
