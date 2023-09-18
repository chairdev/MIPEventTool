using System;
using System.IO;
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
			//read file
			byte[] bin = File.ReadAllBytes(filename);
			string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

			//use the table to unpack
			string text = "";
			for(int i = 0; i < bin.Length; i++)
			{
				//if the byte is in the table, add it to the string
				if(CharacterTable.DialogueTable.ContainsKey(bin[i]))
				{
					text += CharacterTable.DialogueTable[bin[i]];
				}
				//if it's not in the table, add a space
				else
				{
					text += "?";
				}
			}

			//write the string to a file
			File.WriteAllText(output + filenameWithoutExtension + ".txt", text);

		}
	}
}

public enum UnpackerType
{
	Dialogue,
	Menu,
}

