namespace Grammar.Interfaces
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;

    public interface IGrammarManager
    {
        Module GetModule(string input, TextWriter output = null);

        void AddModulesTo(ConcurrentBag<Module> bag, IEnumerable<Type> types, TextWriter output = null);
    }
}
