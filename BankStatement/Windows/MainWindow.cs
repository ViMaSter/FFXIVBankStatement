using System;
using System.Numerics;
using BankStatement.Data;
using Dalamud.Interface.Windowing;

namespace BankStatement.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly AccountStanding currentStanding;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(AccountStanding currentStanding)
        : base("Bank Statement###BankStatement")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.currentStanding = currentStanding;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        currentStanding.Draw();
    }
}
