using System.Diagnostics;

namespace Hjerpbakk.Media.Server.Environment
{
    public class DebuggingService
    {
        bool debugging;

        public DebuggingService()
        {
            CheckIfDEBUG();
            RunningInDebugMode = debugging;
        }

        public bool RunningInDebugMode { get; }

        [Conditional("DEBUG")]
        void CheckIfDEBUG() => debugging = true;
    }
}
