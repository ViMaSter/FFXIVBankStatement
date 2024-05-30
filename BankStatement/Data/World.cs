using System.Collections.Generic;
using System.Linq;
using BankStatement.Extensions;
using ImGuiNET;

namespace BankStatement.Data;

public class World(Dictionary<string, Character> characters)
{
    public Dictionary<string, Character> Characters { get; set; } = characters;

    public long ToTotal()
    {
        return Characters.Values.Sum(character => character.ToTotal());
    }

    public void Draw()
    {
        foreach (var (name, character) in Characters)
        {
            ImGui.SetNextItemOpen(true);
            if (ImGui.TreeNode($"{name} ({character.ToTotal().AddSpacing()} gil)###{name}"))
            {
                character.Draw();
                ImGui.TreePop();
            }
        }
    }
}
