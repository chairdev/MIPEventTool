using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIPEventTool
{
    public class Unpacker
    {
        public uint basePointer = 0x80100000;

        //unpack file using the table
        public Unpacker(string filename, string output, UnpackerType type)
        {
            switch (type)
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


            List<string> strings = new List<string>();
            Dictionary<int, byte> ranges = new Dictionary<int, byte>();

            for (int i = 0; i < bin.Length - 8; i += 4)
            {
                UInt16 val = BitConverter.ToUInt16(bin, i);

                if (val == 0x55FF)
                {
                    //get 32 bit value
                    int value = BitConverter.ToInt32(bin, i + 4);
                    int pointer = ((int)(value - basePointer));
                    if (IsValidPointer(value, bin.Length))
                    {
                        //copy into from bin into res from pointer
                        byte[] res = bin.SubArray(8, (int)pointer);
                        //Array.Copy(bin, res, (int)value);
                        if (ranges.ContainsKey((int)pointer))
                        {
                            ranges[(int)pointer] = res[0];
                        }
                        else
                        {
                            ranges.Add((int)pointer, res[0]);
                        }


                        //DecodeDialogue(res, filenameWithoutExtension + i);
                    }

                }
            }

            foreach (var entry in ranges)
            {
                int pointer = entry.Key;
                byte[] data = new byte[1]; // Modify the size as needed

                // Extract the data from 'bin' based on the 'pointer' and 'data' size
                Array.Copy(bin, 8 + pointer, data, 0, data.Length);

                // Decode the extracted data using your DecodeDialogue function
                strings.Add(DecodeDialogue(bin, 8 + pointer));
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(strings, options);
            File.WriteAllText(Environment.CurrentDirectory + "/output/" + $"{filenameWithoutExtension}_text.json", jsonString);
        }

        bool IsValidPointer(int ptr, int length)
        {
            int newVal = ((int)(ptr - basePointer));
            bool value = (newVal >= 0 && newVal < length);
            return value;
        }

        string DecodeDialogue(byte[] bin, int startIndex = 0)
        {
            string text = "";
            bool finished = false;
            //start of persona game text = 0x00002500
            for (int j = startIndex; j < bin.Length; j++)
            {
                if (!finished)
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
                    else if (bin[j] == 0xFF)
                    {
                        //total command = FF 05 XX 00
                        switch (bin[j + 1])
                        {
                            case 0x01:
                                finished = true;
                                text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])];
                                j += 1;
                                break;
                                break;
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
                    else if (CharacterTable.DialogueTable.ContainsKey(bin[j]))
                    {
                        text += CharacterTable.DialogueTable[bin[j]];
                    }
                    //if it's not in the table, write [XX]
                    else
                    {
                        text += "[" + bin[j].ToString("X2") + "]";
                    }
                }
            }
            return text;
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
                //DecodeDialogue(data, filenameWithoutExtension + i);
            }
        }
    }
}

public enum UnpackerType
{
	Dialogue,
	Menu,
}

