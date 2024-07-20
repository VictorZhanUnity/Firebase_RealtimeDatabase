using Firebase.Firestore;
using System;
using UnityEngine;
/// <summary>
/// Firestore資料物件格式
/// <para>+ 變數只能使用{get;set;}形式</para>
/// <para>+ 可以用Debug模式觀察每個變數值</para>
/// </summary>
[FirestoreData]
public class MyFirestoreObjectFormat : ScriptableObject
{
    [FirestoreDocumentId] //在Convert時會自動儲存documentId
    public string documentId { get; set; }

    // 以下為各項欄位
    [FirestoreProperty]
    public string UserName { get; set; }
    [FirestoreProperty]
    public string Gender { get; set; }
    [FirestoreProperty]
    public int age { get; set; }
    [FirestoreProperty]
    public float height { get; set; }
    [FirestoreProperty]
    public string eMail { get; set; }

    /// <summary>
    /// 可以用 timestamp.ToDateTime()的方式轉成DateTime型態
    /// </summary>
    [FirestoreProperty]
    public  Timestamp timestamp { get; set; }
}