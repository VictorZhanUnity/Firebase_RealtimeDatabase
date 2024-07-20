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
        /// </summary>
       public void GetAllDocuments(string collection)
        {
            CollectionReference collectionRef = dbInstance.Collection("collection"); // 替换为你的集合名称
            collectionRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    QuerySnapshot snapshot = task.Result;
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        Debug.Log("Document ID: " + document.Id + ", Document Data: " + document.ToDictionary());
                    }
                }
                else
                {
                    Debug.LogError("Failed to get documents: " + task.Exception);
                }
            });
        }


        /// <summary>
        /// 擷取資料項 T:資料型別
        /// <para>+ 若有值，回傳其型態</para>
        /// <para>+ 若無值，回傳null</para>
        /// </summary>
        public void GetData<T>(string collectionName, string documentId, Action<T> onSuccess, Action onFailed)
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
                    }
                });
        }
        /// <summary>
        /// 新增一個文件Document
        /// <para>+ 若資料庫不存在時，會自動新增資料庫</para>
        /// </summary>
        /// <param name="data">資料欄位(Dictionary)</param>
        /// <param name="onSuccessd">成功時回傳其資料項ID值</param>
        public void CreateDocument(object data, string collection, Action<string> onSuccessed = null, Action onFailed = null)
        {
            dbInstance.Collection(collection).AddAsync(data).ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsCompleted)
                    {
                        onSuccessed?.Invoke(task.Result.Id);
                        Debug.Log("\t[CreateDocument] Added document with ID: " + task.Result.Id);
                    }
                    else
                    {
                        onFailed?.Invoke();
                        Debug.LogError("\t[CreateDocument] Failed to add document: " + task.Exception);
                    }
                });
        }
        /// <summary>
        /// 更新資料 / 新增資料
        /// <para>+ 若DocumentID值不存在，則變成以DocumentID進行新增資料</para>
        /// </summary>
        /// <param name="data">資料項內容</param>
        /// <param name="collectionName">集合名稱(資料表)</param>
        /// <param name="documentId">文件名稱(資料ID)</param>
        public void UpdateData(object data, string collectionName, string documentId, Action<string> onSuccessed = null, Action onFailed = null)
         => GetDocRef(collectionName, documentId).SetAsync(data).ContinueWithOnMainThread(task =>
         {
             if (task.IsCompleted) onSuccessed?.Invoke(documentId);
             else onFailed?.Invoke();
         });

        /// <summary>
        /// 刪除資料Document
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
                    Debug.LogError("\t[DeleteDocument] Failed to delete document: " + task.Exception);
                }
            });
        }

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

        /// <summary>
        /// 取得Document Ref
        /// </summary>
        private DocumentReference GetDocRef(string collectionName, string documentId) => dbInstance.Collection(collectionName).Document(documentId);

        private void OnDestroy()
        {
            foreach (string key in listenerDict.Keys)
            {
                listenerDict[key].Stop();
            }
            listenerDict.Clear();
        }

        public void AddProduct()
        {
            CollectionReference productsRef = dbInstance.Collection("products");
            Dictionary<string, object> productData = new Dictionary<string, object>
        {
            { "Name", "Sample Product" },
            { "Price", 19.99 },
            { "Stock", 100 }
        };

            productsRef.AddAsync(productData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Product successfully added!");
                }
                else
                {
                    Debug.LogError("Failed to add product: " + task.Exception);
                }
            });
        }
    }
}
