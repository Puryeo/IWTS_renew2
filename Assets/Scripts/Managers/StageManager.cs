using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : IManager
{
    /// <summary>
    /// ����� �������� �� �̸��� ��Ƶδ� ����, ������ �ʿ���� �ѵ� �ϴ� ����
    /// </summary>
    private static readonly HashSet<string> clearedStages = new();

    public List<string> _allStages;

    public void Initialize()
    {
        _allStages = new List<string>
        {
            Scenes.TUTORIAL,
            Scenes.STEP1,
            Scenes.STEP2,
        };
    }

    public void Release()
    {
        clearedStages.Clear();
    }

    /// <summary>
    /// �������� ����Ͽ����� ȣ���ϴ� �޼���
    /// </summary>
    /// <param name="stageName">�������� �� �̸�</param>
    public void ClearedStage(string stageName)
    {
        clearedStages.Add(stageName);
        int currentStageIdx = _allStages.IndexOf(stageName);
        if(currentStageIdx != -1 && currentStageIdx + 1 < _allStages.Count)
        {
            string nextStageName = _allStages[currentStageIdx + 1];
            GameManager.Scene.LoadScene(nextStageName);
        }
    }

    public bool IsStageCleared(string stageName)
    {
        return clearedStages.Contains(stageName);
    }
}
