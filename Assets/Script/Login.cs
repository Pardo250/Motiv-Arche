using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.SceneManagement;


// Based on https://www.owasp.org/index.php/Certificate_and_Public_Key_Pinning#.Net
class AcceptAllCertificatesSignedWithASpecificPublicKey : CertificateHandler
{
    // Encoded RSAPublicKey
    private static string PUB_KEY = "30818902818100C4A06B7B52F8D17DC1CCB47362" +
        "C64AB799AAE19E245A7559E9CEEC7D8AA4DF07CB0B21FDFD763C63A313A668FE9D764E" +
        "D913C51A676788DB62AF624F422C2F112C1316922AA5D37823CD9F43D1FC54513D14B2" +
        "9E36991F08A042C42EAAEEE5FE8E2CB10167174A359CEBF6FACC2C9CA933AD403137EE" +
        "2C3F4CBED9460129C72B0203010001";

    protected override bool ValidateCertificate(byte[] certificateData)
    {
        X509Certificate2 certificate = new X509Certificate2(certificateData);
        string pk = certificate.GetPublicKeyString();
        Debug.Log(pk);
        //return pk.Equals(PUB_KEY));
        return true;
    }
}


public class Login : MonoBehaviour
{

    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;
    public GameObject LoadingPanel;
    

    public void loginUser()
    {
        string email = UsernameInput.text;
        string password = PasswordInput.text;
        RawImage loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        TextMeshProUGUI loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        RawImage closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();

        loadingText.text = Main.obtenerValor("textLoading");

        string uri = Main.dominio+"/server.php";

        if (!email.Trim().Equals("") && !password.Trim().Equals(""))
        {
            WWWForm www = new WWWForm();
            www.AddField("email", email);
            www.AddField("password",password);
            www.AddField("login_user", "login");
            www.AddField("lang", Web.SystemLanguage);
            StartCoroutine(createRequest(www, uri,loadingImage,loadingText,closeLoadingImage,email));
        }
        else
        {
            LoadingPanel.SetActive(true);
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
            loadingText.text = Main.obtenerValor("errorFieldLogin");
        }
    }


    IEnumerator createRequest(WWWForm www, string uri,RawImage loadingImage, TextMeshProUGUI loadingText,RawImage closeLoadingImage,string email)
    {
        UnityWebRequest request = UnityWebRequest.Post(uri,www);
        LoadingPanel.SetActive(false);
        request.SendWebRequest();

        
        LoadingPanel.SetActive(true);
        closeLoadingImage.gameObject.SetActive(false);
        while (!request.isDone)
        {
            yield return null;
            loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
            loadingText.text = "Iniciando sesión ...";
        }

        if(request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Unity Web request error:"+ request.error);
            loadingText.text = request.error;
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
        }
        else {
            
            string response = request.downloadHandler.text.Trim();
            if (!response.Equals(""))
            {
               
                string[] results = response.Split("|");
                if (results[0].Equals("login_correcto"))
                {
                    LoadingPanel.SetActive(false);
                    Main.email = email;
                    SceneManager.LoadScene("MenuScene");
                }
                else
                {
                    string errors = "";
                    foreach(string r in results)
                    {
                        errors = errors + "\n" + r;
                    } 
                    loadingText.text = errors;
                    loadingImage.gameObject.SetActive(false);
                    closeLoadingImage.gameObject.SetActive(true);
                }
            }
            else
            {
                loadingText.text = "Error del servidor";
                loadingImage.gameObject.SetActive(false);
                closeLoadingImage.gameObject.SetActive(true);
            }
            

        }
    }


}
