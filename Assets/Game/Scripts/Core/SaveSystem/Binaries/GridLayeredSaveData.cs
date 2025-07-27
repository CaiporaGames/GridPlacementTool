using System.Collections.Generic;
using System.IO;

public class GridLayeredSaveData : IBinarySerializable
{
    public Dictionary<string, GridPlacerSaveData> layers = new();

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(layers.Count);
        foreach (var kvp in layers)
        {
            writer.Write(kvp.Key); // layer name
            kvp.Value.Serialize(writer);
        }
    }

    public void Deserialize(BinaryReader reader)
    {
        layers.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString();
            var data = new GridPlacerSaveData();
            data.Deserialize(reader);
            layers[key] = data;
        }
    }
}
