using Colossal.Json;
using Colossal.PSI.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

            var path = Path.Combine(ByLawsFolder, record.idName + ".json");
            File.WriteAllText(path, JSON.Dump(record));
        }

        public static ByLawRecord LoadFromFile(string filename)
        {
            return new ByLawRecord();
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
