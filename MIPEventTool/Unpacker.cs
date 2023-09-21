using System;
using System.IO;
using System.Linq;

namespace MIPEventTool
{
	public class Unpacker
	{
        public uint basePointer = 0x80100000;

        //unpack file using the table
        public Unpacker(string filename, string output, UnpackerType type)
		{
			switch(type)
			{
				case UnpackerType.Dialogue:
					UnpackDialogue(filename, output);
				break;
			}
		}

		public void UnpackDialogue(string filename, string output)
		{
			Console.WriteLine("hi");
			//read file
			byte[] bin = File.ReadAllBytes(filename);
			string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            
            Dictionary<int, int> strings = new Dictionary<int, int>();
			Dictionary<int, byte> ranges = new Dictionary<int, byte>();

			for (int i = 0; i < bin.Length-8; i+=4)
			{
                UInt16 val = BitConverter.ToUInt16(bin, i);

                if (val == 0x55FF)
				{
                    //get 32 bit value
                    int value = BitConverter.ToInt32(bin, i + 4);
                    int pointer = ((int)(value - basePointer));
					if(IsValidPointer(value, bin.Length))
                    {
                        //copy into from bin into res from pointer
                        byte[] res = bin.SubArray(8, (int)pointer);
                        //Array.Copy(bin, res, (int)value);
                        ranges.Add((int)pointer, res[0]);

                        //DecodeDialogue(res, filenameWithoutExtension + i);
                    }
					
				}
			}

	
				
		}

        bool IsValidPointer(int ptr, int length)
        {
            int newVal = ((int)(ptr - basePointer));
            bool value = (newVal >= 0 && newVal < length);
            return value;
        }

		void DecodeDialogue(byte[] bin, string filenameWithoutExtension)
		{
            string text = "";
            //start of persona game text = 0x00002500
            for (int j = 0; j < bin.Length; j++)
            {
                if (bin[j] == 0x80)
                {
                    byte otherByte = bin[j + 1];
                    ushort key = (ushort)(bin[j] << 8 | otherByte);
                    //write like 0x80A5 etc
                    //Console.WriteLine("0x" + key.ToString("X4"));

                    if (CharacterTable.DialogueTable.ContainsKey(key))
                    {
                        text += CharacterTable.DialogueTable[key];
                        j++;
                    }
                }
                if (bin[j] == 0xFF)
                {
                    //total command = FF 05 XX 00
                    switch (bin[j + 1])
                    {
                        case 0x01:
                        case 0x02:
                        case 0x03:
                        case 0x04:
                            text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])];
                            j += 1;
                            break;
                        case 0x05:
                            byte time = bin[j + 2];
                            text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])] + time + "]";
                            j += 2;
                            break;
                        case 0x06:
                        case 0x07:
                        case 0x08:
                        case 0x0E:
                        case 0x0F:
                            byte arg = bin[j + 2];
                            text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])] + arg + "]";
                            j += 1;
                            break;
                    }

                }
                //if the byte is in the table, add it to the string
                if (CharacterTable.DialogueTable.ContainsKey(bin[j]))
                {
                    text += CharacterTable.DialogueTable[bin[j]];
                }
                //if it's not in the table, add a space
                else
                {
                    //text += "?";
                }
            }
            File.WriteAllText(Environment.CurrentDirectory + "/output/" + filenameWithoutExtension + ".txt", text);
        }


        void UnpackSectors(byte[] bin, string filenameWithoutExtension)
        {
            List<int> sectors = new List<int>();

            for (int i = 0; i < bin.Length; i += 2)
            {
                ushort sector = (ushort)(bin[i] << 8 | bin[i + 1]);

                if (sector == 0x0000)
                {
                    break;
                }

                sectors.Add(sector);
            }

            for (int i = 0; i < sectors.Count - 1; i++)
            {
                int start = sectors[i];
                int end = sectors[i + 1];
                byte[] data = bin.SubArray(start * 0x800, end * 0x800);
                DecodeDialogue(data, filenameWithoutExtension + i);
            }
        }
	}
}

public enum UnpackerType
{
	Dialogue,
	Menu,
}

