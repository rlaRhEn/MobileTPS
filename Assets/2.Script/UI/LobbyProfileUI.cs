using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Firebase.Firestore;
using Firebase.Extensions;


public class LobbyProfileUI : MonoBehaviour
{
    [SerializeField] private GameObject myInfoPanel;
    [SerializeField] private Text nicknameText;
    [SerializeField] private Text killText;
    [SerializeField] private Text deathText;
    [SerializeField] private Text profileStatusText;

    private FirebaseFirestore db;
    [SerializeField] private bool useFirestoreProfile = true;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        if (myInfoPanel != null)
            myInfoPanel.SetActive(false);
    }

    public void OpenMyInfo()
    {
        if (myInfoPanel != null)
            myInfoPanel.SetActive(true);

        RefreshMyInfo();
        LoadMyStatFromFirestore();
    }
    void LoadMyStatFromFirestore()
    {
        if (db == null)
        {
            Debug.LogError("[Firestore Profile] db가 null 입니다.");
       
            return;
        }

        if (string.IsNullOrEmpty(UserData.Uid))
        {
            Debug.LogError("[Firestore Profile] UserData.Uid가 비어 있음");
           
            return;
        }
    
        DocumentReference docRef = db.Collection("users").Document(UserData.Uid);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogError("[Firestore Profile] 조회 실패");
                if (task.Exception != null)
                    Debug.LogError(task.Exception.ToString());
                return;
            }

            DocumentSnapshot snapshot = task.Result;

            if (!snapshot.Exists)
            {
                Debug.LogWarning("[Firestore Profile] 문서가 없음");
                return;
            }

            string uid = snapshot.ContainsField("uid") ? snapshot.GetValue<string>("uid") : UserData.Uid;
            string nickname = snapshot.ContainsField("nickname") ? snapshot.GetValue<string>("nickname") : UserData.Nickname;
            int totalKill = snapshot.ContainsField("totalKill") ? snapshot.GetValue<int>("totalKill") : 0;
            int totalDeath = snapshot.ContainsField("totalDeath") ? snapshot.GetValue<int>("totalDeath") : 0;

            ProfileStatResponse response = new ProfileStatResponse
            {
                uid = uid,
                nickname = nickname,
                totalKill = totalKill,
                totalDeath = totalDeath
            };

            ApplyProfileStat(response);

            Debug.Log($"[Firestore Profile] uid={uid}, nick={nickname}, kill={totalKill}, death={totalDeath}");
        });
    }



    public void RefreshMyInfo()
    {
        if (nicknameText != null)
            nicknameText.text = $"닉네임 : {UserData.Nickname}";

        if (killText != null)
            killText.text = "Kill : -";

        if (deathText != null)
            deathText.text = "Death : -";
    }
    public void ApplyProfileStat(ProfileStatResponse response)
    {
        if (response == null)
        {
            if (killText != null) killText.text = "Kill : -";
            if (deathText != null) deathText.text = "Death : -";
            return;
        }

        if (nicknameText != null)
            nicknameText.text = $"닉네임 : {response.nickname}";


        if (killText != null)
            killText.text = $"Kill : {response.totalKill}";

        if (deathText != null)
            deathText.text = $"Death : {response.totalDeath}";
    }
    public void CloseMyInfo()
    {
        if (myInfoPanel != null)
            myInfoPanel.SetActive(false);
    }

}
