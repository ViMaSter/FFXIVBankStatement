using System;
using System.Text;
using System.Threading;
using BankStatement.Data;
using BankStatement.Extensions;
using BankStatement.Windows;
using Dalamud.Game;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Task = System.Threading.Tasks.Task;

namespace BankStatement;

// ReSharper disable once UnusedType.Global - Dalamud plugin entry point
public sealed class Plugin : IDalamudPlugin
{
    private IDalamudPluginInterface PluginInterface { get; init; }
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("BankStatement");
    private MainWindow MainWindow { get; init; }
    
    public readonly AccountStanding CurrentStanding;

    private readonly CancellationTokenSource tokenSource = new();
    
    public Plugin(
        IDalamudPluginInterface pluginInterface,
        IPluginLog pluginLog,
        IFramework framework,
        IClientState clientState,
        IAddonLifecycle addonLifecycle
            )
    {
        PluginInterface = pluginInterface;
        PluginLog = pluginLog;
        ClientState = clientState;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        CurrentStanding = Configuration.AccountStanding;
        PluginLog.Info("Restored state: ");
        CurrentStanding.Print(PluginLog);
        
        MainWindow = new MainWindow(CurrentStanding);

        WindowSystem.AddWindow(MainWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += () => MainWindow.Toggle();
        PluginInterface.UiBuilder.OpenConfigUi += () => {};
        
        GilRefresherTask = framework.Run(FetchCurrentGil, tokenSource.Token);

        // add hook for 0x140F8B8B0
        addonLifecycle.RegisterListener(AddonEvent.PreSetup, "Currency", (_, _) =>
        {
            if (MainWindow.IsOpen)
            {
                return;
            }
            MainWindow.Toggle();
        });
        addonLifecycle.RegisterListener(AddonEvent.PreFinalize, "Currency", (_, _) =>
        {
            if (!MainWindow.IsOpen)
            {
                return;
            }
            MainWindow.Toggle();
        });
    }

    public Task GilRefresherTask { get; set; }

    public IClientState ClientState { get; set; }
    public IPluginLog PluginLog { get; set; }
    
    private void SaveData()
    {
        PluginLog.Info("Saving state: ");
        CurrentStanding.Print(PluginLog);
        Configuration.AccountStanding = CurrentStanding;
        PluginInterface.SavePluginConfig(Configuration);
    }

    private async Task FetchCurrentGil()
    {
        while (true)
        {
            await Task.Delay(1000, tokenSource.Token);
            unsafe
            {
                var homeWorld = ClientState.LocalPlayer?.HomeWorld.Value;
                var homeWorldName = homeWorld?.Name.ToString();
                
                if (homeWorld?.DataCenter.Value == null)
                {
                    PluginLog.Warning("Current character world data center is null");
                    continue;
                }

                var homeWorldDataCenter = homeWorld.Value.DataCenter.Value.Name.ToString();
                if (string.IsNullOrEmpty(homeWorldDataCenter))
                {
                    PluginLog.Warning("Current character world data center is null");
                    continue;
                }
                
                var homeWorldRegion = homeWorld.Value.DataCenter.Value.Region.GetRegionName();
                if (string.IsNullOrEmpty(homeWorldRegion))
                {
                    PluginLog.Warning("Current character world region is null");
                    continue;
                }
                
                if (string.IsNullOrEmpty(homeWorldName))
                {
                    PluginLog.Warning("Current character world is null");
                    continue;
                }
                
                var currentCharacterName = ClientState.LocalPlayer?.Name.TextValue;
                
                if (currentCharacterName == null || string.IsNullOrEmpty(currentCharacterName))
                {
                    PluginLog.Warning("Current character name is null");
                    continue;
                }
                
                var currentCharacterGil = InventoryManager.Instance()->GetGil();
                CurrentStanding.UpdateCharacterStanding(homeWorldRegion, homeWorldDataCenter, homeWorldName, currentCharacterName, currentCharacterGil, SaveData);
                
                var retainerCount = RetainerManager.Instance()->GetRetainerCount();
                for (uint i = 0; i < retainerCount; i++)
                {
                    var retainer = RetainerManager.Instance()->GetRetainerBySortedIndex(i);
                    if (retainer == null)
                    {
                        continue;
                    }
                    var retainerNameBytes = retainer->Name.ToArray();
                    // find first 00 and only take bytes before that
                    var first00 = Array.IndexOf(retainerNameBytes, (byte) 0);
                    if (first00 != -1)
                    {
                        retainerNameBytes = retainerNameBytes[..first00];
                    }
                    var retainerNameString = Encoding.UTF8.GetString(retainerNameBytes);
                    CurrentStanding.UpdateRetainerStanding(homeWorldRegion, homeWorldDataCenter, homeWorldName, currentCharacterName, retainerNameString, retainer->Gil, SaveData);
                }
            }
        }
        // ReSharper disable once FunctionNeverReturns - this is a looping task
    }

    public void Dispose()
    {
        tokenSource.Cancel();
    }

    private void DrawUI() => WindowSystem.Draw();
}
