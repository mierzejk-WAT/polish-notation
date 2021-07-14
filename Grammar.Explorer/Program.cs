namespace Grammar.Explorer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;

    using Forms;

    public static class Program
    {
        private const string LockName = @"_AssemblyResolve_Lock";
        private const string PathsName = @"_AssemblyResolve_Paths";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(params string[] args)
        {
           /*
            * The method prevents Assembly.LoadFrom 'reloading' assemblies that are already being referenced.
            * Otherwise, if a common assembly (e.g. Grammar.Interfaces) is explicitely reloaded by Assembly.LoadFrom,
            * and another (plug-in) assembly is being loaded by Assembly.LoadFrom, it happens the load context
            * refers to the reloaded assembly in place of the one referenced by the executing assembly.
            * It entails disorder where the same common type (e.g. Grammar.Interfaces.IGrammarManager) is available
            * in two disparate assemblies, which can lead to false result of type equality check, like Type.IsAssignableFrom.
            */
            AppDomain.CurrentDomain.SetData(PathsName, new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase));
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Type[] types = null;
            if (args.Length > 0)
            {
                Helper.TryGetGrammarManagers(Path.GetFullPath(args[0]), out types);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var mainForm = new MainForm(types))
            {
                Application.Run(mainForm);
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Monitor.Enter(LockName);
            var appDomain = AppDomain.CurrentDomain;
            try
            {
                if (null != appDomain.GetData(LockName))
                {
                    return null;
                }

                appDomain.SetData(LockName, LockName);
                return Path.IsPathRooted(args.Name) && File.Exists(args.Name)
                           ? appDomain.GetLoaded(args.Name, PathsName, true)
                             ?? Assembly.LoadFrom(args.Name)
                           : appDomain.GetLoaded(args.Name, PathsName);
            }
            finally
            {
                appDomain.SetData(LockName, null);
                Monitor.Exit(LockName);
            }
        }

        /*
        // DO NOT USE - works for loading from paths only, while it is to support also full names
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            lock (sender)
            {
                var path = args.Name;
                var appDomain = (AppDomain)sender;
                if (null != appDomain.GetData(LockName) || !Path.IsPathRooted(path) || !File.Exists(path))
                {
                    return null;
                }

                appDomain.SetData(LockName, LockName);

                // name.CodeBase == null => Assembly.Load fails for external (dynamically loaded) assemblies every time -> (recursion) Assembly.LoadFrom
                //var name = Assembly.ReflectionOnlyLoadFrom(path).FullName;
                // name.CodeBase != null for all assemblies being loaded externally, including those preloaded by the application -> Assembly.Load == Assembly.LoadFrom (no recursion)
                var name = Assembly.ReflectionOnlyLoadFrom(args.Name).GetName();

                Assembly result;
                try
                {
                    result = Assembly.Load(name);
                }
                catch (FileNotFoundException)
                {
                    result = Assembly.LoadFrom(path);
                }

                appDomain.SetData(LockName, null);
                return result;
            }
        }
        */
    }
}
