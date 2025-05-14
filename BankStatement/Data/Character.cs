using System.Collections.Generic;
using System.Linq;
using BankStatement.Extensions;
using ImGuiNET;

namespace BankStatement.Data;

public class Character(long gil, Dictionary<string, Retainer> retainers)
{
    public long Gil { get; set; } = gil;
    public Dictionary<string, Retainer> Retainers { get; init; } = retainers;

    public long ToTotal()
    {
        return Gil + Retainers.Values.Sum(r => r.ToTotal());
    }

    public void Draw()
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPos().X + 20);
        ImGui.Text($"Inventory: {Gil.AddSpacing()} gil");
        foreach (var (name, retainer) in Retainers)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPos().X + 40);
            ImGui.Text($"{name}: {retainer.ToTotal().AddSpacing()} gil");
        }
    }
}
