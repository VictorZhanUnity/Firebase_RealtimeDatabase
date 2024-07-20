using TMPro;
using UnityEngine;

public class ListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtUserName, txtGender, txtAge, txtHeight, txtEmail;

    [SerializeField] private MyFirestoreObjectFormat _accountData;

    public MyFirestoreObjectFormat accountData
    {
        get => _accountData;
        set
        {
            _accountData = value;
            txtUserName.SetText(_accountData.UserName);
            txtGender.SetText(_accountData.Gender);
            txtAge.SetText(_accountData.age.ToString());
            txtHeight.SetText(_accountData.height.ToString("F1"));
            txtEmail.SetText(_accountData.eMail);
        }
    }

    private void OnValidate()
    {
        Transform hLayout = transform.GetChild(0);
        txtUserName ??= hLayout.Find("txtUserName").GetComponent<TextMeshProUGUI>();
        txtGender ??= hLayout.Find("txtGender").GetComponent<TextMeshProUGUI>();
        txtAge ??= hLayout.Find("txtAge").GetComponent<TextMeshProUGUI>();
        txtHeight ??= hLayout.Find("txtHeight").GetComponent<TextMeshProUGUI>();
        txtEmail ??= hLayout.Find("txtEmail").GetComponent<TextMeshProUGUI>();
    }
}
