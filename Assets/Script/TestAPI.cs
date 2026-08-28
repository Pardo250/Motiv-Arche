using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAPI : MonoBehaviour
{

    public string Email;
    public string Password;
    public string ModelUIDToDownload;

    void Start()
    {
        SketchfabAPI.GetAccessToken(Email, Password, (SketchfabResponse<SketchfabAccessToken> answer) =>
        {
            if (answer.Success)
            {
                SketchfabAPI.AuthorizeWithAccessToken(answer.Object);
                DownloadModel();
            }
            else
            {
                Debug.LogError(answer.ToString());
                Debug.LogError(answer.ErrorMessage);
            }

        });
    }


    private void DownloadModel()
    {
        // This first call will get the model information
        SketchfabAPI.GetModel(ModelUIDToDownload, (resp) =>
        {
            // This second call will get the model information, download it and instantiate it
            SketchfabModelImporter.Import(resp.Object, (obj) =>
            {
                if (obj != null)
                {
                    // Here you can do anything you like to obj (A unity game object containing the sketchfab model)
                }
            });
        });
    }

}
