using System;
using System.Collections.Generic;

namespace Andromeda_Actions_Core.Plugins
{
    public interface IPlugin
    {
        string PluginName { get; }
        string PluginAuthor { get; }
        string PluginVersion { get; }
        List<Type> ImportActions();
    }
}