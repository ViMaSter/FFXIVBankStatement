using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using BankStatement.Data;
using BankStatement.Extensions;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace BankStatement.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private readonly AccountStanding currentStanding;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, AccountStanding currentStanding)
        : base("Bank Statement")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        this.currentStanding = currentStanding;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text($"Current standings");

        ImGui.Spacing();
        
        foreach (var (region, dataCenters) in currentStanding.regions)
        {
            var totalGilForRegion = dataCenters.dataCenters.Values.Sum(dc => dc.worlds.Values.Sum(w => w.characters.Values.Sum(c => c.Gil + c.Retainers.Values.Sum(r => r.Gil))));
            ImGui.SetCursorPosX(20);
            if (!ImGui.CollapsingHeader($"{region} (Total: {totalGilForRegion.AddSpacing()} gil)"))
            {
                continue;
            }
            foreach (var (dataCenter, worlds) in dataCenters.dataCenters)
            {
                var totalGilForDataCenter = worlds.worlds.Values.Sum(w => w.characters.Values.Sum(c => c.Gil + c.Retainers.Values.Sum(r => r.Gil)));
                ImGui.SetCursorPosX(40);
                if (!ImGui.CollapsingHeader($"{dataCenter} (Total: {totalGilForDataCenter.AddSpacing()} gil)"))
                {
                    continue;
                }
                foreach (var (world, characters) in worlds.worlds)
                {
                    var totalGilForWorld = characters.characters.Values.Sum(c => c.Gil + c.Retainers.Values.Sum(r => r.Gil));
                    ImGui.SetCursorPosX(60);
                    if (!ImGui.CollapsingHeader($"{world} (Total: {totalGilForWorld.AddSpacing()} gil)"))
                    {
                        continue;
                    }
                    foreach (var (characterName, character) in characters.characters)
                    {
                        var totalGilForCharacter = character.Gil + character.Retainers.Values.Sum(r => r.Gil);
                        ImGui.SetCursorPosX(80);
                        ImGui.Text($"Character: {totalGilForCharacter.AddSpacing()} Gil");
                        ImGui.SetCursorPosX(80);
                        if (!ImGui.CollapsingHeader($"{characterName} (Total: {totalGilForCharacter.AddSpacing()} gil)"))
                        {
                            continue;
                        }
                        
                        foreach (var (retainerName, retainer) in character.Retainers)
                        {
                            ImGui.SetCursorPosX(100);
                            ImGui.Text($"{retainerName}: {retainer.Gil.AddSpacing()} Gil");
                        }
                    }
                }
            }
        }
    }
}
