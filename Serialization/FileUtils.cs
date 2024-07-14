using Colossal.IO.AssetDatabase.Internal;
using Colossal.Json;
using Colossal.PSI.Environment;
using Game.Prefabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trejak.ZoningByLaw.UISystems;
using UnityEngine.Profiling;

namespace Trejak.ZoningByLaw.Serialization
{
    public static class FileUtils
    {
        public static string ContentFolder { get; }        
        public static string ByLawsFolder { get; }

        static FileUtils()
        {
            ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "Trejak.ZoningByLaw");
            ByLawsFolder = Path.Combine(ContentFolder, "ByLaws");
            Directory.CreateDirectory(ContentFolder);
        }

        public static void SaveToFile(ByLawRecord record)
        {
            if (record == null)
            {
                throw new NullReferenceException("ByLawRecord \"record\" cannot be null!");
            }

            if (!Directory.Exists(ByLawsFolder))
            {
                Directory.CreateDirectory(ByLawsFolder);
            }

            var path = Path.Combine(ByLawsFolder, GetByLawFileName(record));
            File.WriteAllText(path, JSON.Dump(record));
        }

        public static IEnumerable<string> GetByLawRecordFileNames()
        {
            return Directory.GetFiles(ByLawsFolder)
                .Select(x => Path.GetFileName(x));                
        }

        public static IEnumerable<ByLawRecord> LoadAllByLaws()
        {
            var fileNames = GetByLawRecordFileNames();
            List<ByLawRecord> re = new List<ByLawRecord>();
            foreach(var file in fileNames)
            {
                re.Add(LoadFromFile(file));
            }
            return re.AsEnumerable();
        }
        
        public static string GetByLawFileName(string bylawId) => bylawId + ".json";
        public static string GetByLawFileName(PrefabID prefabId) => GetByLawFileName(prefabId.GetName());
        public static string GetByLawFileName(ByLawRecord bylaw) => GetByLawFileName(bylaw.idName);

        public static void DeleteByLaw(string filename)
        {
            var path = Path.Combine(ByLawsFolder, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static ByLawRecord LoadFromFile(string filename)
        {
            var path = Path.Combine(ByLawsFolder, filename);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find Zoning ByLaw file with name: {filename}");
            }
            ByLawRecord re = JSON.MakeInto<ByLawRecord>(JSON.Load(File.ReadAllText(path)));
            return re;
        }

        public static void SaveAll(IEnumerable<ByLawRecord> records)
        {
            foreach(var record in records)
            {
                SaveToFile(record);
            }
        }    

    }
}
