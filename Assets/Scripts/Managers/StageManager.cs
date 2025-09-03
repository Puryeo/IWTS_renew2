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
            Scenes.FINAL,
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
        Debug.Log($"{stageName} Ŭ����");
        clearedStages.Add(stageName);
        int currentStageIdx = _allStages.IndexOf(stageName);
        if(currentStageIdx != -1 && currentStageIdx + 1 < _allStages.Count)
        {
            string nextStageName = _allStages[currentStageIdx + 1];
            GameManager.Scene.LoadScene(nextStageName);
        }

        if(stageName == Scenes.FINAL)
        {
            FinalClear();
        }
    }

    public bool IsStageCleared(string stageName)
    {
        return clearedStages.Contains(stageName);
    }

    private void FinalClear()
    {
        //������ ���� ���� ���ξ����� �̵�
        GameManager.Scene.LoadScene(Scenes.START);
    }
}
