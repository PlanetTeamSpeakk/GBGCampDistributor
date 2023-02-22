using System.Collections.Immutable;

namespace GBGCampDistributor.Maps;

public class VolcanoArchipelagoMap : IMap
{
    public IDictionary<string, Province> Provinces { get; }

    public VolcanoArchipelagoMap()
    {
        Dictionary<string, Province> provinces = new()
        {
            // First ring
            {"A1M", new Province("A1M")},
            {"B1O", new Province("B1O")},
            {"C1N", new Province("C1N")},
            {"D1B", new Province("D1B")}
        };
        
        for (int i = 0; i < 4; i++)
        {
            char quarterId = (char) ('A' + i);
            
            // Second ring
            for (int j = 0; j < 2; j++)
            {
                string name = $"{quarterId}2{(char)('S' + j)}";
                provinces[name] = new Province(name);
            }

            // Third ring
            for (int j = 0; j < 4; j++)
            {
                string name = $"{quarterId}3{(char)('V' + (j == 0 ? 0 : j + 1))}";
                provinces[name] = new Province(name);
            }

            // Fourth ring
            for (int j = 0; j < 8; j++)
            {
                string name = $"{quarterId}4{(char)('A' + j)}";
                provinces[name] = new Province(name);
            }
        }

        #region Neighbours

        void Add(string province, params string[] neighbors)
        {
            foreach (string neighbor in neighbors) provinces[province].AddNeighbor(provinces[neighbor]);
        }

        // First ring
        Add("A1M", "A2S", "A2T", "B1O", "D1B");
        Add("B1O", "A1M", "B2S", "B2T", "C1N");
        Add("C1N", "D1B", "B1O", "C2S", "C2T");
        Add("D1B", "D2T", "A1M", "C1N", "D2S");

        
        // Second ring
        Add("A2S", "A3V", "A3X", "A2T", "A1M", "D2T");
        Add("A2T", "A3Y", "A3Z", "B2S", "A1M", "A2S");
        Add("B2S", "A2T", "B3V", "B3X", "B2T", "B1O");
        Add("B2T", "B1O", "B2S", "B3Y", "B3Z", "C2S");
        Add("C2S", "C1N", "B2T", "C3V", "C3X", "C2T");
        Add("C2T", "D2S", "C1N", "C2S", "C3Y", "C3Z");
        Add("D2S", "D2T", "D1B", "C2T", "D3V", "D3X");
        Add("D2T", "D3Y", "D3Z", "A2S", "D1B", "D2S");


        // Third ring
        Add("A3V", "A3X", "A2S", "D3Z");
        Add("A3X", "A3Y", "A2S", "A3V");
        Add("A3Y", "A3Z", "A2T", "A3X");
        Add("A3Z", "B3V", "A2T", "A3Y");
        Add("B3V", "B3X", "B2S", "A3Z");
        Add("B3X", "B3Y", "B2S", "B3V");
        Add("B3Y", "B3Z", "B2T", "B3X");
        Add("B3Z", "C3V", "B2T", "B3Y");
        Add("C3V", "C3X", "C2S", "B3Z");
        Add("C3X", "C3Y", "C2S", "C3V");
        Add("C3Y", "C3Z", "C2T", "C3X");
        Add("C3Z", "D3V", "C2T", "C3Y");
        Add("D3V", "D3X", "D2S", "C3V");
        Add("D3X", "D3Y", "D2S", "D3V");
        Add("D3Y", "D3Z", "D2T", "D3X");
        Add("D3Z", "A3V", "D2T", "D3Y");

        #endregion

        Provinces = provinces.ToImmutableDictionary();
    }
    
    public string IdToName(int id)
    {
        return id switch
        {
            // First ring (4 tiles)
            < 4 => $"{(char)('A' + id)}1{id switch {
                0 => 'M',
                1 => 'O',
                2 => 'N',
                3 => 'B',
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Impossible error")}}",
            // Second ring (8 tiles)
            < 12 => $"{(char)('A' + (id - 4) / 2)}2{(id % 2 == 0 ? 'S' : 'T')}",
            // Third ring (16 tiles)
            < 28 => $"{(char)('A' + (id - 12) / 4)}3{(id % 4 == 0 ? 'V' : (char)('W' + id % 4))}",
            // Fourth ring (32 tiles)
            < 60 => $"{(char)('A' + (id - 28) / 8)}4{(char)('A' + id % 8)}",
            _ => throw new ArgumentOutOfRangeException(nameof(id), "Id must be at most 59.")
        };
    }
}