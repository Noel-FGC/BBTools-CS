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
      writer = new StreamWriter(Path.ChangeExtension(path, ".bbscr"));
      reader = new BinaryReader(stream);
      Endian = endian;
      CommandDB = tmpCommandDB;
      parseBBScriptRoutine();
    }
    
    void parseBBScriptRoutine() {
      uint FunctionCount = (uint) PyStruct.unpack(Endian + "I", reader.ReadBytes(4))[0];
      stream.Seek(FunctionCount * 0x24, SeekOrigin.Current);
      Console.WriteLine(FunctionCount);
      int indentationLevel = 0;
      uint lastCommandID = 0;
      bool isElseIf = false;
      while (stream.Position < stream.Length) {
        lastCommandID = commandID;
        uint commandID = (uint) PyStruct.unpack(Endian + "I", reader.ReadBytes(4))[0];
        
        if (commandID != 56 || !((uint[])[5, 55]).Contains(lastCommandID)) { // else, endIf, endIfNot
          writer.Write('\n');
        }
        if (((uint[])[1, 5, 9, 16, 35, 55, 57, 14002]).Contains(commandID)) { // endState, endIf, endSubroutine, endUpon, ApplyFunctionsToSelf, endIfNot, endElse, Move_EndRegister();
          if (commandID == 57 && isElseIf) continue;
          indentationLevel--;
          for(int i = 0;i < indentationLevel;i++) {
            writer.Write("  ");
          }
          writer.Write("}");
          continue;
        }
        if (commandID != 56){ 
          for(int i = 0;i < indentationLevel;i++) {
            writer.Write("  ");
          }
        }

        string format = CommandDB[commandID].format;
        List<object> commandData = PyStruct.unpack(Endian + format, reader.ReadBytes(PyStruct.calcSize(format)));
        if (((uint[])[4, 15, 36, 54, 56, 14001]).Contains(commandID)) {
          indentationLevel++;
        }
        switch (commandID) {
          case(0): {
                    indentationLevel = 1;
                    writer.Write("State " + commandData[0] + " {");
                  } break;
          case(4): {
                    bool SLOT = false;
                    string temp = "";
                    if ((int)commandData[0] > 0) {
                      SLOT = true;
                    }
                    if (SLOT) {
                      temp = (string)("SLOT_" + commandData[1]);
                    } else {
                      temp = (string)commandData[1];
                    }
                    writer.Write("if (" + temp + ") {");
                  } break;
          case(8): {
                     indentationLevel = 1;
                    writer.Write("Subroutine " + commandData[0] + " {");
                  } break;
          case(15): {
                     writer.Write("upon_" + commandData[0] + " {");
                   } break;
          case(36): {
                     writer.Write("ModifyObject(" + commandData[0] + ") {");
                   } break;
          case(54): {
                     bool SLOT = false;
                     string temp = "";
                     if ((int)commandData[0] > 0) {
                       SLOT = true;
                     }
                     if (SLOT) {
                       temp = (string)("SLOT_" + commandData[1]);
                     } else {
                       temp = (string)commandData[1];
                     }
                     writer.Write("if (!" + temp + ") {");
                   } break;
          case (56): {
                     writer.Write(" else {");
                   } break;
          case(14001): {
                        writer.Write("Register_Move(\"" + commandData[0] + "\", " + commandData[1] + ") {");
                      } break;
          default: {
                      writer.Write(CommandDB[commandID].name);
                      writer.Write('(');
                      foreach (object val in commandData) {
                        /*writer.Write("(" + val.GetType() + ") ");*/
                        if (val.GetType() == typeof(string)) {
                          writer.Write('"' + (string)val + '"');
                        } else {
                          writer.Write(val);
                        }
                        /*writer.Write(val);*/
                        if (val != commandData.Last()) {
                          writer.Write(", ");
                        }
                      }
                      writer.Write(");");
                    } break;
          }
      }
      writer.Flush();
    }
  }
}
