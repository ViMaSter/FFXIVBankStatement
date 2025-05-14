using System.Collections.Generic;
using System.Linq;
using BankStatement.Extensions;
using ImGuiNET;

namespace BankStatement.Data;

public class Region(Dictionary<string, DataCenter> dataCenters)
{
    public Dictionary<string, DataCenter> DataCenters { get; set; } = dataCenters;

    public long ToTotal()
    {
        return DataCenters.Values.Sum(dc => dc.ToTotal());
    }
    
    public void Draw()
    {
        foreach (var (name, dataCenter) in DataCenters)
        {
            ImGui.SetNextItemOpen(true);
            if (ImGui.TreeNode($"{name}: {dataCenter.ToTotal().AddSpacing()} gil###{name}")) 
            {
                dataCenter.Draw();
                ImGui.TreePop();
            }
        }
    }
}
