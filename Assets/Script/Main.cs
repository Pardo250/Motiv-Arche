using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static CloudContentManager;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using System.Security.Cryptography.X509Certificates;

public class Main : MonoBehaviour
{


    
    public static string dominio = "http://api.motivarch.online";
    public static string dominioWeb = "https://motivarch.online";
    public static string dominioArchivos = "http://assets.motivarch.online/uploads";
    public static Dictionary<string,string> mensajes= new Dictionary<string, string>();
    public static string access_key = "31ec90e65ceb71e331ca6aa36486bdf7fcde254b";
    public static string secret_key = "3394c8a3708ebd6f68e6d044ea56ecaaf73ff168";
    public static string driveReal = "drive.google.com/open?id=";
    public static string driveReplace = "drive.google.com/uc?export=download&id=";
    public static string driveReal2 = "drive.google.com/file/d/";
    public static string driveReplace2 = "drive.google.com/uc?export=download&id=";
    public static string driveRep = "/view?usp=sharing";
    public static string fontStyle;
    public static List<GPSContenidos> contenidosHotspot = new List<GPSContenidos>();
    public static string email;
    public static string role;
    
    public TMP_Dropdown languageDropdown;


    public void Awake()
    {
        languageDropdown.value = Web.optionSelected;
    }

    public void Start()
    {
        mensajes.Clear();
        getSelectedLanguage();
    }

    public void getSelectedLanguage()
    {


        switch (languageDropdown.value)
        {
            case 0:
                Web.SystemLanguage = "es";
                Web.optionSelected = 0;
                ObtenerMensajes(Web.SystemLanguage);
                changeLanguageWords();
                languageDropdown.value = 0;
                break;
            case 1:
                Web.SystemLanguage = "en";
                Web.optionSelected = 1;
                ObtenerMensajes(Web.SystemLanguage);
                changeLanguageWords();
                languageDropdown.value = 1;
                break;
            case 2:
                Web.SystemLanguage = "cat";
                Web.optionSelected = 2;
                ObtenerMensajes(Web.SystemLanguage);
                changeLanguageWords();
                languageDropdown.value = 2;
                break;

        }
    }

    public void agregarIdiomas()
    {
        GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().options.Clear();

        string spanish = obtenerValor("spanishOption");
        string english = obtenerValor("englishOption");
        string catalan = obtenerValor("catalanOption");
        if (spanish != null)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(spanish);
            GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().options.Add(option);
        }
        if (english != null)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(english);
            GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().options.Add(option);
        }
        if (catalan != null)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(catalan);
            GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().options.Add(option);

        }

        if (GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().value == 0)
            GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().captionText.text = spanish;
        else if (GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().value == 1)
            GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().captionText.text = english;
        else if (GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().value == 2)
            GameObject.Find("Canvas/Background/Login/Inputs/LanguageDropdown").GetComponent<TMP_Dropdown>().captionText.text = catalan;
    }
    public void changeLanguageWords()
    {


        agregarIdiomas();


        List<string> llaves = new List<string>();
        List<TextMeshProUGUI> textos = new List<TextMeshProUGUI>();

        TextMeshProUGUI tituloAplicacion = GameObject.Find("Canvas/Background/Login/TitleApplication").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI tituloIniciarSesion = GameObject.Find("Canvas/Background/Login/Inputs/Title").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI inputfieldEmail = GameObject.Find("Canvas/Background/Login/Inputs/InputfieldEmail/Text Area/Placeholder").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI inputfieldPassword = GameObject.Find("Canvas/Background/Login/Inputs/InputfieldPassword/Text Area/Placeholder").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI buttonLogin = GameObject.Find("Canvas/Background/Login/Inputs/ButtonLogin/Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI registerText = GameObject.Find("Canvas/Background/Login/Inputs/RegisterText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI forgetPasswordText = GameObject.Find("Canvas/Background/Login/Inputs/ForgetPasswordText").GetComponent<TextMeshProUGUI>();



        llaves.Add("tituloAplicacion");
        llaves.Add("tituloIniciarSesion");
        llaves.Add("inputfieldEmail");
        llaves.Add("inputfieldPassword");
        llaves.Add("buttonLogin");
        llaves.Add("registerText");
        llaves.Add("forgetPasswordText");

        textos.Add(tituloAplicacion);
        textos.Add(tituloIniciarSesion);
        textos.Add(inputfieldEmail);
        textos.Add(inputfieldPassword);
        textos.Add(buttonLogin);
        textos.Add(registerText);
        textos.Add(forgetPasswordText);

        traducirElementos(llaves, textos);


    }

    public static void traducirElementos(List<string> llaves, List<TextMeshProUGUI> elementos)
    {
        for(int i=0;i<llaves.Count;i++)
        {
            string valor;
            if (mensajes.TryGetValue(llaves[i], out valor))
            {
                elementos[i].text = valor;
            }
        }
        
    }

    public static string obtenerValor(string llave)
    {
        string valor;
        if (mensajes.TryGetValue(llave, out valor))
        {
            return valor;
        }

        return null;
    }


    void ObtenerMensajes(string idioma)
    {
        string url = dominioArchivos/*.Replace("https","http")*/ + "/idiomas/" + idioma + ".txt";
        UnityWebRequest www = UnityWebRequest.Get(url);

        www.SendWebRequest();
        while (!www.isDone)
        {

        }

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string savePath = string.Format("{0}/{1}.txt", Application.persistentDataPath, idioma);
            System.IO.File.WriteAllText(savePath, www.downloadHandler.text);
            string[] lines = System.IO.File.ReadAllLines(savePath);
            if (mensajes != null)
                mensajes.Clear();
            foreach (string line in lines)
            {
                string[] llaveValor = line.Replace("\\n", "\n").Split(":");
                string llave = llaveValor[0];
                string valor = llaveValor[1];
                mensajes.Add(llave, valor);
            }
        }
    }



   
}




