/*
MIT License

Copyright (c) 2023 Electrostats

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace HighscoreAccuracy;

public static class FileHelper
{
    public static readonly string FILE_PATH_TOOTTALLY_APPDATA = Path.Combine(Application.persistentDataPath, "TootTally");

    public static T LoadFromTootTallyAppData<T>(string fileName)
    {
        var path = Path.Combine(FILE_PATH_TOOTTALLY_APPDATA, fileName);

        if (!File.Exists(path))
        {
            Plugin.Log.LogError($"File {path} doesnt exist.");
            return default;
        }
        try
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Couldn't deserialize object: {ex.Message} - {ex.StackTrace}");
        }
        return default;
    }

    public static void SaveToTootTallyAppData<T>(string fileName, T obj, bool saveToBackupIfExists = false)
    {
        var path = Path.Combine(FILE_PATH_TOOTTALLY_APPDATA, fileName);
        try
        {
            var json = JsonConvert.SerializeObject(obj);
            if (File.Exists(path) && saveToBackupIfExists)
            {
                if (File.Exists(path + ".old")) //For fuck sake give me NetCore3.0 please
                    File.Delete(path + ".old");
                File.Move(path, path + ".old"); //Backup
            }

            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Couldn't serialize object: {ex.Message} - {ex.StackTrace}");
        }
    }
}
