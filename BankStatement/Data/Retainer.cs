using BankStatement.Extensions;
using ImGuiNET;

namespace BankStatement.Data;

public class Retainer(long gil)
{
    public long Gil { get; set; } = gil;

    public long ToTotal()
    {
        return Gil;
    }

    public void Draw()
    {
        ImGui.Text($"{Gil.AddSpacing()} gil");
    }
}
