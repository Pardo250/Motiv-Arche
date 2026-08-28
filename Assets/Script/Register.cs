using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Register : MonoBehaviour
{

    public TMP_InputField nameField;
    public TMP_InputField lastnameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordConfirmationField;
    public GameObject LoadingPanel;

    RawImage loadingImage;
    TextMeshProUGUI loadingText;
    RawImage closeLoadingImage;

    public void Start()
    {
        GameObject.Find("Canvas/Background/Register/TitleApplication").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("tituloAplicacion");
        GameObject.Find("Canvas/Background/Register/Inputs/Title").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("tituloRegistro");
        GameObject.Find("Canvas/Background/Register/Inputs/InputfieldName/Text Area/Placeholder").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("inputfieldName");
        GameObject.Find("Canvas/Background/Register/Inputs/InputfieldLastname/Text Area/Placeholder").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("inputfieldLastname");
        GameObject.Find("Canvas/Background/Register/Inputs/InputfieldEmail/Text Area/Placeholder").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("inputfieldEmail");
        GameObject.Find("Canvas/Background/Register/Inputs/InputfieldPassword/Text Area/Placeholder").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("inputfieldPassword");
        GameObject.Find("Canvas/Background/Register/Inputs/InputfieldPasswordConfirmation/Text Area/Placeholder").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("inputfieldPasswordConfirmation");
        GameObject.Find("Canvas/Background/Register/Inputs/ButtonRegister/Text").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("buttonRegister");
        GameObject.Find("Canvas/Background/Register/Inputs/LoginText").GetComponent<TextMeshProUGUI>().text = Main.obtenerValor("tituloIniciarSesion");

    }

    public void realizarRegistro()
    {

        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
        StartCoroutine(RegisterOnClick());
    }

    IEnumerator RegisterOnClick()
    {
        WWWForm formData = new WWWForm();
        formData.AddField("reg_user", "register");
        formData.AddField("name", nameField.text);
        formData.AddField("lastname", lastnameField.text);
        formData.AddField("email", emailField.text);
        formData.AddField("password_1", passwordField.text);
        formData.AddField("password_2", passwordConfirmationField.text);
        formData.AddField("lang", Web.SystemLanguage);

        UnityWebRequest www = UnityWebRequest.Post(Main.dominio + "/server.php", formData);

        LoadingPanel.SetActive(false);
        www.SendWebRequest();


        LoadingPanel.SetActive(true);
        closeLoadingImage.gameObject.SetActive(false);

        while (!www.isDone)
        {
            yield return null;
            loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);

            loadingText.text ="Registrando ...";

        }
        LoadingPanel.SetActive(false);

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Unity Web request error:" + www.error);
            loadingText.text = www.error;
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
        }
        else
        {
            string result = www.downloadHandler.text;
            Debug.Log(result);
            if (www.downloadHandler.text.Contains("registro_correcto"))
            {
                LoadingPanel.SetActive(true);
                loadingText.text ="Registro correcto";
                loadingImage.gameObject.SetActive(false);
                closeLoadingImage.gameObject.SetActive(true);
            }
            else
            {
                string mensajeError = www.downloadHandler.text.Substring(0, www.downloadHandler.text.Length - 1);
                Debug.Log(mensajeError);
                if (mensajeError.Contains("|"))
                {
                    LoadingPanel.SetActive(true);
                    string[] mensajeErrores = mensajeError.Split('|');
                    mensajeError = "";
                    foreach (string error in mensajeErrores)
                    {
                        mensajeError = mensajeError + "\n" + error;
                    }

                    loadingText.text = mensajeError;
                    loadingImage.gameObject.SetActive(false);
                    closeLoadingImage.gameObject.SetActive(true);
                }
                else
                {
                    LoadingPanel.SetActive(true);
                    loadingText.text = mensajeError;
                    loadingImage.gameObject.SetActive(false);
                    closeLoadingImage.gameObject.SetActive(true);
                }
            }
        }
    }
}