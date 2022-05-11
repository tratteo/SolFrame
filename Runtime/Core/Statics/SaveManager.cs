using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolFrame.Statics
{
    public static class SaveManager
    {
        public static string SaveJson<T>(T jsonClass, string path, bool pretty = true)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var json = JsonUtility.ToJson(jsonClass, pretty);
            File.WriteAllText(path, json);
            return json;
        }

        public static bool TryLoadJson<T>(string path, out T jsonClass)
        {
            if (!File.Exists(path))
            {
                jsonClass = default;
                return false;
            }
            try
            {
                jsonClass = JsonUtility.FromJson<T>(File.ReadAllText(path));
                return true;
            }
            catch (Exception)
            {
                jsonClass = default;
                return false;
            }
        }
    }
}