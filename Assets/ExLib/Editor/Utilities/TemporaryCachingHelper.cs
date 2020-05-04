using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace ExLib.Editor.Utils
{
    public static class TemporaryCachingHelper
    {
        public const string tempCache = "mycache";
        public const string tempCacheExtension = ".tmp";

        public static void WriteCache(string name, object data)
        {
            WriteCache(Application.temporaryCachePath, tempCache, name, data);
        }

        public static void WriteCache(string folder, string cachename, string name, object data)
        {
            Hashtable dataTable = ReadCacheTable(folder, cachename);

            if (dataTable == null)
                dataTable = new Hashtable();

            if (dataTable.ContainsKey(name))
                dataTable[name] = data;
            else
                dataTable.Add(name, data);

            WriteCacheTable(folder, cachename, dataTable);
        }

        private static void WriteCacheTable(string folder, string cachename, Hashtable dataTable)
        {
            BinaryFormatter binary = new BinaryFormatter();
            string path = Path.Combine(folder, cachename + tempCacheExtension);
            FileStream fs = new FileStream(path, FileMode.Create);
            try
            {
                binary.Serialize(fs, dataTable);
            }
            catch (System.Runtime.Serialization.SerializationException ex)
            {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                fs.Close();
                System.GC.SuppressFinalize(binary);
                binary = null;
            }
        }

        public static void RemoveCacheData(string name)
        {
            RemoveCacheData(Application.temporaryCachePath, tempCache, name);
        }

        public static void RemoveCacheData(string folder, string cachename, string name)
        {
            Hashtable dataTable = ReadCacheTable(folder, cachename);

            if (dataTable == null)
            {
                Debug.LogWarning("not found cached data. maybe not be wrote the data which is named the value of a parameter \"name\"");
                return;
            }

            if (!dataTable.ContainsKey(name))
            {
                Debug.LogWarning("the cache does not have the data which is named the value of a parameter \"name\"");
                return;
            }
            else
            {
                dataTable.Remove(name);
            }

            WriteCacheTable(folder, cachename, dataTable);
        }

        public static T ReadCache<T>(string name)
        {
            return ReadCache<T>(Application.temporaryCachePath, tempCache, name);
        }

        public static T ReadCache<T>(string folder, string cachename, string name)
        {
            string path = Path.Combine(folder, cachename + tempCacheExtension);
            if (!File.Exists(path))
            {
                Debug.LogWarning("not found cached data. maybe not wrote any cache");
                return default(T);
            }

            Hashtable dataTable = ReadCacheTable(folder, cachename);

            if (dataTable == null)
            {
                Debug.LogWarning("not found cached data. maybe not wrote any cache");
                return default(T);
            }

            if (!dataTable.ContainsKey(name))
            {
                Debug.LogWarning("the cache does not have the data which is named the value of a parameter \"name\"");
                return default(T);
            }
            else
            {
                return (T)dataTable[name];
            }
        }

        public static Hashtable ReadCacheTable(string folder, string cachename)
        {
            string path = Path.Combine(folder, cachename + tempCacheExtension);
            if (!File.Exists(path))
            {
                Debug.LogWarning("not found cached data. maybe not be wrote any cache");
                return null;
            }

            Hashtable dataTable = null;

            FileStream fs = new FileStream(path, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                dataTable = (Hashtable)formatter.Deserialize(fs);
            }
            catch (System.Runtime.Serialization.SerializationException ex)
            {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                fs.Close();
            }

            return dataTable;
        }
    }
}