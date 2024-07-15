using Firebase.Database;
using System;
using System.Collections;
using UnityEngine;


[Serializable]
public class DataToSave
{
    public string userName;
    public int totalCoins;
    public int crrLevel;
    public int highScore;
}

public class DataSaver : MonoBehaviour
{
    public DataToSave dataToSave;
    public string userId;

    private DatabaseReference dbRef;

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(dataToSave);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }

    public void LoadData() => StartCoroutine(LoadDataEnumerator());

    IEnumerator LoadDataEnumerator()
    {
        var serverData = dbRef.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        print("process is complete");

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();
        if (jsonData != null)
        {
            print("server data found");

            dataToSave = JsonUtility.FromJson<DataToSave>(jsonData);

        }else
        {
            print("no data found");
        }
    }
}
