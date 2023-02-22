using System.Collections.Immutable;

namespace GBGCampDistributor.Maps;

public class Province
{
    public ISet<Province> Neighbors => _neighbors.ToImmutableHashSet();
    private readonly ISet<Province> _neighbors = new HashSet<Province>();
    public string Name { get; }
    public int SlotCount { get; private set; }
    public bool Ours { get; private set; }
    public int DesiredCount
    {
        get => _desiredCount;
        set => _desiredCount = Math.Max(_desiredCount, value);
    }
    private int _desiredCount;

    public Province(string name) => Name = name;

    public Province(string name, int slotCount, bool ours)
    {
        Name = name;
        Init(slotCount, ours);
    }

    public void Init(int slotCount, bool ours)
    {
        SlotCount = slotCount;
        Ours = ours;
    }

    public void AddNeighbor(Province p)
    {
        _neighbors.Add(p);
        p._neighbors.Add(this); // Add directly to avoid StackOverflow
    }

    public void ForceSetDesired(int desired) => _desiredCount = desired;
}