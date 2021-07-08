using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  public class BinarySerializer : IDisposable {

    private readonly BinaryWriterEx _writer;
    private readonly BinaryReaderEx _reader;

    public delegate void ElementSerializer<T>(BinarySerializer serializer, ref T element);

    private sealed class BinaryWriterEx : BinaryWriter {
      public BinaryWriterEx(Stream stream, System.Text.Encoding encoding, bool leaveOpen) : base(stream, encoding, leaveOpen) { }

      public void Write7BitEncoded(int value) => base.Write7BitEncodedInt(value);
      public void Write7BitEncoded(long value) {
        ulong num;
        for (num = (ulong)value; num >= 128; num >>= 7) {
          Write((byte)(num | 0x80));
        }
        Write((byte)num);
      }
    }

    private sealed class BinaryReaderEx : BinaryReader {
      public BinaryReaderEx(Stream stream, System.Text.Encoding encoding, bool leaveOpen) : base(stream, encoding, leaveOpen) { }

      public int Read7BitEncodedInt32() => base.Read7BitEncodedInt();

      public long Read7BitEncodedInt64() {
        long num = 0;
        int num2 = 0;
        byte b;
        do {
          if (num2 == 70) {
            throw new InvalidOperationException();
          }
          b = ReadByte();
          num |= ((long)(b & 0x7F)) << num2;
          num2 += 7;
        }
        while ((b & 0x80) != 0);
        return num;
      }
    }

    public BinarySerializer(Stream stream, bool writing, Encoding encoding, bool leaveOpen) {
      if (writing) {
        _writer = new BinaryWriterEx(stream, encoding, leaveOpen);
      } else {
        _reader = new BinaryReaderEx(stream, encoding, leaveOpen);
      }
    }

    public BinarySerializer(Stream stream, bool writing) : this(stream, writing, Encoding.Default, false) { }

    public bool IsWriting => _writer != null;
    public bool IsReading => !IsWriting;

    public void Dispose() {
      if (_writer != null) {
        _writer.Dispose();
      } else {
        _reader.Dispose();
      }
    }

    public void Serialize(ref byte value) {
      if (_writer != null) {
        _writer.Write(value);
      } else {
        value = _reader.ReadByte();
      }
    }

    public void Serialize(ref bool value) {
      if (_writer != null) {
        _writer.Write(value);
      } else {
        value = _reader.ReadBoolean();
      }
    }

    public void Serialize(ref float value) {
      if (_writer != null) {
        _writer.Write(value);
      } else {
        value = _reader.ReadSingle();
      }
    }

    public void Serialize(ref Color value) {
      Serialize(ref value.r);
      Serialize(ref value.g);
      Serialize(ref value.b);
      Serialize(ref value.a);
    }



    public void Serialize(ref string value) {
      if (_writer != null) {
        _writer.Write(value);
      } else {
        value = _reader.ReadString();
      }
    }

    public void Serialize(ref int value) {
      if (_writer != null) {
        _writer.Write(value);
      } else {
        value = _reader.ReadInt32();
      }
    }

    public void Serialize(ref long value) {
      if (_writer != null) {
        _writer.Write(value);
      } else {
        value = _reader.ReadInt64();
      }
    }

    public void Serialize(ref byte[] value) {
      if (_writer != null) {
        _writer.Write7BitEncoded(value.Length);
        _writer.Write(value);
      } else {
        int count = _reader.Read7BitEncodedInt32();
        value = _reader.ReadBytes(count);
      }
    }

    public void Serialize<T>(ref T value, Func<T, int> toInt, Func<int, T> fromInt) {
      if (_writer != null) {
        _writer.Write(toInt(value));
      } else {
        value = fromInt(_reader.ReadInt32());
      }
    }

    public void Serialize<T>(ref T value, Func<T, byte[]> toBytes, Func<byte[], T> fromBytes) {
      if (_writer != null) {
        var bytes = toBytes(value);
        _writer.Write7BitEncoded(bytes.Length);
        _writer.Write(bytes);
      } else {
        int count = _reader.Read7BitEncodedInt32();
        var bytes = _reader.ReadBytes(count);
        value = fromBytes(bytes);
      }
    }

    public void Serialize7BitEncoded(ref int value) {
      if (_writer != null) {
        _writer.Write7BitEncoded(value);
      } else {
        value = _reader.Read7BitEncodedInt32();
      }
    }

    public void Serialize7BitEncoded(ref long value) {
      if (_writer != null) {
        _writer.Write7BitEncoded(value);
      } else {
        value = _reader.Read7BitEncodedInt64();
      }
    }

    public void SerializeList<T>(ref List<T> list, ElementSerializer<T> serializer) where T : new() {
      if (_writer != null) {
        _writer.Write(list != null ? list.Count : 0);
        if (list != null) {
          for (int i = 0; i < list.Count; ++i) {
            var element = list[i];
            serializer(this, ref element);
          }
        }
      } else {

        if (list == null) {
          list = new List<T>();
        } else {
          list.Clear();
        }

        var count = _reader.ReadInt32();
        list.Capacity = count;

        for (int i = 0; i < count; ++i) {
          var element = new T();
          serializer(this, ref element);
          list.Add(element);
        }
      }
    }
  }
}


