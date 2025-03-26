﻿using System;
using System.IO;
using System.Text.Json;
using PythonStructCS;

namespace BBToolsCS {
  public class BBScriptCommandData {
    public string name { get; set; } = "";
    public string format { get; set; } = "";
    public int size { get; set; } = 4;
    public bool hex { get; set; } = false;
  }

  class Program {
    public static Dictionary<uint, BBScriptCommandData> fetchCommandDB() {
      string dbPath = "CommandDB.json";
      string jsonString = File.ReadAllText(dbPath);
      
      var commands = JsonSerializer.Deserialize<Dictionary<uint, BBScriptCommandData>>(jsonString);
      if (commands is null) {
        throw new NullReferenceException("Json loader returned null");
      }

      foreach(uint key in commands.Keys) {
        if (commands[key].name == "") {
          commands[key].name = ("Instruction" + key);
        }
      }


      return commands;
    }
    static void Main(string[] args) {
      var commandDB = fetchCommandDB();
      Console.WriteLine(commandDB.Count + " Commands Loaded.");
      
      var binParser = new BinParser("./scr_hz.bin", commandDB);
      
    }
  }
}
