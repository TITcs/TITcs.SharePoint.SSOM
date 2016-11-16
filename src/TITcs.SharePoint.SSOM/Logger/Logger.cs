#define DEBUG
using Microsoft.SharePoint.Administration;
using System.Collections.Generic;

namespace TITcs.SharePoint.SSOM.Logger
{
    public class Logger : SPDiagnosticsServiceBase
    {
        public static string DiagnosticAreaName = "TIT Framework";
        private static Logger _current;

        public static Logger Current
        {
            get { return _current ?? (_current = new Logger()); }
        }

        public Logger()
            : base("TITcs Logging", SPFarm.Local)
        {

        }

        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            List<SPDiagnosticsArea> areas = new List<SPDiagnosticsArea>
            {
                new SPDiagnosticsArea(DiagnosticAreaName, new List<SPDiagnosticsCategory>
                {
                    new SPDiagnosticsCategory("Unexpected", TraceSeverity.Unexpected, EventSeverity.Error),
                    new SPDiagnosticsCategory("High", TraceSeverity.High, EventSeverity.Warning),
                    new SPDiagnosticsCategory("Medium", TraceSeverity.Medium, EventSeverity.Information),
                    new SPDiagnosticsCategory("Information", TraceSeverity.Verbose, EventSeverity.Information),
                    new SPDiagnosticsCategory("Debug", TraceSeverity.Verbose, EventSeverity.Information)
                })
            };

            return areas;
        }

        private static void WriteLog(LoggerCategory categoryName, string source, string errorMessage)
        {
            SPDiagnosticsCategory category = Current.Areas[DiagnosticAreaName].Categories[categoryName.ToString()];
            Current.WriteTrace(0, category, category.TraceSeverity, string.Concat(string.Format("[{0}]", source), " ", errorMessage));
        }

        public static void Information(string source, string message)
        {
            WriteLog(LoggerCategory.Information, source, message);
        }

        public static void Information(string source, string message, params object[] parameters)
        {
            WriteLog(LoggerCategory.Information, source, string.Format(message, parameters));
        }

        public static void Debug(string source, string message, params object[] parameters)
        {
#if DEBUG
            WriteLog(LoggerCategory.Debug, source, message != null ? string.Format(message, parameters) : "");
#endif
        }

        public static void Debug(string source)
        {
#if DEBUG
            Debug(source, null);
#endif
        }

        public static void Unexpected(string source, string message)
        {
            WriteLog(LoggerCategory.Unexpected, source, message);
        }

        public static void Unexpected(string source, string message, params object[] parameters)
        {
            WriteLog(LoggerCategory.Unexpected, source, string.Format(message, parameters));
        }
    }
}
