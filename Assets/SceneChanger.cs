using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    // Attach this script to your button

    public string sceneToLoad; // Set the scene name in the Unity Editor

    private void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(ChangeScene);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
