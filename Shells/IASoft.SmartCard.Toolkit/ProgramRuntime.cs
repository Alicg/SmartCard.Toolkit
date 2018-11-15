namespace SVA
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    using Utils.Extensions;

    internal class ProgramRuntime
    {
        public static readonly DirectoryInfo ProgramDirectory = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
        public static readonly FileInfo ProgramFile = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);

        public static readonly string ProgramName = typeof(ProgramRuntime).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false).OfType<AssemblyTitleAttribute>().First().Title;
        public static readonly Version ProgramVersion = typeof(ProgramRuntime).Assembly.GetName().Version;
        public static readonly Version ProgramFileVersion = new Version(typeof(ProgramRuntime).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().First().Version);
        public static readonly string ProgramVersionString = ProgramVersion.ToString();

        private static readonly Lazy<ProgramRuntime> Holder = new Lazy<ProgramRuntime>(() => new ProgramRuntime());
        public static ProgramRuntime Instance => Holder.Value;

        public static void Shutdown()
        {
            var t = new Thread(() => Environment.Exit(0));
            t.Start();
            t.Join();
        }

        public static void ShowErrorMessage(string message, Exception error)
        {
            var msg = $"{message}";
            if (error != null)
            {
                msg += $"\n\nMessage details: {error.GetFullMessage()}";
            }
            MessageBox.Show(msg, $"{ProgramName} [{ProgramVersion}]", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        public static void ShowWarningMessage(string message, Exception error)
        {
            //TODO: [deemah: 29.09.2016] Aggregated exceptions support
            var msg = $"{message}";
            if (error != null)
            {
                msg += $"\n\nMessage details: {error.GetFullMessage()}";
            }
            MessageBox.Show(msg, $"{ProgramName} [{ProgramVersion}]", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        }
    }
}