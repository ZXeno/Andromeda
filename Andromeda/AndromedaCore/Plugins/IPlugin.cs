namespace AndromedaCore.Plugins
{
    public interface IPlugin
    {
        string PluginName { get; }
        string PluginAuthor { get; }
        string PluginVersion { get; }
        string CompatibleVersion { get; }

        bool IsCompatible(string version);
        void InitializePlugin();
        void UnloadPlugin();
    }
}