namespace Grammar.Explorer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Grammar.Interfaces;
    using JetBrains.Annotations;

    internal static class Helper
    {
        private static readonly Regex ExtensionRegex = new Regex("\\.(dll|exe)$", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.IgnoreCase);

        internal interface ICancellationSourceOwner
        {
            ref CancellationTokenSource this[Enum source] { get; }
        }

        public static async Task WriteBytesAsync(string filePath, byte[] bytes, CancellationToken? cancellationToken = null)
        {
            using(var sourceStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 0x1000, true))
            {
                await sourceStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken ?? CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        internal static async Task WrapCancellation(
            [NotNull] Func<CancellationToken, Task> action,
            [NotNull] ICancellationSourceOwner owner,
            Enum sourceName,
            bool catchOperationCancelled = false)
        {
            CancellationTokenSource cancellationSource = null;
            try
            {
                cancellationSource = new CancellationTokenSource();
                Interlocked.Exchange(ref owner[sourceName], cancellationSource)?.Cancel();

                await action(cancellationSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (catchOperationCancelled)
            {
            }
            finally
            {
                if (null != cancellationSource)
                {
                    Interlocked.CompareExchange(ref owner[sourceName], null, cancellationSource);
                    cancellationSource.Dispose();
                }
            }
        }

        public static bool TryOpenFiles(out string[] fileNames, bool multiselect = true)
        {
            using (var dialog = new OpenFileDialog
                                    {
                                        AddExtension = true,
                                        DefaultExt = "dll",
                                        Filter = @".dll|*.dll|exe|*.exe",
                                        Multiselect = multiselect
                                    })
            {
                if (DialogResult.OK != dialog.ShowDialog() || 0 == dialog.FileNames.Length)
                {
                    fileNames = null;
                    return false;
                }

                fileNames = dialog.FileNames;
                return true;
            }
        }

        public static bool TryGetGrammarManagers([NotNull] string fileName, out Type[] types)
        {
            try
            {
                types = (from type in Assembly.Load(fileName).GetExportedTypes()
                         where typeof(IGrammarManager).IsAssignableFrom(type)
                               && null != type.GetConstructor(Type.EmptyTypes)
                         select type).ToArray();
                return true;
            }
            catch (SystemException exc) when (exc is ReflectionTypeLoadException ||
                                              exc is BadImageFormatException ||
                                              exc is FileNotFoundException ||
                                              exc is FileLoadException)
            {
                types = null;
                return false;
            }
        }

        internal static string GetAssemblyFullName(this string path)
        {
            return AssemblyName.GetAssemblyName(path).FullName;
        }

        internal static Assembly GetLoaded([NotNull] this AppDomain appDomain, [NotNull] string fullName, [NotNull] string dataKey, bool unwrap = false)
        {
            var privateProbing = (ConcurrentDictionary<string, HashSet<string>>)appDomain.GetData(dataKey);
            try
            {
                if (unwrap)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Parallel.ForEach(
                        new DirectoryInfo(Path.GetDirectoryName(fullName)).EnumerateFiles(),
                        file =>
                            {
                                var fileName = file.FullName;
                                if (ExtensionRegex.IsMatch(fileName))
                                {
                                    try
                                    {
                                        var fullAssemblyName = fileName.GetAssemblyFullName();
                                        privateProbing.GetOrAdd(
                                            fullAssemblyName,
                                            new HashSet<string>(StringComparer.OrdinalIgnoreCase)).Add(fileName);
                                    }
                                    catch (BadImageFormatException)
                                    {
                                    }
                                }
                            });

                    fullName = fullName.GetAssemblyFullName();
                }

                return Assembly.Load(fullName);
            }
            catch (FileLoadException)
            {
            }
            catch (FileNotFoundException)
            {
                if (privateProbing.TryGetValue(fullName, out var set))
                {
                    foreach (var library in set)
                    {
                        try
                        {
                            return Assembly.LoadFrom(library);
                        }
                        catch (SystemException e) when (e is FileNotFoundException || e is BadImageFormatException)
                        {
                        }
                    }
                }
            }

            return null;
        }
    }
}

