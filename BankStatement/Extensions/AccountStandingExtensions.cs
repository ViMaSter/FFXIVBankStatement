using System;
using System.Collections.Generic;
using BankStatement.Data;

namespace BankStatement.Extensions;

public static class RegionConverter
{
    private static readonly Dictionary<byte, string> Regions = new() { { 1, "Japan" }, { 2, "North-America" }, { 3, "Europe" }, { 4, "Oceania" } };
    public static string GetRegionName(this byte regionId)
    {
        return Regions[regionId];
    }
}

public static class GilFormatting
{
    public static string AddSpacing(this long gil)
    {
        return gil.ToString("N0");
    }
}

public static class AccountStandingExtensions
{
    /// <summary>
    /// Persists new standing using <see cref="saveData"/>, if the standing has changed.
    /// </summary>
    public static void UpdateCharacterStanding(this AccountStanding accountStanding, string regionName, string dataCenterName, string worldName, string characterName, uint gil, Action saveData)
    {
        if (!accountStanding.Regions.TryGetValue(regionName, out var region))
        {
            region = new Region(new Dictionary<string, DataCenter>());
            accountStanding.Regions[regionName] = region;
        } 
        
        if (!region.DataCenters.TryGetValue(dataCenterName, out var dataCenter))
        {
            dataCenter = new DataCenter(new Dictionary<string, World>());
            region.DataCenters[dataCenterName] = dataCenter;
        }
        
        if (!dataCenter.Worlds.TryGetValue(worldName, out var world))
        {
            world = new World(new Dictionary<string, Character>());
            dataCenter.Worlds[worldName] = world;
        }
        
        if (!world.Characters.TryGetValue(characterName, out var character))
        {
            character = new Character(gil, new Dictionary<string, Retainer>());
            world.Characters[characterName] = character;
            saveData();
            return;
        }

        if (character.Gil == gil)
        {
            return;
        }
        
        character.Gil = gil;
        saveData();
    }
    
    /// <summary>
    /// Persists new standing using <see cref="saveData"/>, if the standing has changed.
    /// </summary>
    public static void UpdateRetainerStanding(this AccountStanding accountStanding, string regionName, string dataCenterName, string worldName, string characterName, string retainerName, uint gil, Action saveData)
    {
        if (!accountStanding.Regions.TryGetValue(regionName, out var region))
        {
            region = new Region(new Dictionary<string, DataCenter>());
            accountStanding.Regions[regionName] = region;
        } 
        
        if (!region.DataCenters.TryGetValue(dataCenterName, out var dataCenter))
        {
            dataCenter = new DataCenter(new Dictionary<string, World>());
            region.DataCenters[dataCenterName] = dataCenter;
        }
        
        if (!dataCenter.Worlds.TryGetValue(worldName, out var world))
        {
            world = new World(new Dictionary<string, Character>());
            dataCenter.Worlds[worldName] = world;
        }
        
        if (!world.Characters.TryGetValue(characterName, out var character))
        {
            character = new Character(gil, new Dictionary<string, Retainer>());
            world.Characters[characterName] = character;
        }
        
        if (!character.Retainers.TryGetValue(retainerName, out var retainer))
        {
            retainer = new Retainer(gil);
            character.Retainers[retainerName] = retainer;
            saveData();
            return;
        }

        if (retainer.Gil == gil)
        {
            return;
        }
        
        retainer.Gil = gil;
        saveData();
    }
}
