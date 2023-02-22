using System.Collections.Immutable;

namespace GBGCampDistributor.Maps;

public class Province
{
    public ISet<Province> Neighbors => _neighbors.ToImmutableHashSet();
    private readonly ISet<Province> _neighbors = new HashSet<Province>();
    public int Id { get; }
    public string Name { get; }
    public int SlotCount { get; private set; }
    public bool Ours { get; private set; }
    public bool IsSpawnSpot { get; private set; }
    public int DesiredCount
    {
        get => _desiredCount;
        set => _desiredCount = Math.Max(_desiredCount, value);
    }
    private int _desiredCount;

    public Province(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public void Init(int slotCount, bool ours, bool isSpawnSpot)
    {
        SlotCount = slotCount;
        Ours = ours;
        IsSpawnSpot = isSpawnSpot;
    }

    public void AddNeighbor(Province p)
    {
        _neighbors.Add(p);
        p._neighbors.Add(this); // Add directly to avoid StackOverflow
    }
}