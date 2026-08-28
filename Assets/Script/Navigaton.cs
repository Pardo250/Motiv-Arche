using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigaton : MonoBehaviour
{


    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    public void cargarPaginaWeb()
    {

        InAppBrowser.OpenURL("https://motivarch.online/");
    }
}
