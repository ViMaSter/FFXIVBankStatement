using System.Threading;
using BankStatement.Data;
using BankStatement.Extensions;
using BankStatement.Windows;
using Dalamud;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Task = System.Threading.Tasks.Task;

namespace BankStatement;

// ReSharper disable once UnusedType.Global - Dalamud plugin entry point
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
        CurrentStanding.Print(PluginLog);
        
        MainWindow = new MainWindow(CurrentStanding);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Show current gil across all characters and retainers"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += () => {};
        
        GilRefresherTask = framework.Run(FetchCurrentGil, tokenSource.Token);
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
