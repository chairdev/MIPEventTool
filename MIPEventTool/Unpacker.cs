using System;
using System.IO;
using System.Linq;

namespace MIPEventTool
{
	public class Unpacker
	{
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

			int sectors = bin.Length / 800;

			//split the file into 800 byte chunks using linq
			
			//write the chunks, make sure to create the output directory first
			//for(int i = 0; i < chunks.Length; i++)
			//{
			//	File.WriteAllBytes(Environment.CurrentDirectory + "/output/" + filenameWithoutExtension + "_" + i + ".bin", chunks[i]);
			//}

			//read chunks and use the table to convert it to text
			//use the table to unpack

	
				string text = "";
				for(int j = 0; j < bin.Length; j++)
				{
					if(bin[j] == 0x80)
					{
						if(CharacterTable.DialogueTable.ContainsKey((ushort)(bin[j] << 8 | bin[j + 1])))
						{
							text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])];
							j++;
						}
					}
					if(bin[j] == 0xFF)
					{
						//total command = FF 05 XX 00
						switch(bin[j + 1])
						{
							case 0x01:
							case 0x02:
							case 0x03:
							case 0x04:
								text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])];
								j += 2;
							break;
							case 0x05:
								byte time = bin[j + 2];
								text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])] + time + "]";
								j += 3;
							break;
							case 0x06:
							case 0x07:
							case 0x08:
							case 0x0E:
							case 0x0F:
								byte arg = bin[j + 2];
								text += CharacterTable.DialogueTable[(ushort)(bin[j] << 8 | bin[j + 1])] + arg + "]";
								j += 2;
							break;
						}
						
					}
					//if the byte is in the table, add it to the string
					if(CharacterTable.DialogueTable.ContainsKey(bin[j]))
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
	}
}

public enum UnpackerType
{
	Dialogue,
	Menu,
}

