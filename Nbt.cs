// an implementation of nbt format reader/writer
// NBTFile only supports .ToBytes() and .FromBytes() methods because i am lazy to do others
// not sure does it actually work i tested it last time like 4 months ago

namespace Nbt;

public enum CompressionType {
    GZipCompressed = 1,
    ZLibCompressed = 2,
    Uncompressed = 3
}

internal class BinaryReader2(Stream stream) : BinaryReader(stream, System.Text.Encoding.UTF8, true) {
    private Span<byte> ReadBytesReversed(int count) {
        var data = base.ReadBytes(count);
        Array.Reverse(data);
        return new Span<byte>(data);
    }
    public override short ReadInt16() {
        return BitConverter.ToInt16(ReadBytesReversed(2));
    }

    public override ushort ReadUInt16() {
        return BitConverter.ToUInt16(ReadBytesReversed(2));
    }

    public uint ReadUInt24() {
        return (uint)ReadByte() << 16 | (uint)ReadByte() << 8 | ReadByte();
    }

    public override uint ReadUInt32() {
        return BitConverter.ToUInt32(ReadBytesReversed(4));
    }

    public override int ReadInt32() {
        return BitConverter.ToInt32(ReadBytesReversed(4));
    }

    public override long ReadInt64() {
        return BitConverter.ToInt64(ReadBytesReversed(8));
    }

    public override float ReadSingle() {
        return BitConverter.ToSingle(ReadBytesReversed(4));
    }

    public override double ReadDouble() {
        return BitConverter.ToDouble(ReadBytesReversed(8));
    }
}

internal class BinaryWriter2(Stream stream) : BinaryWriter(stream) {
    public override void Write(short value) {
        var data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(ushort value) {
        var data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public void WriteUInt24(uint value) {
        Write((byte)value);
        Write((byte)(value >> 8));
        Write((byte)(value >> 16));
    }

    public override void Write(uint value) {
        var data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(int value) {
        var data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(long value) {
        var data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(float value) {
        var data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(double value) {
        var data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }
}

public enum TAG_ID : byte {
    TAG_End,
    TAG_Byte,
    TAG_Short,
    TAG_Int,
    TAG_Long,
    TAG_Float,
    TAG_Double,
    TAG_Byte_Array,
    TAG_String,
    TAG_List,
    TAG_Compound,
    TAG_Int_Array,
    TAG_Long_Array
}

public interface ITag {
    TAG_ID Id { get; }
    string Name { get; }
}

public abstract class TAG<T>(string name, T value) : ITag {
    public abstract TAG_ID Id { get; }
    public string Name { get; set; } = name;
    public T Value { get; } = value;
}

public abstract class TAG_Array<T>(string name, List<T> value) : TAG<List<T>>(name, value), IList<T> {
    public void Add(T item) => Value.Add(item);
    public void Clear() => Value.Clear();
    public bool Contains(T item) => Value.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => Value.CopyTo(array, arrayIndex);
    public int Count => Value.Count;
    public bool IsReadOnly => false;
    public bool Remove(T item) => Value.Remove(item);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Value.GetEnumerator();
    public int IndexOf(T item) => Value.IndexOf(item);
    public IEnumerator<T> GetEnumerator() => Value.GetEnumerator();
    public void Insert(int index, T item) => Value.Insert(index, item);
    public void RemoveAt(int index) => Value.RemoveAt(index);
    public T this[int index] {
        get => Value[index];
        set => Value[index] = value;
    }
}

public class TAG_End() : TAG<object?>("", null) {
    public override TAG_ID Id => TAG_ID.TAG_End;
}

public class TAG_Byte(string name, byte value) : TAG<byte>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Byte;
}

public class TAG_Short(string name, short value) : TAG<short>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Short;
}

public class TAG_Int(string name, int value) : TAG<int>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Int;
}

public class TAG_Long(string name, long value) : TAG<long>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Long;
}

public class TAG_Float(string name, float value) : TAG<float>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Float;
}

public class TAG_Double(string name, double value) : TAG<double>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Double;
}

public class TAG_Byte_Array(string name, List<sbyte> value) : TAG_Array<sbyte>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Byte_Array;
}

public class TAG_String(string name, string value) : TAG<string>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_String;
}

public class TAG_List(string name, List<ITag> value, TAG_ID tagsId) : TAG_Array<ITag>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_List;
    public TAG_ID tagsId = tagsId;
}

