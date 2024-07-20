using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VictorDev.FirebaseUtils;

public class Test_FirestoreDB : MonoBehaviour
{
    public TMP_InputField txtUserName;
    public TMP_InputField txtGender;
    public TMP_InputField txtAge;
    public TMP_InputField txtHeight;
    public TMP_InputField txtEmail;

    public ListItem listItemPrefab;
    public ScrollRect scrollRect;

    public FirestoreDBManager firestoreDBManager;

    private string collectionName = "Accounts";

    public MyFirestoreObjectFormat myAccountInfo;

    public void UpdateData()
    {
        myAccountInfo ??= ScriptableObject.CreateInstance<MyFirestoreObjectFormat>();

        myAccountInfo.UserName = txtUserName.text;
        myAccountInfo.Gender = txtGender.text;
        myAccountInfo.age = int.Parse(txtAge.text);
        myAccountInfo.height = float.Parse(txtHeight.text);
        myAccountInfo.eMail = txtEmail.text;

        firestoreDBManager.UpdateData(myAccountInfo, collectionName, myAccountInfo.documentId, (documentId) =>
        {
            Debug.Log($">>> [UpdateData] onSuccessed: {documentId}");
            GetDocument();
        });
    }

    public void GetDocument()
    {
        firestoreDBManager.GetDocument<MyFirestoreObjectFormat>(collectionName, myAccountInfo.documentId,
            (data) =>
            {
                myAccountInfo = data;

                txtUserName.text = myAccountInfo.UserName;
                txtGender.text = myAccountInfo.Gender;
                txtAge.text = myAccountInfo.age.ToString();
                txtHeight.text = myAccountInfo.height.ToString();
                txtEmail.text = myAccountInfo.eMail.ToString();
                Debug.Log($">>> [GetData] onSuccessed!! data: {data}");
            });
    }

    public void CreateDocmuent()
    {
        myAccountInfo ??= ScriptableObject.CreateInstance<MyFirestoreObjectFormat>();

        myAccountInfo.UserName = txtUserName.text;
        myAccountInfo.Gender = txtGender.text;
        myAccountInfo.age = int.Parse(txtAge.text);
        myAccountInfo.height = float.Parse(txtHeight.text);
        myAccountInfo.eMail = txtEmail.text;
        myAccountInfo.timestamp = Timestamp.GetCurrentTimestamp();

        firestoreDBManager.CreateDocument(myAccountInfo, collectionName,
            (documentId) =>
            {
                myAccountInfo.documentId = documentId;
                Debug.Log($">>> [CreateDocmuent] onSuccessed!! DocumentId: {documentId}");
            });
    }

    public void DelectDocument()
    {
        if (myAccountInfo == null) return;
        firestoreDBManager.DeleteDocument(collectionName, myAccountInfo.documentId, (documentId) =>
        {
            Debug.Log($">>> [DelectDocument] onSuccessed!! DocumentId: {documentId}");
        });
    }

    public void GetAllDocument()
    {
        firestoreDBManager.GetAllDocuments<MyFirestoreObjectFormat>(collectionName, onSuccessed);
    }

    private void onSuccessed(Dictionary<string, MyFirestoreObjectFormat> dataSet)
    {
        //清除列表
        foreach (Transform child in scrollRect.content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (string documentId in dataSet.Keys)
        {
            MyFirestoreObjectFormat account = dataSet[documentId];

            Debug.Log($"Timestamp: {account.timestamp.ToDateTime()}");

            ListItem item = Instantiate(listItemPrefab, scrollRect.content);
            item.accountData = account;
        }
    }

    //匯出資料庫
    public async void BackUpCollection()
    {
        FirestoreBackup firestoreBackup = new FirestoreBackup();
        await firestoreBackup.ExportCollectionAsync(collectionName, (path, jsonString) =>
        {
            print(">>> Firestore Database BackUp is Complete!!");
            print($"\tPath: {path}");
            print($"\tJSON: {jsonString}");
        });
    }

    [TextArea(1, 5)]
    public string filePath;

    //匯入資料庫
    public async void ImportCollection()
    {
        FirestoreBackup firestoreBackup = new FirestoreBackup();
        await firestoreBackup.ImportCollectionAsync(filePath, (collectionName, dataSet) =>
        {
            print(">>> Firestore Database Import is Complete!!");
            print($"\tPath: {filePath}");
            print($"\t\tCollectionName: {collectionName}");

            foreach (string documentId in dataSet.Keys)
            {
                print($"\t\tdocumentId: {documentId} - Value: {dataSet[documentId]}");
            }
        });
    }
}
