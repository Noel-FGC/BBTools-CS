using System;
using System.IO;
using System.Text;
using PythonStructCS;

namespace BBToolsCS {
  public class BinParser {
    private Stream stream;
    private BinaryReader reader;
    private char Endian;
    private Dictionary<uint, BBScriptCommandData> CommandDB;
    public BinParser(string path, Dictionary<uint, BBScriptCommandData> tmpCommandDB) {
      stream = File.Open(path, FileMode.Open);
      reader = new BinaryReader(stream);
      Endian = '<';
      CommandDB = tmpCommandDB;
      parseBBScriptRoutine();
    }
    
    void parseBBScriptRoutine() {
      var FunctionCount = reader.ReadUInt32();
      stream.Seek(FunctionCount * 0x24, SeekOrigin.Current);
      Console.WriteLine(FunctionCount);
      while (stream.Position < stream.Length) {
        uint commandID = reader.ReadUInt32();
        string format = CommandDB[commandID].format;
        Console.Write(CommandDB[commandID].name);
        byte[] tmp = reader.ReadBytes(PyStruct.calcSize(format));
        List<object> commandData = PyStruct.unpack(format, tmp);
        Console.Write('(');
        foreach (object val in commandData) {
          Console.Write(val);
          if (val != commandData.Last()) {
            Console.Write(", ");
          }
        }
        Console.Write(")\n");
      }
    }
  }
}

