using Firebase.Database;
using UnityEngine;

namespace VictorDev.FirebaseUtils
{
    public class FireBaseManager : MonoBehaviour
    {
        public DatabaseReference dbRef { get; private set; }
        private void Awake() => dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }
}
