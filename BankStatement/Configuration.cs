﻿using System;
using System.Collections.Generic;
using BankStatement.Data;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace BankStatement;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public AccountStanding AccountStanding { get; set; } = new(new Dictionary<string, Region>());

    [NonSerialized]
    private DalamudPluginInterface? _pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
}
