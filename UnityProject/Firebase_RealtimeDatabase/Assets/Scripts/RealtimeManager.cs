using Firebase.Database;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace VictorDev.FirebaseUtils
{
    public class RealtimeManager : MonoBehaviour
    {
        [Header(">>> ��Ū������Ʈw�ɡAInvoke��lJSON�r��")]
        public UnityEvent<string> onLoadDataComplete = new UnityEvent<string>();

        [SerializeField] private FireBaseManager fireBaseManager;

        /// <summary>
        /// �x�s���
        /// </summary>
        /// <param name="jsonString">�N�ؼ����O�নjson�榡 (JsonUtility)</param>
        /// <param name="childsName">��Ƹ`�I������</param>
        public void SaveData(string jsonString, params string[] childsName) => GetDbRefFromChildRoots(childsName).SetRawJsonValueAsync(jsonString);

        /// <summary>
        /// Ū�����
        /// </summary>
        /// <param name="childsName">��Ƶ��I������</param>
        public void LoadData(params string[] childsName)
        {
            IEnumerator LoadDataEnumerator(params string[] childsName)
            {
                var serverData = GetDbRefFromChildRoots(childsName).GetValueAsync();
                yield return new WaitUntil(predicate: () => serverData.IsCompleted);

                DataSnapshot snapshot = serverData.Result;
                string jsonData = snapshot.GetRawJsonValue();

                //�Y�L��ƫh�^��empty
                if (string.IsNullOrEmpty(jsonData)) jsonData = "empty";
                onLoadDataComplete?.Invoke(jsonData);
            }

            StartCoroutine(LoadDataEnumerator(childsName));
        }

        /// <summary>
        /// �ھ�ChildName�ƶq�A�i��`�I�����פ��^��
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