public class TAG_Compound(string name, List<ITag> value) : TAG_Array<ITag>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Compound;
}

public class TAG_Int_Array(string name, List<int> value) : TAG_Array<int>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Int_Array;
}

public class TAG_Long_Array(string name, List<long> value) : TAG_Array<long>(name, value) {
    public override TAG_ID Id => TAG_ID.TAG_Long_Array;
}

public class NBTFile(string name, List<ITag> value) : TAG_Compound(name, value) {
    private static string ReadString(BinaryReader2 reader) {
        ushort length = reader.ReadUInt16();
        string s = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(length));
        return s;
    }

    private static TAG_List ReadList(string name, BinaryReader2 reader) {
        TAG_ID id = (TAG_ID)reader.ReadByte();
        int length = reader.ReadInt32();
        var list = new List<ITag>(length);

        for (int i = 0; i < length; i++)
            list.Add(ReadTag(id, "", reader));

        return new TAG_List(name, list, id);
    }

    private static TAG_Compound ReadCompound(string name, BinaryReader2 reader) {
        List<ITag> tags = [];
        while (true) {
            ITag tag = ReadTagWithPrefix(reader);
            if (tag.Id == TAG_ID.TAG_End) break;
            tags.Add(tag);
        }
        return new TAG_Compound(name, tags);
    }

    private static TAG_Byte_Array ReadByteArray(string name, BinaryReader2 reader) {
        int length = reader.ReadInt32();
        var list = new List<sbyte>(length);

        for (int i = 0; i < length; i++)
            list.Add(reader.ReadSByte());

        return new TAG_Byte_Array(name, list);
    }

    private static TAG_Int_Array ReadIntArray(string name, BinaryReader2 reader) {
        int length = reader.ReadInt32();
        var list = new List<int>(length);

        for (int i = 0; i < length; i++)
            list.Add(reader.ReadInt32());

        return new TAG_Int_Array(name, list);
    }

    private static TAG_Long_Array ReadLongArray(string name, BinaryReader2 reader) {
        int length = reader.ReadInt32();
        var list = new List<long>(length);

        for (int i = 0; i < length; i++) {
            list.Add(reader.ReadInt64());
        }

        return new TAG_Long_Array(name, list);
    }

    private static ITag ReadTag(TAG_ID id, string name, BinaryReader2 reader) => id switch {
        TAG_ID.TAG_End => new TAG_End(),
        TAG_ID.TAG_Byte => new TAG_Byte(name, reader.ReadByte()),
        TAG_ID.TAG_Short => new TAG_Short(name, reader.ReadInt16()),
        TAG_ID.TAG_Int => new TAG_Int(name, reader.ReadInt32()),
        TAG_ID.TAG_Long => new TAG_Long(name, reader.ReadInt64()),
        TAG_ID.TAG_Float => new TAG_Float(name, reader.ReadSingle()),
        TAG_ID.TAG_Double => new TAG_Double(name, reader.ReadDouble()),
        TAG_ID.TAG_Byte_Array => ReadByteArray(name, reader),
        TAG_ID.TAG_String => new TAG_String(name, ReadString(reader)),
        TAG_ID.TAG_List => ReadList(name, reader),
        TAG_ID.TAG_Compound => ReadCompound(name, reader),
        TAG_ID.TAG_Int_Array => ReadIntArray(name, reader),
        TAG_ID.TAG_Long_Array => ReadLongArray(name, reader),
        _ => throw new NotSupportedException($"Unknown tag type: {id}"),
    };

    private static ITag ReadTagWithPrefix(BinaryReader2 reader) {
        TAG_ID id = (TAG_ID)reader.ReadByte();
        string name = id == TAG_ID.TAG_End ? "" : ReadString(reader);
        return ReadTag(id, name, reader);
    }

    private static NBTFile FromReader(BinaryReader2 reader) {
        var tags = ReadTagWithPrefix(reader);
        if (tags.Id == TAG_ID.TAG_Compound) return new NBTFile(tags.Name, ((TAG_Compound)tags).Value);
        throw new FormatException("The file is not TAG_Compound.");
    }

    public static NBTFile FromBytes(byte[] bytes) => FromReader(new BinaryReader2(bytes[0] switch {
        0x0A => new MemoryStream(bytes),
        0x1F when bytes[1] == 0x8B => new System.IO.Compression.GZipStream(new MemoryStream(bytes), System.IO.Compression.CompressionMode.Decompress),
        0x78 when bytes[1] == 0x9C || bytes[1] == 0xDA => new System.IO.Compression.ZLibStream(new MemoryStream(bytes), System.IO.Compression.CompressionMode.Decompress),
        _ => throw new FormatException("Unknown compression type.")
    }));

    private static void WriteString(string s, BinaryWriter2 writer) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    private static void WriteList(TAG_List list, BinaryWriter2 writer) {
        writer.Write((byte)list.tagsId);
        writer.Write(list.Count);

        foreach (ITag tag in list) {
            if (tag.Id != list.tagsId)
                throw new ArgumentException($"Expected {list.tagsId} in TAG_List, not {tag.Id}");
            WriteTag(tag, writer);
        }
    }

    private static void WriteCompound(TAG_Compound compound, BinaryWriter2 writer) {
        foreach (ITag tag in compound)
            WriteTagWithPrefix(tag, writer);
        writer.Write((byte)TAG_ID.TAG_End);
    }

    private static void WriteByteArray(TAG_Byte_Array byteArray, BinaryWriter2 writer) {
        writer.Write(byteArray.Count);
        
        foreach (sbyte b in byteArray)
            writer.Write(b);
    }

    private static void WriteIntArray(TAG_Int_Array intArray, BinaryWriter2 writer) {
        writer.Write(intArray.Count);

        foreach (int i in intArray) {
            writer.Write(i);
        }
    }

    private static void WriteLongArray(TAG_Long_Array longArray, BinaryWriter2 writer) {
        writer.Write(longArray.Count);

        foreach (long l in longArray)
            writer.Write(l);
    }

    private static void WriteTag(ITag tag, BinaryWriter2 writer) {
        switch (tag.Id) {
            case TAG_ID.TAG_Byte:
                writer.Write(((TAG_Byte)tag).Value);
                break;
            case TAG_ID.TAG_Short:
                writer.Write(((TAG_Short)tag).Value);
                break;
            case TAG_ID.TAG_Int:
                writer.Write(((TAG_Int)tag).Value);
                break;
            case TAG_ID.TAG_Long:
                writer.Write(((TAG_Long)tag).Value);
                break;
            case TAG_ID.TAG_Float:
                writer.Write(((TAG_Float)tag).Value);
                break;
            case TAG_ID.TAG_Double:
                writer.Write(((TAG_Double)tag).Value);
                break;
            case TAG_ID.TAG_Byte_Array:
                WriteByteArray((TAG_Byte_Array)tag, writer);
                break;
            case TAG_ID.TAG_String:
                WriteString(((TAG_String)tag).Value, writer);
                break;
            case TAG_ID.TAG_List:
                WriteList((TAG_List)tag, writer);
                break;
            case TAG_ID.TAG_Compound:
                WriteCompound((TAG_Compound)tag, writer);
                break;
            case TAG_ID.TAG_Int_Array:
                WriteIntArray((TAG_Int_Array)tag, writer);
                break;
            case TAG_ID.TAG_Long_Array:
                WriteLongArray((TAG_Long_Array)tag, writer);
                break;
            case TAG_ID.TAG_End:
            default:
                throw new NotSupportedException($"Unknown tag type: {tag.Id}");
        }
    }

    private static void WriteTagWithPrefix(ITag tag, BinaryWriter2 writer) {
        writer.Write((byte)tag.Id);
        WriteString(tag.Name, writer);
        WriteTag(tag, writer);
    }

    public byte[] ToBytes(CompressionType compression = CompressionType.GZipCompressed) {
        MemoryStream memoryStream = new();
        WriteTagWithPrefix(this, new BinaryWriter2(compression switch {
            CompressionType.Uncompressed => memoryStream,
            CompressionType.GZipCompressed => new System.IO.Compression.GZipStream(memoryStream, System.IO.Compression.CompressionMode.Compress),
            CompressionType.ZLibCompressed => new System.IO.Compression.ZLibStream(memoryStream, System.IO.Compression.CompressionMode.Compress),
            _ => throw new ArgumentOutOfRangeException(nameof(compression), compression, null)
        }));
        return memoryStream.ToArray();
    }
}
