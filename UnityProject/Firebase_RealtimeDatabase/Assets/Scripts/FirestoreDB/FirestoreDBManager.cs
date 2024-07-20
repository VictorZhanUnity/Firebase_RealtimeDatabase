using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VictorDev.FirebaseUtils
{
    public class FirestoreDBManager : MonoBehaviour
    {
        private FirebaseFirestore dbInstance { get; set; }
        private Dictionary<string, ListenerRegistration> listenerDict { get; set; } = new Dictionary<string, ListenerRegistration>();

        private void Awake() => dbInstance = FirebaseFirestore.DefaultInstance;



        /// <summary>
        /// 擷取Collection底下所有的Document與其欄位值
        /// <para>+ onSuccessed：回傳 字典{documentId, 字典{欄位, 值}}</para>
        /// <para>+ orderByName：若不為空，則依照欄位名稱進行排序</para>
        /// <para>+ isDescending：是否為降冪排序(從最近排到最先前)</para>
        /// </summary>
        public void GetAllDocuments<T>(string collection, Action<Dictionary<string, T>> onSuccessed, Action onFailed = null, bool isDescending = true, string orderByName = "timestamp")
        {
            CollectionReference collectionRef = dbInstance.Collection(collection); // 替换为你的集合名称

            Query query = collectionRef;

            //是否需要排序之判斷
            if (string.IsNullOrEmpty(orderByName) == false)
            {
                query = isDescending ? query.OrderByDescending(orderByName) : query.OrderBy(orderByName);
            }

            query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Dictionary<string, T> result = new Dictionary<string, T>();

                    QuerySnapshot snapshot = task.Result;
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        if (document.Exists)
                        {
                            Debug.Log("Document ID: " + document.Id + ", Document Data: " + document.ToDictionary());
                            result[document.Id] = document.ConvertTo<T>();
                        }
                    }
                    onSuccessed?.Invoke(result);
                }
                else
                {
                    Debug.LogError("Failed to get documents: " + task.Exception);
                    onFailed?.Invoke();
                    if (onFailed == null) LogOnFailed("GetAllDocuments", task.Exception);
                }
            });
        }

        /// <summary>
        /// 擷取資料項 T:資料型別
        /// <para>+ 若有值，回傳其型態</para>
        /// <para>+ 若無值，回傳null</para>
        /// <para>+ onSuccessed: 回傳T型態之物件</para>
        /// </summary>
        public void GetDocument<T>(string collectionName, string documentId, Action<T> onSuccess, Action onFailed = null)
        {
            GetDocRef(collectionName, documentId).GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    T result = default(T);
                    if (task.IsCompleted)
                    {
                        if (task.Result.Exists)
                            result = task.Result.ConvertTo<T>();
                        onSuccess.Invoke(result);
                    }
                    else
                    {
                        Debug.LogError("\t [SelectData] Failed to get document: " + task.Exception);
                        onFailed?.Invoke();
                        if (onFailed == null) LogOnFailed("CreateDocument", task.Exception);
                    }
                });
        }

        /// <summary>
        /// 新增一個文件Document
        /// <para>+ 若資料庫不存在時，會自動新增資料庫</para>
        /// <para>+ object data: 可帶入任意類別或字典{string, 任意類型值}</para>
        /// <para>+ onSuccessed: 回傳documentId (動態產生)</para>
        /// </summary>
        /// <param name="data">資料集 字典{string, object值}</param>
        /// <param name="onSuccessd">成功時回傳其資料項ID值</param>
        public void CreateDocument(object data, string collection, Action<string> onSuccessed = null, Action onFailed = null)
        {
            dbInstance.Collection(collection).AddAsync(data).ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsCompleted) onSuccessed?.Invoke(task.Result.Id);
                    else
                    {
                        onFailed?.Invoke();
                        if (onFailed == null) LogOnFailed("CreateDocument", task.Exception);
                    }
                });
        }

        /// <summary>
        /// 更新資料 / 新增資料
        /// <para>+ 若DocumentID值不存在，則變成以DocumentID進行新增資料</para>
        /// <para>+ onSuccessed: 回傳documentId</para>
        /// </summary>
        /// <param name="data">資料項內容</param>
        /// <param name="collectionName">集合名稱(資料表)</param>
        /// <param name="documentId">文件名稱(資料ID)</param>
        public void UpdateData(object data, string collectionName, string documentId, Action<string> onSuccessed = null, Action onFailed = null)
         => GetDocRef(collectionName, documentId).SetAsync(data).ContinueWithOnMainThread(task =>
         {
             if (task.IsCompleted) onSuccessed?.Invoke(documentId);
             else
             {
                 onFailed?.Invoke();
                 if (onFailed == null) LogOnFailed("UpdateData", task.Exception);
             }
         });

        /// <summary>
        /// 刪除資料Document
        /// <para>+ onSuccessed: 回傳documentId</para>
        /// </summary>
        public void DeleteDocument(string collectionName, string documentId, Action<string> onSuccessed = null, Action onFailed = null)
        {
            // 删除 Document
            GetDocRef(collectionName, documentId).DeleteAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    onSuccessed?.Invoke(documentId);
                    Debug.Log("\t[DeleteDocument] Document with ID: " + documentId + " has been deleted.");
                }
                else
                {
                    onFailed?.Invoke();
                    if (onFailed == null) LogOnFailed("DeleteDocument", task.Exception);
                }
            });
        }



        /// <summary>
        /// 取得Document Ref
        /// </summary>
        private DocumentReference GetDocRef(string collectionName, string documentId) => dbInstance.Collection(collectionName).Document(documentId);

        #region [>>>同步即時監聽資料]
        /// <summary>
        /// 監聽即時同步更新資料
        /// </summary>
        public ListenerRegistration RegistraionAsync<T>(string collectionName, string documentId, Action<T> onAsyncUpdate)
        {
            ListenerRegistration registration = GetDocRef(collectionName, documentId)
                .Listen(snapShot => onAsyncUpdate?.Invoke(snapShot.ConvertTo<T>()));
            listenerDict[string.Concat(collectionName, documentId)] = registration;
            return registration;
        }

        /// <summary>
        /// 取消監聽即時同步更新資料
        /// </summary>
        public void CancellRegistraionAsync(string collectionName, string documentId)
        {
            string key = string.Concat(collectionName, documentId);
            if (listenerDict.ContainsKey(key))
            {
                listenerDict[key].Stop();
                listenerDict.Remove(key);
            }
        }
        #endregion

        /// <summary>
        /// 停止所有同步監聽
        /// </summary>
        private void OnDestroy()
        {
            foreach (string key in listenerDict.Keys)
            {
                listenerDict[key].Stop();
            }
            listenerDict.Clear();
        }

        private void LogOnFailed(string funcName, AggregateException error) => Debug.LogWarning($"\t[{funcName}] onFailed: {error}");
    }
}