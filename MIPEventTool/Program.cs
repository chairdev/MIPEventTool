// See https://aka.ms/new-console-template for more information

//check if the user typed unpack [filename] [output directory]
using MIPEventTool;

if (args[0] == "unpack")
    {
        //check if the file exists
        if (File.Exists(args[1]))
        {
                Unpacker unpacker = new Unpacker(args[1], args[2], UnpackerType.Dialogue);
                Console.WriteLine("unpacked");
        }
        else
        {
            //file does not exist
            Console.WriteLine("File does not exist");
        }
    }
    else
    {
        //user did not type unpack
        Console.WriteLine("Please type unpack [filename] [output directory]");
    }