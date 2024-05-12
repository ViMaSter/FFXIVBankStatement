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
        if (!accountStanding.regions.TryGetValue(regionName, out var region))
        {
            region = new AccountStanding.Region(new Dictionary<string, AccountStanding.Region.DataCenter>());
            accountStanding.regions[regionName] = region;
        } 
        
        if (!region.dataCenters.TryGetValue(dataCenterName, out var dataCenter))
        {
            dataCenter = new AccountStanding.Region.DataCenter(new Dictionary<string, AccountStanding.Region.DataCenter.World>());
            region.dataCenters[dataCenterName] = dataCenter;
        }
        
        if (!dataCenter.worlds.TryGetValue(worldName, out var world))
        {
            world = new AccountStanding.Region.DataCenter.World(new Dictionary<string, AccountStanding.Region.DataCenter.World.Character>());
            dataCenter.worlds[worldName] = world;
        }
        
        if (!world.characters.TryGetValue(characterName, out var character))
        {
            character = new AccountStanding.Region.DataCenter.World.Character(characterName, gil, new Dictionary<string, AccountStanding.Region.DataCenter.World.Character.Retainer>());
            world.characters[characterName] = character;
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
        if (!accountStanding.regions.TryGetValue(regionName, out var region))
        {
            region = new AccountStanding.Region(new Dictionary<string, AccountStanding.Region.DataCenter>());
            accountStanding.regions[regionName] = region;
        } 
        
        if (!region.dataCenters.TryGetValue(dataCenterName, out var dataCenter))
        {
            dataCenter = new AccountStanding.Region.DataCenter(new Dictionary<string, AccountStanding.Region.DataCenter.World>());
            region.dataCenters[dataCenterName] = dataCenter;
        }
        
        if (!dataCenter.worlds.TryGetValue(worldName, out var world))
        {
            world = new AccountStanding.Region.DataCenter.World(new Dictionary<string, AccountStanding.Region.DataCenter.World.Character>());
            dataCenter.worlds[worldName] = world;
        }
        
        if (!world.characters.TryGetValue(characterName, out var character))
        {
            character = new AccountStanding.Region.DataCenter.World.Character(characterName, gil, new Dictionary<string, AccountStanding.Region.DataCenter.World.Character.Retainer>());
            world.characters[characterName] = character;
        }
        
        if (!character.Retainers.TryGetValue(retainerName, out var retainer))
        {
            retainer = new AccountStanding.Region.DataCenter.World.Character.Retainer(retainerName, gil);
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
