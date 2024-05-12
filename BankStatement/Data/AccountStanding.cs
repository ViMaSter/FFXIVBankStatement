using System.Collections.Generic;

namespace BankStatement.Data;

public record AccountStanding(Dictionary<string, AccountStanding.Region> regions)
{
    public record Region(Dictionary<string, Region.DataCenter> dataCenters)
    {
        public record DataCenter(Dictionary<string, DataCenter.World> worlds)
        {
            public record World(Dictionary<string, World.Character> characters)
            {
                public record Character(string Name, long Gil, Dictionary<string, Character.Retainer> Retainers)
                {
                    public record Retainer(string Name, long Gil)
                    {
                        public long Gil { get; set; } = Gil;
                    }

                    public long Gil { get; set; } = Gil;
                }

                public Dictionary<string, World.Character> characters { get; set; } = characters;
            }

            public Dictionary<string, DataCenter.World> worlds { get; set; } = worlds;
        }

        public Dictionary<string, Region.DataCenter> dataCenters { get; set; } = dataCenters;
    }

    public Dictionary<string, AccountStanding.Region> regions { get; set; } = regions;
}
