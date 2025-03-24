using System;
using System.IO;
using System.Text;
using PythonStructCS;

namespace BBToolsCS {
  public class BinParser {
    private Stream stream;
    private StreamWriter writer;
    private BinaryReader reader;
    private char Endian;
    private Dictionary<uint, BBScriptCommandData> CommandDB;
    public BinParser(string path, Dictionary<uint, BBScriptCommandData> tmpCommandDB, char endian = '<') {
      stream = File.Open(path, FileMode.Open);
      writer = new StreamWriter(Path.ChangeExtension(path, ".cs"));
      reader = new BinaryReader(stream);
      Endian = endian;
      CommandDB = tmpCommandDB;
      parseBBScriptRoutine();
    }
    
    void parseBBScriptRoutine() {
      uint FunctionCount = (uint) PyStruct.unpack(Endian + "I", reader.ReadBytes(4))[0];
      stream.Seek(FunctionCount * 0x24, SeekOrigin.Current);
      Console.WriteLine(FunctionCount);
      while (stream.Position < stream.Length) {
        uint commandID = (uint) PyStruct.unpack(Endian + "I", reader.ReadBytes(4))[0];
        string format = CommandDB[commandID].format;
        writer.Write(CommandDB[commandID].name);
        List<object> commandData = PyStruct.unpack(Endian + format, reader.ReadBytes(PyStruct.calcSize(format)));
        writer.Write('(');
        foreach (object val in commandData) {
          writer.Write(" " + val.GetType() + ": " + val);
          /*writer.Write(val);*/
          if (val != commandData.Last()) {
            writer.Write(", ");
          }
        }
        writer.Write(");\n");
      }
    }
  }
}

