using EnvDTE;
using CodicExtension.Settings;

namespace CodicExtension
{
    static class Global
    {		
		public static DTE DTE = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE;
    };	
}