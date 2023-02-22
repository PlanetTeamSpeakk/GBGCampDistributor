using System.Diagnostics;
using GBGCampDistributor.Maps;

namespace GBGCampDistributor;

public static class Program
{
    internal static void Main()
    {
        IMap map = new VolcanoArchipelagoMap();
        IList<Province> provinces = map.Provinces.ToList();

        const int campTarget = 4;
        foreach (Province p in provinces)
        {
            if (p.Ours) continue;

            IList<Province> ours = p.Neighbors.Where(n => n.Ours).ToList();
            int totalCC = ours.Select(op => op.SlotCount).Sum();

            // We cannot achieve the desired amount of camps.
            // Simply give each province the maximum amount of camps it can hold.
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

        int saved = 0;
        Console.WriteLine("Result: ");
        foreach (Province province in provinces.Where(prov => prov.Ours))
        {
            Console.WriteLine($" - {province.Name}: {province.DesiredCount}/{province.SlotCount}");
            saved += province.SlotCount - province.DesiredCount;
        }
        
        Console.WriteLine("Camps saved: " + saved);

        foreach (Province province in provinces.Where(prov => !prov.Ours && prov.Neighbors.Sum(n => n.DesiredCount) > campTarget))
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
}