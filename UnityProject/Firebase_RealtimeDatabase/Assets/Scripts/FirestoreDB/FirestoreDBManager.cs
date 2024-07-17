using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FirestoreDBManager : MonoBehaviour
{
    private FirebaseFirestore db;

    private ListenerRegistration registration;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        registration = db.Collection("counters").Document("counter").Listen(snapShot =>
        {
            Debug.Log($"CallBack");
            Counter counter = snapShot.ConvertTo<Counter>();
            Debug.Log($"reg listen: {counter.Count}");
            txtCounter.SetText( counter.Count.ToString() );
        });
    }
    private void OnDestroy()
    {
        registration.Stop();
    }

    public TextMeshProUGUI txtCounter;
    public void OnClick()
    {
        int oldCount = int.Parse(txtCounter.text);
    


        Dictionary<string, object> counter = new Dictionary<string, object>()
        {
            { "Count", oldCount+1},
            { "UpdatedBy", "Victor(Dictionary)"},
        };

        DocumentReference countRef = db.Collection("counters").Document("counter");
        countRef.SetAsync(counter).ContinueWithOnMainThread(task =>
        {
            print("Update Counter");
        });
    }

    public void GetData()
    {
        db.Collection("counters").Document("counter").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            print("Called");
            Counter counter = task.Result.ConvertTo<Counter>();
            Debug.Log($"Count:" + counter.Count);
            txtCounter.SetText(counter.Count.ToString());
            print(counter.Count.ToString());
        });
    }
}
