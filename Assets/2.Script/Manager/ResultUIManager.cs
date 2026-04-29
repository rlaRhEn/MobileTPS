using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MatchStatManager;
public class ResultUIManager : MonoBehaviour
{
    public static ResultUIManager Instance { get; private set; }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Transform rowParent;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Text saveStatusText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void ShowResults(List<MatchPlayerStat> results)
    {
        if (resultPanel == null || rowParent == null || rowPrefab == null)
        {
            Debug.LogError("[ResultUI] 세팅이 비어 있음");
            return;
        }

        foreach (Transform child in rowParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < results.Count; i++)
        {
            GameObject rowObj = Instantiate(rowPrefab, rowParent);
            ResultRowUI rowUI = rowObj.GetComponent<ResultRowUI>();

            if (rowUI != null)
            {
                rowUI.SetRow(i + 1, results[i]);
            }
        }
        if (saveStatusText != null)
            saveStatusText.text = "저장 상태 : 대기중";

        resultPanel.SetActive(true);
    }
    public void SetSaveStatus(string message)
    {
        if (saveStatusText != null)
            saveStatusText.text = message;
    }
}