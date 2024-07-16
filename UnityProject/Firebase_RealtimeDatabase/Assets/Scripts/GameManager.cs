using UnityEngine;
using VictorDev.FirebaseUtils;

public class GameManager : MonoBehaviour
{
    public UserData userData;

    public RealtimeManager realtimeManager;

    private void Awake()
    {
        realtimeManager.onLoadDataComplete.AddListener(OnLoadDataComplete);
    }

    private void OnLoadDataComplete(string jsonString)
    {
        userData = JsonUtility.FromJson<UserData>(jsonString);
    }

    public void SaveData()
    {
        string jsonString = JsonUtility.ToJson(userData);
        realtimeManager.SaveData(jsonString, "users", "Victor");
    }

    public void LoadData()
    {
        realtimeManager.LoadData("users", "Victor");
    }
}
