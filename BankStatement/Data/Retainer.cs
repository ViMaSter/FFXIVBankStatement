using BankStatement.Extensions;
using ImGuiNET;

namespace BankStatement.Data;

public class Retainer(long gil, long gilInMarket)
{
    public long Gil { get; set; } = gil;
    public long GilInMarket { get; set; } = gilInMarket;

    public long ToTotal()
    {
        return Gil;
    }

    public void Draw()
    {
        ImGui.Text($"Inventory: {Gil.AddSpacing()} gil");
        ImGui.Text($"Market: {GilInMarket.AddSpacing()} gil");
    }
}
