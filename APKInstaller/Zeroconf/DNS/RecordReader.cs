using System.Collections.Generic;
using System.Text;

namespace Zeroconf.DNS
{
    internal class RecordReader
    {
        private readonly byte[] m_Data;

        public RecordReader(byte[] data)
        {
            m_Data = data;
            Position = 0;
        }

        public int Position { get; set; }

        public int Length => m_Data == null ? 0 : m_Data.Length;

        public RecordReader(byte[] data, int Position)
        {
            m_Data = data;
            this.Position = Position;
        }


        public byte ReadByte()
        {
            return Position >= m_Data.Length ? (byte)0 : m_Data[Position++];
        }

        public ushort ReadUInt16()
        {
            return (ushort)((ReadByte() << 8) | ReadByte());
        }

        public ushort ReadUInt16(int offset)
        {
            Position += offset;
            return ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return (uint)((ReadUInt16() << 16) | ReadUInt16());
        }

        public string ReadDomainName()
        {
            List<byte> bytes = new();
            int length;

            // get  the length of the first label
            while ((length = ReadByte()) != 0)
            {
                // top 2 bits set denotes domain name compression and to reference elsewhere
                if ((length & 0xc0) == 0xc0)
                {
                    // work out the existing domain name, copy this pointer
                    RecordReader newRecordReader = new(m_Data, ((length & 0x3f) << 8) | ReadByte());
                    return bytes.Count > 0
                        ? Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count) + newRecordReader.ReadDomainName()
                        : newRecordReader.ReadDomainName();
                }

                // if not using compression, copy a char at a time to the domain name
                while (length > 0)
                {
                    bytes.Add(ReadByte());
                    length--;
                }
                bytes.Add((byte)'.');
            }
            return bytes.Count == 0 ? "." : Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        public string ReadString()
        {
            short length = ReadByte();
            List<byte> bytes = new();
            for (int i = 0; i < length; i++)
            {
                bytes.Add(ReadByte());
            }

            return Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        // changed 28 augustus 2008
        public byte[] ReadBytes(int intLength)
        {
            byte[] list = new byte[intLength];
            for (int intI = 0; intI < intLength; intI++)
            {
                list[intI] = ReadByte();
            }

            return list;
        }

        public Record ReadRecord(Type type, int Length)
        {
            return type switch
            {
                Type.A => new RecordA(this),
                Type.PTR => new RecordPTR(this),
                Type.TXT => new RecordTXT(this, Length),
                Type.AAAA => new RecordAAAA(this),
                Type.SRV => new RecordSRV(this),
                Type.NSEC => new RecordNSEC(this),
                _ => new RecordUnknown(this),
            };
        }
    }
}
