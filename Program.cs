using System.Diagnostics;
using System.Text.Json.Nodes;
using GBGCampDistributor.Maps;

namespace GBGCampDistributor;

public static class Program
{
    internal static void Main()
    {
        // Ask user data
        int? guildId = null;
        do
        {
            Console.WriteLine("Guild id:");
            string? guildIdString = Console.ReadLine();

            if (guildIdString == null) continue;
            if (int.TryParse(guildIdString, out int guildId0)) guildId = guildId0;
        } while (guildId == null);

        if (!File.Exists("gbgdata.txt"))
        {
            Console.WriteLine("Could not find a file named 'gbgdata.txt'. Aborting.\n" +
                              "Press any key to exit.");
            Console.ReadKey();
            return;
        }
        

        // Parse data
        IMap? map = ParseMap(File.ReadAllText("gbgdata.txt"), guildId!.Value);
        if (map == null) return;
        Console.WriteLine("Data parsed, distributing camps...");

        
        // Distribute camps
        const int campTarget = 4;
        DistributeCamps(map, campTarget);
        

        // Print results
        int saved = 0;
        Console.WriteLine("Camps distributed, result: ");
        foreach (Province province in map.Provinces.Values
                     .Where(prov => prov is {Ours: true, SlotCount: > 0})
                     .OrderBy(province => province.Id))
        {
            Console.WriteLine($" - {province.Name}: {province.DesiredCount}/{province.SlotCount}");
            saved += province.SlotCount - province.DesiredCount;
        }
        
        Console.WriteLine("Camps saved: " + saved);

        foreach (Province province in map.Provinces.Values.Where(prov => !prov.Ours && prov.Neighbors.Sum(n => n.DesiredCount) > campTarget))
        {
            int totalCamps = province.Neighbors.Sum(n => n.DesiredCount);

            switch (totalCamps)
            {
                case > campTarget:
                    Console.WriteLine($"Overshot {province.Name}: {totalCamps}");
                    break;
                case < campTarget:
                    Console.WriteLine($"Undershot {province.Name}: {totalCamps}");
                    break;
            }
        }
    }

    private static IMap? ParseMap(string gbgData, int guildId)
    {
        JsonArray? data = null;
        try
        {
            data = JsonNode.Parse(gbgData)?.AsArray();
        }
        catch
        {
            // ignored
        }

        if (data == null)
        {
            Console.WriteLine("Invalid data! Press any key to exit.");
            Console.ReadKey();
            return null;
        }
        
        foreach (JsonNode? node in data)
        {
            // Ensure this is the data we're looking for.
            if (node is not JsonObject o || o["requestClass"]?.ToString() != "GuildBattlegroundService" ||
                o["requestMethod"]?.ToString() != "getBattleground") continue;

            JsonObject resp = o["responseData"]!.AsObject();

            int? pid = null;
            JsonArray participants = resp["battlegroundParticipants"]!.AsArray();
            foreach (JsonNode? participant in participants)
            {
                if (participant is not JsonObject po) continue;

                JsonObject clan = po["clan"]!.AsObject();
                if (clan["id"]!.GetValue<int>() == guildId) pid = po["participantId"]!.GetValue<int>();
            }

            if (pid == null)
            {
                Console.WriteLine("Invalid guild id! Could not find a participant with the given guild id in the given data.\n" +
                                  "Press any key to exit.");
                Console.ReadKey();
                return null;
            }

            JsonObject mapData = resp["map"]!.AsObject();
            if (mapData["id"]!.ToString() != "volcano_archipelago")
            {
                Console.WriteLine("Invalid map! This map is not yet supported, please contact PlanetTeamSpeak#4157 on Discord about this.\n" +
                                  "Press any key to exit.");
                Console.ReadKey();
                return null;
            }

            IMap map = new VolcanoArchipelagoMap(); // TODO support other map
            
            JsonArray provincesData = mapData["provinces"]!.AsArray();
            foreach (JsonNode? province in provincesData)
            {
                if (province is not JsonObject po) continue;

                int id = province["id"]?.GetValue<int>() ?? 0; // First province is missing the id key.
                string name = map.IdToName(id);
                
                map.Provinces[name].Init(po["isSpawnSpot"]?.GetValue<bool>() ?? false ? 0 : // No camp slots on spawn spots
                    po["totalBuildingSlots"]?.GetValue<int>() ?? 0, po["ownerId"]!.GetValue<int>() == pid);
            }

            return map;
        }

        Console.WriteLine("Could not find a map in the given data. Aborting.\n" +
                          "Press any key to exit.");
        Console.ReadKey();
        return null;
    }

    private static void DistributeCamps(IMap map, int campTarget)
    {
        foreach (Province p in map.Provinces.Values)
        {
            if (p.Ours) continue;

            IList<Province> ours = p.Neighbors.Where(n => n.Ours).ToList();
            int totalCC = ours.Select(op => op.SlotCount).Sum();

            // We cannot achieve the desired amount of camps.
            // Simply give each neighboring province the maximum amount of camps it can hold.
            if (totalCC <= campTarget)
            {
                foreach (Province neighbour in ours)
                    neighbour.DesiredCount = neighbour.SlotCount;
                continue;
            }

            // Calculate how many camps we need to reach the target of 4.
            // Take the camps we've already placed into account as well.
            int campsLeft = ours.Aggregate(campTarget, (current, neighbour) => current - neighbour.DesiredCount);
            // Order by desired count ascending so we add camps to tiles
            // with the least amount of slots filled first.
            ours = ours.OrderBy(p1 => p1.DesiredCount).ToList();

            while (campsLeft > 0)
            {
                foreach (Province neighbor in ours)
                {
                    // If this province can't hold any more camps, continue to the next.
                    if (neighbor.SlotCount == neighbor.DesiredCount) continue;
                    
                    campsLeft -= 1;
                    neighbor.DesiredCount += 1;

                    Debug.Assert(campsLeft >= 0);
                    if (campsLeft == 0) break;
                }
            }
        }
    }
}