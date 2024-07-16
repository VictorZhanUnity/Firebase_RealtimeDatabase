using UnityEngine;
using VictorDev.FirebaseUtils;

[RequireComponent(typeof(FireBaseManager))]
public class FirestoreDBManager : MonoBehaviour
{
    [SerializeField] private FireBaseManager fireBaseManager;

    private void OnValidate() => fireBaseManager ??= GetComponent<FireBaseManager>();
}
