using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class StageManager : IManager
{
    private static readonly HashSet<string> clearedStages = new();
    public List<string> _allStages;

    public void Initialize()
    {
        _allStages = new List<string>
        {
            Scenes.TUTORIAL,
            Scenes.STEP1,
            Scenes.FINAL,
        };
    }

    public void Release()
    {
        clearedStages.Clear();
    }

    /// <summary>
    /// �������� Ŭ���� �� ȣ��
    /// </summary>
    public async void ClearedStage(string stageName)
    {
        Debug.Log($"{stageName} Ŭ����");
        clearedStages.Add(stageName);

        int currentStageIdx = _allStages.IndexOf(stageName);

        // ������ ���������� ���
        if (stageName == Scenes.FINAL)
        {
            await FinalClear();
            return;
        }

        //���� ���������� �ִ� ���
        if (currentStageIdx != -1 && currentStageIdx + 1 < _allStages.Count)
        {
            string nextStageName = _allStages[currentStageIdx + 1];

            var player = GameObject.Find("FinalPlayer");
            var hs = player?.GetComponent<HealthSystem>();

            // ���� ���� üũ
            if (hs != null && hs.currentHealth > 50 && stageName == Scenes.STEP1 &&
                !GameManager.Accomplishment.IsUnlocked((int)AchievementKey.POTENTIAL))
            {
                // ���� �޼� �˾� ������ ��ٸ� �� ���� �� �ε�
                await GameManager.Accomplishment.UnLock((int)AchievementKey.POTENTIAL);
            }

            // �� ��ȯ�� ������ �� ����
            if(UnitySceneManager.GetActiveScene().name != nextStageName)
            {
                GameManager.Scene.LoadScene(nextStageName);
            }
        }
    }

    public bool IsStageCleared(string stageName) => clearedStages.Contains(stageName);

    /// <summary>
    /// ���� �������� Ŭ���� ó��
    /// </summary>
    private async Task FinalClear()
    {
        // ALIVE ����
        if (!GameManager.Accomplishment.IsUnlocked((int)AchievementKey.ALIVE))
        {
            await GameManager.Accomplishment.UnLock((int)AchievementKey.ALIVE);
        }

        // STRONGER ����
        var player = GameObject.Find("FinalPlayer");
        var hs = player?.GetComponent<HealthSystem>();

        if (hs != null && hs.currentHealth > 50 &&
            !GameManager.Accomplishment.IsUnlocked((int)AchievementKey.STRONGER))
        {
            await GameManager.Accomplishment.UnLock((int)AchievementKey.STRONGER);
        }

        // ���� ���� ����
        var ending = GameObject.FindObjectOfType<EndingEffect>();
        if (ending != null)
        {
            ending.PlayEnding();

            // ���� ���� ������ ���
            await UniTask.Delay(5000);
        }

        //�������� �� �� ���� �� ��ȯ
        if (UnitySceneManager.GetActiveScene().name != Scenes.START)
        {
            GameManager.Scene.LoadScene(Scenes.START);
        }
    }
}
