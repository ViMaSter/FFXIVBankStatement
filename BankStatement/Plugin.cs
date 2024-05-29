using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Threading;
using Dalamud;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.STD;
using BankStatement.Data;
using BankStatement.Extensions;
using BankStatement.Windows;
using Task = System.Threading.Tasks.Task;

namespace BankStatement;

// ReSharper disable once ClassNeverInstantiated.Global - Dalamud plugin entry point
public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/gil";

    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("BankStatement");
    private MainWindow MainWindow { get; init; }
    
    public readonly AccountStanding CurrentStanding;

    private readonly CancellationTokenSource tokenSource = new();

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] IPluginLog pluginLog,
        [RequiredVersion("1.0")] IFramework framework,
        [RequiredVersion("1.0")] IClientState clientState
            )
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        PluginLog = pluginLog;
        ClientState = clientState;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        CurrentStanding = Configuration.AccountStanding;
        PluginLog.Info("Restored state: ");
        foreach (var (region, dataCenters) in CurrentStanding.regions)
        {
            foreach (var (dataCenter, worlds) in dataCenters.dataCenters)
            {
                foreach (var (world, characters) in worlds.worlds)
                {
                    foreach (var (characterName, character) in characters.characters)
                    {
                        PluginLog.Info($"{region}/{dataCenter}/{world}/{characterName} has {character.Gil} gil");
                        foreach (var (retainerName, retainer) in character.Retainers)
                        {
                            PluginLog.Info($"{region}/{dataCenter}/{world}/{characterName}/{retainerName} has {retainer.Gil} gil");
                        }
                    }
                }
            }
        }
        
        MainWindow = new MainWindow(this, CurrentStanding);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Show current gil across all characters and retainers"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        
        GilRefresherTask = framework.Run(FetchCurrentGil, tokenSource.Token);
    }

    public Task GilRefresherTask { get; set; }

    public IClientState ClientState { get; set; }
    public IPluginLog PluginLog { get; set; }
    
    private void SaveData()
    {
        PluginLog.Info("Saving state: ");
        foreach (var (region, dataCenters) in CurrentStanding.regions)
        {
            foreach (var (dataCenter, worlds) in dataCenters.dataCenters)
            {
                foreach (var (world, characters) in worlds.worlds)
                {
                    foreach (var (characterName, character) in characters.characters)
                    {
                        PluginLog.Info($"{region}/{dataCenter}/{world}/{characterName} has {character.Gil} gil");
                        foreach (var (retainerName, retainer) in character.Retainers)
                        {
                            PluginLog.Info($"{region}/{dataCenter}/{world}/{characterName}/{retainerName} has {retainer.Gil} gil");
                        }
                    }
                }
            }
        }
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
                var homeWorld = ClientState.LocalPlayer?.HomeWorld.GetWithLanguage(ClientLanguage.English);
                var homeWorldName = homeWorld?.Name.ToString();
                
                if (homeWorld?.DataCenter.Value == null)
                {
                    PluginLog.Warning("Current character world data center is null");
                    continue;
                }
                
                var homeWorldDataCenter = homeWorld.DataCenter.Value?.Name.ToString();
                if (string.IsNullOrEmpty(homeWorldDataCenter))
                {
                    PluginLog.Warning("Current character world data center is null");
                    continue;
                }
                
                var homeWorldRegion = homeWorld.DataCenter?.Value?.Region.GetRegionName();
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
                    var byteArrayOfName = retainer->Name;
                    var retainerName = new string((sbyte*)byteArrayOfName);
                    CurrentStanding.UpdateRetainerStanding(homeWorldRegion, homeWorldDataCenter, homeWorldName, currentCharacterName, retainerName, retainer->Gil, SaveData);
                }
            }
        }
        // ReSharper disable once FunctionNeverReturns - this is a looping task
    }

    public void Dispose()
    {
        tokenSource.Cancel();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}

public static class StdVectorExtensions
{
    public static IEnumerable<T> ToList<T> (this StdVector<T> vector) where T : unmanaged
    {
        var list = new List<T>();
        var size = vector.Size();
        for (uint i = 0; i < size; i++)
        {
            list.Add(vector.Get(i));
        }
        return list;
    }
}
