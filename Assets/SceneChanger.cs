using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{

    public string sceneToLoad; 

    private void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(ChangeScene);
    }

    void ChangeScene()
    {
        // Reinitialize or clear static data here
        MarkSpawner.clearStaticData();
        SceneManager.LoadScene(sceneToLoad);
    }

}
