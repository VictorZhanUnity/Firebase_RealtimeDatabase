using Firebase.Database;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace VictorDev.FirebaseUtils
{
    public class RealtimeManager : MonoBehaviour
    {
        [Header(">>> 當讀取完資料庫時，Invoke原始JSON字串")]
        public UnityEvent<string> onLoadDataComplete = new UnityEvent<string>();

        [SerializeField] private FireBaseManager fireBaseManager;

        /// <summary>
        /// 儲存資料
        /// </summary>
        /// <param name="jsonString">將目標類別轉成json格式 (JsonUtility)</param>
        /// <param name="childsName">資料節點複雜度</param>
        public void SaveData(string jsonString, params string[] childsName) => GetDbRefFromChildRoots(childsName).SetRawJsonValueAsync(jsonString);

        /// <summary>
        /// 讀取資料
        /// </summary>
        /// <param name="childsName">資料結點複雜度</param>
        public void LoadData(params string[] childsName)
        {
            IEnumerator LoadDataEnumerator(params string[] childsName)
            {
                var serverData = GetDbRefFromChildRoots(childsName).GetValueAsync();
                yield return new WaitUntil(predicate: () => serverData.IsCompleted);

                DataSnapshot snapshot = serverData.Result;
                string jsonData = snapshot.GetRawJsonValue();

                //若無資料則回傳empty
                if (string.IsNullOrEmpty(jsonData)) jsonData = "empty";
                onLoadDataComplete?.Invoke(jsonData);
            }

            StartCoroutine(LoadDataEnumerator(childsName));
        }

        /// <summary>
        /// 根據ChildName數量，進行節點複雜度之擷取
        /// </summary>
        private DatabaseReference GetDbRefFromChildRoots(params string[] childsName)
        {
            DatabaseReference result = fireBaseManager.dbRef;

            foreach (string childName in childsName)
            {
                result = result.Child(childName);
            }
            return result;
        }

        private void OnValidate() => fireBaseManager ??= transform.parent.GetComponent<FireBaseManager>();
    }
}
