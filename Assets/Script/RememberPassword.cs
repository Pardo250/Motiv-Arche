using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RememberPassword : MonoBehaviour
{

    public TMP_InputField EmailInput;
    public GameObject LoadingPanel;


    public void Start()
    {
        GameObject.Find("Canvas/Background/ForgetPassword/TitleApplication").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("tituloAplicacion");
        GameObject.Find("Canvas/Background/ForgetPassword/Inputs/Title").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("tituloRecordarPassword");
        GameObject.Find("Canvas/Background/ForgetPassword/Inputs/InputfieldEmail/Text Area/Placeholder").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("inputfieldEmail");
        GameObject.Find("Canvas/Background/ForgetPassword/Inputs/ButtonForgetPassword/Text").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("botonRecordarPassword");
        GameObject.Find("Canvas/Background/ForgetPassword/Inputs/LoginText").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("tituloIniciarSesion");
    }

    public void rememberPassword()
    {
        string email = EmailInput.text;
        RawImage loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        TextMeshProUGUI loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        RawImage closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();

        string uri = Main.dominio + "/forgot-password.php";

        if (!email.Equals(""))
        {
            WWWForm www = new WWWForm();
            www.AddField("email",email);
            www.AddField("lang", Web.SystemLanguage);
            StartCoroutine(recordarContraseñaRequest(www, uri, loadingImage, loadingText, closeLoadingImage));
        }
        else
        {
            LoadingPanel.SetActive(true);
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
            loadingText.text = Main.obtenerValor("inputfieldEmail");
        }
    }

    IEnumerator recordarContraseñaRequest(WWWForm www, string uri, RawImage loadingImage, TextMeshProUGUI loadingText, RawImage closeLoadingImage)
    {
        UnityWebRequest request = UnityWebRequest.Post(uri, www);
        LoadingPanel.SetActive(false);
        request.SendWebRequest();


        LoadingPanel.SetActive(true);
        closeLoadingImage.gameObject.SetActive(false);
        while (!request.isDone)
        {
            yield return null;
            loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
            loadingText.text = Main.obtenerValor("enviarCorreoPassword");
        }

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Error de petición:" + request.error);
            loadingText.text = request.error;
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
        }
        else
        {
            string response = request.downloadHandler.text.Trim();
            if (!response.Equals("success"))
            {
                loadingText.text = Main.obtenerValor("enviarCorreoPasswordConfirmacion");
                loadingImage.gameObject.SetActive(false);
                closeLoadingImage.gameObject.SetActive(true);
            }
            else
            {
                loadingText.text = response;
                loadingImage.gameObject.SetActive(false);
                closeLoadingImage.gameObject.SetActive(true);
            }
        }
    }
}
