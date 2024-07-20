using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using VictorDev.FirebaseUtils;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Test_FirestoreDB : MonoBehaviour
{
    public TextMeshProUGUI txtCounter;

    public FirestoreDBManager firestoreDBManager;

    public void UpdateData()
    {
        int oldCount = int.Parse(txtCounter.text);

        Dictionary<string, object> counter = new Dictionary<string, object>()
        {
            { "Count", oldCount+1},
            { "UpdatedBy", "Victor(Dictionary)"},
        };

        firestoreDBManager.UpdateData(counter, "counters","counter", ()=>Debug.Log("Updated"));
    }

    public void SelecteData()
    {
       firestoreDBManager.GetData<Counter>("counters", "counter",
           (data) => txtCounter.SetText(data.Count.ToString()));
    }

    [ContextMenu(" - CreateDocmuent")]
    public void CreateDocmuent()
    {
        Dictionary<string, object> user = new Dictionary<string, object>
            {
               { "Count", 1561},
                { "UpdatedBy", "Mina"},
            };

        firestoreDBManager.CreateDocument(user, "counters",
            (data) => txtCounter.SetText(data));
    }

    public void DelectDocument()
    {
        firestoreDBManager.DeleteDocument("counters", "count");
    }
}
