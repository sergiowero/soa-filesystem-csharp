using System;
using System.IO;
using System.Text;

namespace FileSystem
{
    public class DiskManager
    {
        private const int BLOCK_HEADER_SIZE = 3;
        private const int BLOCK_SIZE = 8;
        private const int BLOCK_PAYLOAD_SIZE = BLOCK_SIZE - BLOCK_HEADER_SIZE;
        private const int BLOCK_COUNT = short.MaxValue;

        private readonly static byte[] EMPTY_PAYLOAD = new byte[BLOCK_PAYLOAD_SIZE];

        private const string DISK_FILE = "disk.dat";

        private MemoryStream m_dataContainer;
        private BinaryReader m_reader;
        private BinaryWriter m_writer;
        private byte[] m_data;

        private short m_lastAvailable = 0;

        public DiskManager()
        {
            m_data = new byte[BLOCK_SIZE * BLOCK_COUNT];
            m_dataContainer = new MemoryStream(m_data);
            m_reader = new BinaryReader(m_dataContainer);
            m_writer = new BinaryWriter(m_dataContainer);
            //int[] blocks;
            //WriteNewBlocks(new byte[1500], out blocks);
            //DebugFile();
            //Reallocate(blocks[0], new byte[305], out blocks);
            //DebugFile("debug2.html");
        }

        public void Load()
        {
            FileStream fs = new FileStream(DISK_FILE, FileMode.OpenOrCreate);
            fs.Read(m_data, 0, m_data.Length);
            fs.Close();
        }

        public void Save()
        {
            FileStream fs = new FileStream(DISK_FILE, FileMode.OpenOrCreate);
            fs.Write(m_data, 0, m_data.Length);
            fs.Close();
        }

        public bool IsBlockAvailable(int _index)
        {
            MoveTo(_index);
            return !m_reader.ReadBoolean();
        }

        private void SetState(int _index, bool _state, short _next)
        {
            MoveTo(_index);
            m_writer.Write(_state);
            m_writer.Write(_next);
        }

        private void MoveTo(int _index)
        {
            m_dataContainer.Position = _index * BLOCK_SIZE;
        }

        public bool WriteNewBlocks(byte[] _data, out short[] _allocated)
        {
            int blocks = CalculateBlockCount(_data.Length);
            if(blocks == 0)
            {
                blocks++;
            }
            _allocated = new short[blocks];
            int allocatedIndex = 0;
            short initial = m_lastAvailable;
            short current = m_lastAvailable;
            short last = -1;
            do
            {
                if (IsBlockAvailable(current))
                {
                    _allocated[allocatedIndex++] = current;
                    m_lastAvailable = current;
                    if (last != -1)
                    {
                        SetState(last, true, current);
                    }
                    last = current;
                }
                current++;
                if (current == BLOCK_COUNT)
                {
                    current = 0;
                }
            }
            while (initial != current && allocatedIndex < _allocated.Length);

            if (last != -1)
            {
                SetState(last, true, -1);
            }

            if (allocatedIndex == _allocated.Length)
            {
                Write(_data, _allocated);
                return true;
            }

            return false;
        }

        public bool Reallocate(short _startBlock, byte[] _data, out short[] _blocks)
        {
            int blocks = CalculateBlockCount(_data.Length);
            _blocks = new short[blocks];

            int allocatedIndex = 0;
            short initial = _startBlock;
            short current = _startBlock;
            short last = -1;
            bool lookingfornew = false;
            do
            {
                MoveTo(current);
                bool available = m_reader.ReadBoolean();
                short next = m_reader.ReadInt16();

                if (available && !lookingfornew)
                {
                    _blocks[allocatedIndex++] = current;
                    last = current;
                }
                else if (IsBlockAvailable(current))
                {
                    _blocks[allocatedIndex++] = current;
                    m_lastAvailable = current;
                    if (last != -1)
                    {
                        SetState(last, true, current);
                    }
                    last = current;
                }
                if (available && next > -1)
                {
                    current = next;
                }
                else
                {
                    current++;
                    lookingfornew = true;
                }
                if (current == BLOCK_COUNT)
                {
                    current = 0;
                }
            }
            while (initial != current && allocatedIndex < _blocks.Length);

            int toClean = -1;
            if (last != -1)
            {
                MoveTo(last);
                bool available = m_reader.ReadBoolean();
                short next = m_reader.ReadInt16();
                if (available && next > -1)
                {
                    toClean = next;
                }
                SetState(last, true, -1);
            }

            if (toClean > -1)
            {
                CleanBlocks(toClean);
            }

            if (allocatedIndex == _blocks.Length)
            {
                Write(_data, _blocks);
                return true;
            }

            return false;
        }

        public void CleanBlocks(int _start)
        {
            int current = _start;
            int last = _start;
            do
            {
                MoveTo(current);
                m_reader.ReadBoolean();
                current = m_reader.ReadInt16();
                SetState(last, false, -1);
                m_writer.Write(EMPTY_PAYLOAD);
                last = current;
            }
            while (current > -1);
        }

        private void Write(byte[] _data, short[] _blocks)
        {
            int dataCounter = 0;
            for (int i = 0 ; i < _blocks.Length ; i++)
            {
                MoveTo(_blocks[i]);
                m_reader.ReadBoolean();
                m_reader.ReadInt16();
                for (int z = 0 ; z < BLOCK_PAYLOAD_SIZE && dataCounter < _data.Length ; z++, dataCounter++)
                {
                    m_dataContainer.WriteByte(_data[dataCounter]);
                }
            }
        }

        public int CalculateBlockCount(int _size)
        {
            int blockCount = _size / BLOCK_PAYLOAD_SIZE;
            if (_size % BLOCK_PAYLOAD_SIZE != 0)
                blockCount++;
            return blockCount;
        }

        public byte[] GetData(int _refIndex)
        {
            MemoryStream mem = new MemoryStream();
            int current = _refIndex;
            while(current > -1)
            {
                MoveTo(current);
                bool good = m_reader.ReadBoolean();
                short next = m_reader.ReadInt16();
                byte[] payload = m_reader.ReadBytes(BLOCK_PAYLOAD_SIZE);
                mem.Write(payload, 0, payload.Length);

                if(good)
                {
                    current = next;
                }
            }

            return mem.ToArray();
        }

        public void DebugFile(string file = "debug.html")
        {
            MoveTo(0);
            FileStream fs = new FileStream(file, FileMode.Create);
            StreamWriter writer = new StreamWriter(fs);
            int current = 0;
            writer.WriteLine("<table border=\"1\">");
            for (; current < BLOCK_COUNT ;)
            {
                writer.WriteLine("    <tr>");
                for (int i = 0 ; i < 8 && current < BLOCK_COUNT ; i++, current++)
                {
                    MoveTo(current);
                    byte[] data = m_reader.ReadBytes(BLOCK_SIZE);
                    string str = "" + BitConverter.ToBoolean(data, 0) + "," + BitConverter.ToInt16(data, 1) + ",";
                    for (int s = 5 ; s < data.Length ; s++)
                    {
                        str += "" + data[s];
                    }
                    writer.WriteLine("        <td>");
                    writer.WriteLine("            " + str + "");
                    writer.WriteLine("        </td>");
                }
                writer.WriteLine("    </tr>");
            }
            writer.WriteLine("</table>");

            writer.Close();
            fs.Close();
        }
    }
}