namespace GBGCampDistributor.Maps;

public interface IMap
{
    public IEnumerable<Province> Provinces { get; }

    /// <summary>
    /// Converts a province id to a province name.
    /// This is dependent on the map, but other than that, follows a logical pattern.
    /// </summary>
    /// <param name="id">The id of the province</param>
    /// <returns>The name of the province</returns>
    public string IdToName(int id);
}