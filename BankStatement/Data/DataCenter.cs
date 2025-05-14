using System.Collections.Generic;
using System.Linq;
using BankStatement.Extensions;
using ImGuiNET;

namespace BankStatement.Data;

public class DataCenter(Dictionary<string, World> worlds)
{
    public Dictionary<string, World> Worlds { get; set; } = worlds;

    public long ToTotal()
    {
        return Worlds.Values.Sum(w => w.ToTotal());
    }

    public void Draw()
    {
        foreach (var (name, world) in Worlds)
        {
            ImGui.SetNextItemOpen(true);
            if (ImGui.TreeNode($"{name}: {world.ToTotal().AddSpacing()} gil###{name}"))
            {
                world.Draw();
                ImGui.TreePop();
            }
        }
    }
}
