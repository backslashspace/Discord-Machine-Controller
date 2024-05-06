using System;
using System.Reflection;

namespace Configurator
{
    internal static class State
    {
        internal static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        internal static Boolean InstallDirectoryIsPresent = false;

        internal static Boolean ServiceIsPresent = false;

        internal static Boolean MMCServiceIsPresent = false;

        internal static Boolean ServiceIsRunning = false;
    }
}