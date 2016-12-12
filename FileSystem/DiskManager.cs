using System.IO;
using System.Text;

namespace FileSystem
{
    public class DiskManager
    {
        private const int BLOCK_HEADER_SIZE = 5;
        private const int BLOCK_SIZE = 16;
        private const int BLOCK_PAYLOAD_SIZE = BLOCK_SIZE - BLOCK_HEADER_SIZE;
        private const int BLOCK_COUNT = 2048;

        private const string DISK_FILE = "disk.dat";

        private MemoryStream m_dataContainer;
        private BinaryReader m_reader;
        private BinaryWriter m_writer;
        private byte[] m_data;

        private int m_lastAvailable = 0;

        public DiskManager()
        {
            m_data = new byte[BLOCK_SIZE * BLOCK_COUNT];
            m_dataContainer = new MemoryStream(m_data);
            m_reader = new BinaryReader(m_dataContainer);
            m_writer = new BinaryWriter(m_dataContainer);
            int[] blocks;
            WriteNewBlocks(new byte[6489], out blocks);
            DebugFile();
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

        private void SetState(int _index, bool _state, int _next)
        {
            MoveTo(_index);
            m_writer.Write(_state);
            m_writer.Write(_next);
        }

        private void MoveTo(int _index)
        {
            m_dataContainer.Position = _index * BLOCK_SIZE;
        }

        public bool WriteNewBlocks(byte[] _data, out int[] _allocated)
        {
            int blocks = CalculateBlockCount(_data.Length);
            _allocated = new int[blocks];
            int allocatedIndex = 0;
            int initial = m_lastAvailable;
            int current = m_lastAvailable;
            int last = -1;
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

        public bool Reallocate(int _startBlock, byte[] _data, out int[] _blocks)
        {
            int blocks = CalculateBlockCount(_data.Length);
            _blocks = new int[blocks];
            
            //Clean blocks

            int allocatedIndex = 0;
            int initial = m_lastAvailable;
            int current = m_lastAvailable;
            int last = -1;
            do
            {
                if (IsBlockAvailable(current))
                {

                    _blocks[allocatedIndex++] = current;
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
            while (initial != current && allocatedIndex < _blocks.Length);

            if (last != -1)
            {
                SetState(last, true, -1);
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

        }

        private void Write(byte[] _data, int[] _blocks)
        {
            int dataCounter = 0;
            for (int i = 0 ; i < _blocks.Length ; i++)
            {
                MoveTo(_blocks[i]);
                m_reader.ReadBoolean();
                m_reader.ReadInt32();
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

        public void DebugFile()
        {
            MoveTo(0);
            FileStream fs = new FileStream("debug.html", FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(fs);
            int current = 0;
            writer.WriteLine("<table>");
            for (; current < BLOCK_COUNT ;)
            {
                writer.WriteLine("    <tr>");
                for (int i = 0 ; i < 6 && current < BLOCK_COUNT ; i++, current++)
                {
                    MoveTo(current);
                    byte[] data = m_reader.ReadBytes(BLOCK_SIZE);
                    string str = "";
                    for(int s = 0 ; s < data.Length ; s++)
                    {
                        str += "" + data[s];
                    }
                    writer.WriteLine("        <td>");
                    writer.WriteLine("            |" + str + "|");
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