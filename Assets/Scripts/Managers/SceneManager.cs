using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : IManager
{
    public string CurrentSceneName = Scenes.NONE;

    private const string LOADING_SCENE_NAME = "00_LoadingScene";

    private LoadingUI _loading;

    public void Initialize()
    {
        
    }

    public void Release()
    {
        
    }

    public async void LoadScene(string sceneName)
    {
        CurrentSceneName = sceneName;

        await LoadSceneAsyncWithLoadScene(sceneName);
    }

    public async UniTask LoadSceneAsyncWithLoadScene(string sceneName)
    {
        await UnitySceneManager.LoadSceneAsync(LOADING_SCENE_NAME, LoadSceneMode.Additive);

        var mono = Object.FindAnyObjectByType<LoadingUI>();

        if(mono is LoadingUI loadingScript)
        {
            _loading = loadingScript;
        }

        if(null == _loading)
        {
            Debug.LogError("�ε� �� ��  Loading_UI�� ã�� ����");
            return;
        }

        await PerformSceneTransition(sceneName);
    }

    private async UniTask PerformSceneTransition(string sceneName)
    {
        var currentScene = UnitySceneManager.GetActiveScene().name;
        if(string.IsNullOrEmpty(currentScene) )
        {
            Debug.LogError("���� Ȱ��ȭ �� �� �̸��� �����ϴ�.");
            return;
        }

        if (UnitySceneManager.GetSceneByName(currentScene).isLoaded)
        {
            var unloadSceneAsync = UnitySceneManager.UnloadSceneAsync(currentScene);

            while(!unloadSceneAsync.isDone)
            {
                UpdateLoadingUI($"{unloadSceneAsync.progress * 100:F0}%", unloadSceneAsync.progress);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        else
        {
            Debug.LogError($"���� �� {currentScene}�� �̹� ��ε� �� �����Դϴ�.");
        }

        var sceneLoad = UnitySceneManager.LoadSceneAsync(sceneName);
        if(sceneLoad == null)
        {
            Debug.LogError($"�� {sceneName}�� ã�� �� �����ϴ�.");
            return;
        }

        while (!sceneLoad.isDone)
        {
            UpdateLoadingUI($"{sceneLoad.progress * 100:F0}%", sceneLoad.progress);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        //�� Ȱ��ȭ ���
        sceneLoad.allowSceneActivation = true;
        while (!sceneLoad.isDone)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        //������ ���������� ��ٸ���
        await UniTask.WaitForEndOfFrame();
    }

    public void UpdateLoadingUI(string text, float progress)
    {
        _loading.UpdateLoadingUI(text, progress);
    }
}
