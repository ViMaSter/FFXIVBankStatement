namespace BankStatement.Data;

public class Retainer(long gil)
{
    public long Gil { get; set; } = gil;

    public long ToTotal()
    {
        return Gil;
    }
}
