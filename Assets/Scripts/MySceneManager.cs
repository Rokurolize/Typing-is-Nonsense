using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;



public class MySceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // �i�f�o�b�O���p�jEditor �Ń��[�h���Ă��� ManagerScene �ȊO�̃V�[��������
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string sceneName = SceneManager.GetSceneAt(i).name;
            if (sceneName != "ManagerScene")
                SceneManager.UnloadSceneAsync(sceneName);
        }

        // ���[�h���I������O�ɃA�N�e�B�u�����悤�Ƃ��Ă��܂��̂ŁA���[�h���ɒ��ڃA�N�e�B�u�������A�C�x���g�n���h���ŏ���
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Additive);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ChangeSceneRequest(string targetScene)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "ManagerScene")
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene(targetScene, LoadSceneMode.Additive);
    }

    public void OnSceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        /// EventSystem �͏�Ɉ�ɕۂ�
        /// �܂��O�̃V�[���̂��̂������� Deactivate ���A���̃V�[���� �I�u�W�F�N�g EventSystem�i�R���|�[�l���g EventSystem �����Ă���j�������� Activate
        DeactivateCurrentEventSystem();
        Debug.Log($"{nextScene.name} is loaded.");
        SceneManager.SetActiveScene(nextScene);
        GameObject[] nextSceneGameObjects = nextScene.GetRootGameObjects();
        foreach(GameObject ob in nextSceneGameObjects)
        {
            if (ob.GetComponent<EventSystem>() != null)
            {
                ob.SetActive(true);
                EventSystem.current = ob.GetComponent<EventSystem>();
            }
        }

    }

    public void DeactivateCurrentEventSystem()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] currentSceneGameObjects = currentScene.GetRootGameObjects();
        foreach (GameObject ob in currentSceneGameObjects)
        {
            if (ob.GetComponent<EventSystem>() != null)
            {
                ob.SetActive(false);
            }
        }


    }
}
