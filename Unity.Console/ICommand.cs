using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.Console
{
    public interface ICommand
    {
        string Description { get; }
        string Help { get; }
        int? ExecuteLine(string[] args);
        bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options);
    }
}
