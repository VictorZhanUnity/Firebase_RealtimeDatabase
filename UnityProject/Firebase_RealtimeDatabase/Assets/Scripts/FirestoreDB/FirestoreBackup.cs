using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace VictorDev.FirebaseUtils
{
    /// <summary>
    /// Firestore 集合資料備份管理
    /// <para> + 匯入/匯出json檔案</para>
    /// </summary>
    public class FirestoreBackup
    {
        private FirebaseFirestore db;
        public FirestoreBackup() => db = FirebaseFirestore.DefaultInstance;

        /// <summary>
        /// 匯出儲存成.json檔案
        /// <para>+ 檔名格式：集合名 + 時間點</para>
        /// <para>+ 儲存路徑：應用程式路徑 + FirebaseDB BackUp資料夾</para>
        /// <para>+ 成功時：Invoke{檔案路徑, JSON字串} </para>
        /// </summary>
        /// <param name="collectionName">要儲存的集合名</param>
        public async Task ExportCollectionAsync(string collectionName, Action<string, string> onSuccess = null)
        {
            // 檢查備份資料夾是否存在
            string filePath = Path.Combine(Application.persistentDataPath, "FirebaseDB BackUp");
            if (Directory.Exists(filePath) == false) Directory.CreateDirectory(filePath);

            string fileName = $"{collectionName}-{DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss")}.json";
            filePath = Path.Combine(filePath, fileName);

            CollectionReference collectionRef = db.Collection(collectionName);
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

            using (StreamWriter file = new StreamWriter(filePath))
            {
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        var data = document.ToDictionary();
                        data["documentId"] = document.Id; // 添加文档 ID
                                                          // 处理时间戳字段
                        foreach (var key in data.Keys.ToList())
                        {
                            if (data[key] is Timestamp timestamp)
                            {
                                data[key] = timestamp.ToDateTime(); // 转换时间戳为 DateTime
                            }
                        }
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                        await file.WriteLineAsync(json);
                        onSuccess.Invoke(filePath, json);
                    }
                }
            }
        }


        /// <summary>
        /// 導入集合Collection.json檔案
        /// <para>+ 路徑例：{C:/Users/victo/AppData/LocalLow/VictorDev/Firebase_RealtimeDatabase\FirebaseDB BackUp\Accounts-2024_07_20 19_16_32.json}</para>
        /// <para>+ 成功時Invoke {集合名, 字典(需自行轉型態)}</para>
        /// <param name="filePath">絕對路徑</param>
        /// </summary>
        public async Task ImportCollectionAsync(string filePath, Action<string, Dictionary<string, object>> onSuccessed)
        {
            string[] str = filePath.Split("\\");
            string collectionName = str[str.Length - 1].Split("-")[0];
            await ImportCollectionAsync(filePath, collectionName, onSuccessed);
        }

        /// <summary>
        /// 導入集合Collection.json檔案
        /// <para>+ 當路徑與檔案格式不符合設定格式時使用</para>
        /// <para>+ 成功時Invoke {集合名, 字典(需自行轉型態)}</para>
        /// <param name="filePath">絕對路徑</param>
        /// </summary>
        public async Task ImportCollectionAsync(string filePath, string collectionName, Action<string, Dictionary<string, object>> onSuccessed = null)
        {
            CollectionReference collectionRef = db.Collection(collectionName);

            using (StreamReader file = new StreamReader(filePath))
            {
                string line;
                while ((line = await file.ReadLineAsync()) != null)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(line);
                    if (data != null)
                    {
                        // 恢复时间戳字段
                        foreach (var key in data.Keys)
                        {
                            if (data[key] is Newtonsoft.Json.Linq.JObject jObject && jObject["seconds"] != null)
                            {
                                data[key] = Timestamp.FromDateTime(DateTimeOffset.FromUnixTimeSeconds((long)jObject["seconds"]).DateTime);
                            }
                        }
                        string documentId = data.ContainsKey("documentId") ? (string)data["documentId"] : null;
                        DocumentReference docRef = documentId != null ? collectionRef.Document(documentId) : collectionRef.Document(); // 如果有文档 ID，则使用它
                        await docRef.SetAsync(data);

                        onSuccessed?.Invoke(collectionName, data);
                    }
                }
            }
        }
    }
}
