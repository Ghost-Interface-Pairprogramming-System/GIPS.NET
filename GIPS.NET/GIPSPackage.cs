using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace GIPS.NET
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(GIPSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class GIPSPackage : AsyncPackage, IVsDebuggerEvents
    {

        private EnvDTE80.DTE2 dte = null;
        private uint cookie = 0;

        /// <summary>
        /// GIPSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "e86971ca-38ac-4b00-bf0f-2180de9e0d5f";

        /// <summary>
        /// Initializes a new instance of the <see cref="GIPSPackage"/> class.
        /// </summary>
        public GIPSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var debugger = GetService(typeof(SVsShellDebugger)) as IVsDebugger;
            dte = GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

            debugger.AdviseDebuggerEvents(this, out cookie);
        }

        public int OnModeChange(DBGMODE dbgmodeNew)
        {
            ShowMode(dbgmodeNew);

            var debugger = dte.Debugger as EnvDTE80.Debugger2;
            var reason = debugger.LastBreakReason;

            if (dbgmodeNew == DBGMODE.DBGMODE_Break && reason == EnvDTE.dbgEventReason.dbgEventReasonExceptionNotHandled)
            {
                var exception = debugger.GetExpression2("$exception");
                var firstStackTrace = debugger.GetExpression2("$exception.StackTrace.Split(new String[] { \"\\r\\n\" }, StringSplitOptions.None)[0]").Value;


                var type = exception.Type;
                var lang = debugger.CurrentStackFrame.Language;

                var start = firstStackTrace.LastIndexOf(' ') + 1;
                var end = firstStackTrace.LastIndexOf('\"');
                var lineAt = firstStackTrace.Substring(start, end - start);

                var message = exception.Value;


                Debug.WriteLine("------Exception Not Handled!!------");
                Debug.WriteLine("Type: " + type);
                Debug.WriteLine("Lang: " + lang);
                Debug.WriteLine("LineAt: " + lineAt);
                Debug.WriteLine("Message: " + message);
                Debug.WriteLine("------Exception Not Handled!!------");
                var ukgkConn = new UkagakaSSTPConnection("test");
                ukgkConn.SendNotify1_1("OnExceptionOccured", type);
            }

            return 0;
        }

        private void ShowMode(DBGMODE mode)
        {
            string msg = "";

            // Remove the DBGMODE.DBGMODE_Enc flag if present
            mode = mode & ~DBGMODE.DBGMODE_EncMask;

            switch (mode)
            {
                case DBGMODE.DBGMODE_Design:

                    msg = "Entered mode: Design";
                    break;

                case DBGMODE.DBGMODE_Break:

                    msg = "Entered mode: Break";
                    break;

                case DBGMODE.DBGMODE_Run:

                    msg = "Entered mode: Run";
                    break;
            }
            Debug.WriteLine(msg);
        }

        #endregion
    }
}
