using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using BankStatement.Data;

namespace BankStatement;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public AccountStanding AccountStanding { get; set; } = new(new());

    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}
