using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLogViewer
{
    internal class Logger : ISingleton
    {
        private static readonly Serilog.Core.Logger SeriLog;

        static Logger()
        {
            SeriLog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("data/log.txt")
                .CreateLogger();

            Inf("Logger starting");
        }

        public static void Inf(string text)
        {
            SeriLog.Information(text);
        }

        public static void Inf(string text, params object[] propertyValues)
        {
            SeriLog.Information(text, propertyValues);
        }
    }
}
