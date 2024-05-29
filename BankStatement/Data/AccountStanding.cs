using System.Collections.Generic;
using System.Linq;
using BankStatement.Extensions;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace BankStatement.Data;

public class AccountStanding(Dictionary<string, Region> regions)
{
    public Dictionary<string, Region> Regions { get; set; } = regions;

    public long ToTotal()
    {
        return Regions.Values.Sum(r => r.ToTotal());
    }

    public void Draw()
    {
        ImGui.Text($"Total: {ToTotal().AddSpacing()} gil");
        
        foreach (var (name, region) in Regions)
        {
            if (ImGui.TreeNode($"{name} ({region.ToTotal().AddSpacing()} gil)###{name}"))
            {
                region.Draw();
                ImGui.TreePop();
            }
        }
    }
    
    public void Print(IPluginLog log)
    {
        var characterInfo = from region in Regions
                      from dataCenter in region.Value.DataCenters
                      from world in dataCenter.Value.Worlds
                      from character in world.Value.Characters
                      select $"{region.Key}/{dataCenter.Key}/{world.Key}/{character.Key} has {character.Value.Gil} gil";
        var retainerInfo = from region in Regions
                      from dataCenter in region.Value.DataCenters
                      from world in dataCenter.Value.Worlds
                      from character in world.Value.Characters
                      from retainer in character.Value.Retainers
                      select $"{region.Key}/{dataCenter.Key}/{world.Key}/{retainer.Key} has {retainer.Value.Gil} gil";
        
        foreach (var info in characterInfo.Concat(retainerInfo))
        {
            log.Info(info);
        }
    }
}
