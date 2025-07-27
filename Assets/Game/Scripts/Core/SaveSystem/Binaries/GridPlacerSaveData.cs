using System.Collections.Generic;
using System.IO;

public class GridPlacerSaveData : IBinarySerializable
{
    public List<PlacedObjectData> objects = new();

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(objects.Count);
        foreach (var obj in objects)
            obj.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        objects.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var obj = new PlacedObjectData();
            obj.Deserialize(reader);
            objects.Add(obj);
        }
    }
}
