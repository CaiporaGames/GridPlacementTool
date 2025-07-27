using System.IO;
using UnityEngine;

public class PlacedObjectData : IBinarySerializable
{
    public Vector3Int cell;
    public int prefabIndex;
    public Vector3 rotation;
    public Vector3 scale;

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(cell.x);
        writer.Write(cell.y);
        writer.Write(cell.z);
        writer.Write(prefabIndex);
        writer.Write(rotation.x);
        writer.Write(rotation.y);
        writer.Write(rotation.z);
        writer.Write(scale.x);
        writer.Write(scale.y);
        writer.Write(scale.z);
    }

    public void Deserialize(BinaryReader reader)
    {
        cell = new Vector3Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        prefabIndex = reader.ReadInt32();
        rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}
