/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.
 
Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using LightShaft.Scripts;
using TriLibCore;
using System.IO;
using Lean.Touch;
using Paroxe.PdfRenderer;
using System;

public class CloudContentManager : MonoBehaviour
{

    
    public GameObject MenuOptions;
    public GameObject PlayOptions;
    public TMP_Dropdown DropdownElementos;
    public TMP_Dropdown DropdownAnimaciones;
    public AudioSource AudioSource;
    public GameObject LoadingPanel;
    public GameObject ScrollAreaText;
    
    //public GameObject PanelAugmented;
    public Texture whiteTexture;
    public GameObject YoutubePlayer;
    public Texture YoutubeTexture;
    private GameObject _rootGameObject;
    public GameObject model3d;
    private GameObject model;
    public GameObject ModelOptions;

    public GameObject GPSContentPanel;
    public Camera ARCamera;
    public GameObject namePoi;


    YoutubePlayer youtubePlayer;
    GameObject botonInicio;
    GameObject botonPausa;
    GameObject botonParar;
    GameObject botonCerrar;
    Button botonPlay;
    Button botonPause;
    Button botonStop;
    Button botonClose;
    GameObject SliderEscalarModelo;
    GameObject SliderRotarModelo;
    GameObject SliderEscalarModeloX;
    GameObject SliderEscalarModeloY;
    GameObject SliderEscalarModeloZ;
    GameObject SliderRotarModeloX;
    GameObject SliderRotarModeloY;
    GameObject SliderRotarModeloZ;
    Slider escalarX;
    Slider escalarY;
    Slider escalarZ;
    Slider rotarX;
    Slider rotarY;
    Slider rotarZ;

    List<string> audios = new List<string>();
    List<string> imagenes = new List<string>();
    List<string> videos = new List<string>();
    List<string> modelos = new List<string>();
    List<string> webs = new List<string>();
    List<string> textos = new List<string>();
    List<string> pdfs = new List<string>();
    bool contents = false;

    public GameObject map;

    public RawImage imageGPS;

    List<string> descripciones = new List<string>();
    public PDFViewer pdfViewer;
    public GameObject pdfViewerObj;

    RawImage loadingImage;
    TextMeshProUGUI loadingText;
    RawImage closeLoadingImage;


    TextMeshProUGUI nombrePunto;
    TextMeshProUGUI textContent;

    string sketchfabEmail;
    string sketchfabPassword;

    private Vector3 escalaInicial;
    public float velocidadDeEscalado = 0.5f;

    [System.Serializable]
    public class PosicionGeograficaContenido
    {
        public string name;
        public string descripcion;
        public string url;
        public string tipo_contenido;
        public string latitud;
        public string longitud;
    }

    [System.Serializable]
    public class ListaGPSContenido
    {
        public List<PosicionGeograficaContenido> gps;
    }

    public class ContenidoInfo
    {
        public string descripcion;
        public string url;
        // TODO FALTA PONER LOS TAGS
    }

    public class GPSContenidos
    {
        public List<ContenidoInfo> audios;
        public List<ContenidoInfo> imagenes;
        public List<ContenidoInfo> videos;
        public List<ContenidoInfo> modelos;
        public List<ContenidoInfo> webs;
        public List<ContenidoInfo> textos;
        public List<ContenidoInfo> pdfs;
        public string latitud;
        public string longitud;
        public string name;


    }

    IEnumerator obtenerCredencialesSketchfab()
    {
        WWWForm form = new WWWForm();
        form.AddField("obtenerCredencialesSketchfab", "credenciales");
        form.AddField("email", "juangonzalez1221@gmail.com");

        UnityWebRequest www = UnityWebRequest.Post(Main.dominio + "/UnitySQL.php", form);
        www.SendWebRequest();
        while (!www.isDone)
            yield return null;

        if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string response = www.downloadHandler.text;
            if (response.Equals("no existe"))
            {
                //TODO IMPRIMIR QUE EL EMAIL NO EXISTE EN SKETCHFAB
            }
            else
            {
                Debug.Log(response);
                string[] credenciales = response.Split("|");
                sketchfabEmail = credenciales[0];
                sketchfabPassword = credenciales[1];
                Debug.Log(sketchfabEmail);
                Debug.Log(sketchfabPassword);
            }
        }
    }

    public void Start()
    {
        namePoi.SetActive(false);
        nombrePunto = namePoi.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
        textContent = ScrollAreaText.transform.GetChild(0).gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        youtubePlayer = YoutubePlayer.GetComponent<YoutubePlayer>();
        botonInicio = PlayOptions.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        botonPausa = PlayOptions.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;
        botonParar = PlayOptions.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject;
        botonCerrar = PlayOptions.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject;
        botonPlay = botonInicio.GetComponent<Button>();
        botonPause = botonPausa.GetComponent<Button>();
        botonStop = botonParar.GetComponent<Button>();
        botonClose = botonCerrar.GetComponent<Button>();


        SliderEscalarModelo = ModelOptions.transform.GetChild(1).gameObject;
        SliderRotarModelo = ModelOptions.transform.GetChild(2).gameObject;
        SliderEscalarModeloX = ModelOptions.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject;
        SliderEscalarModeloY = ModelOptions.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject;
        SliderEscalarModeloZ = ModelOptions.transform.GetChild(1).gameObject.transform.GetChild(2).gameObject;
        SliderRotarModeloX = ModelOptions.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject;
        SliderRotarModeloY = ModelOptions.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject;
        SliderRotarModeloZ = ModelOptions.transform.GetChild(2).gameObject.transform.GetChild(2).gameObject;
        escalarX = SliderEscalarModeloX.GetComponent<Slider>();
        escalarY = SliderEscalarModeloY.GetComponent<Slider>();
        escalarZ = SliderEscalarModeloZ.GetComponent<Slider>();
        rotarX = SliderRotarModeloX.GetComponent<Slider>();
        rotarY = SliderRotarModeloY.GetComponent<Slider>();
        rotarZ = SliderRotarModeloZ.GetComponent<Slider>();
        StartCoroutine(obtenerCredencialesSketchfab());
    }

    public void HandleMetadata(string metadata,string poiName)
    {
        WebRequest(metadata,poiName);
    }


    public void WebRequest(string metadata,string poiName)
    {
        Debug.Log("WebRequest() called: \n" + metadata);

        string[] contenidos = metadata.Split('\n');
        namePoi.SetActive(true);
        nombrePunto.text = poiName;
        if (contenidos.Length > 0)
        {
            contents = true;
            audios.Clear();
            imagenes.Clear();
            videos.Clear();
            modelos.Clear();
            webs.Clear();
            textos.Clear();
            pdfs.Clear();
            revisarContenidos(contenidos);
            cargarContenidos(poiName);
        }
    }

    public void revisarContenidos(string [] urls)
    {
        foreach(string contenido in urls)
        {
            if (contenido.Contains("sonido:"))
            {
                string [] urlSonidos = contenido.Replace("sonido:", "").Split("|");
                foreach(string a in urlSonidos)
                {
                    audios.Add(a);
                }
            }
            else if (contenido.Contains("imagen:"))
            {
                string[] urlImagenes = contenido.Replace("imagen:", "").Split("|");
                foreach (string a in urlImagenes)
                {
                    imagenes.Add(a);
                }
            }
            else if (contenido.Contains("video:"))
            {
                string[] urlVideos = contenido.Replace("video:", "").Split("|");
                foreach (string a in urlVideos)
                {
                    videos.Add(a);
                }
            }
            else if (contenido.Contains("modelo:"))
            {
                string[] urlModelos = contenido.Replace("modelo:", "").Split("|");
                foreach (string a in urlModelos)
                {
                    modelos.Add(a);
                }
            }
            else if (contenido.Contains("web:"))
            {
                string[] urlWebs = contenido.Replace("web:", "").Split("|");
                foreach (string a in urlWebs)
                {
                    webs.Add(a);
                }
            }
            else if (contenido.Contains("texto:"))
            {
                string[] urlTextos = contenido.Replace("texto:", "").Split("|");
                foreach (string a in urlTextos)
                {
                    textos.Add(a);
                }
            }
            else if (contenido.Contains("pdf:"))
            {
                string[] urlPdfs = contenido.Replace("pdf:", "").Split("|");
                foreach (string a in urlPdfs)
                {
                    pdfs.Add(a);
                }
            }
        }
    }


    public void cargarContenidos(string poiName)
    {
       
            if (contents)
            {
                MenuOptions.SetActive(true);
                GameObject botonAudio = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject botonImagen = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;
                GameObject botonVideo = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject;
                GameObject botonModelo = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject;
                GameObject botonWeb = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(4).gameObject;
                GameObject botonTexto = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(5).gameObject;
                GameObject botonPdf = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(6).gameObject;
                GameObject botonCerrar = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(7).gameObject;

                botonAudio.SetActive(false);
                botonImagen.SetActive(false);
                botonVideo.SetActive(false);
                botonModelo.SetActive(false);
                botonWeb.SetActive(false);
                botonTexto.SetActive(false);
                botonPdf.SetActive(false);
                botonCerrar.SetActive(true);
                if (audios.Count > 0)
                {
                    botonAudio.SetActive(true);
                    Button audioButton = botonAudio.GetComponent<Button>();
                    audioButton.onClick.RemoveAllListeners();
                    audioButton.onClick.AddListener(() => agregarContenidosDropdown(audios, "audio", poiName));
                }

                if (imagenes.Count > 0)
                {
                    botonImagen.SetActive(true);
                    Button imagenButton = botonImagen.GetComponent<Button>();
                    imagenButton.onClick.RemoveAllListeners();
                    imagenButton.onClick.AddListener(() => agregarContenidosDropdown(imagenes, "imagen", poiName));
                }

                if (videos.Count > 0)
                {
                    botonVideo.SetActive(true);
                    Button videoButton = botonVideo.GetComponent<Button>();
                    videoButton.onClick.RemoveAllListeners();
                    videoButton.onClick.AddListener(() => agregarContenidosDropdown(videos, "video", poiName));
                }
                if (modelos.Count > 0)
                {
                    botonModelo.SetActive(true);
                    Button modeloButton = botonModelo.GetComponent<Button>();
                    modeloButton.onClick.RemoveAllListeners();
                    modeloButton.onClick.AddListener(() => agregarContenidosDropdown(modelos, "modelo", poiName));
                }
                if (webs.Count > 0)
                {
                    botonWeb.SetActive(true);
                    Button webButton = botonWeb.GetComponent<Button>();
                    webButton.onClick.RemoveAllListeners();
                    webButton.onClick.AddListener(() => agregarContenidosDropdown(webs, "web", poiName));
                }
                if (textos.Count > 0)
                {
                    botonTexto.SetActive(true);
                    Button textoButton = botonTexto.GetComponent<Button>();
                    textoButton.onClick.RemoveAllListeners();
                    textoButton.onClick.AddListener(() => agregarContenidosDropdown(textos, "texto", poiName));
                }
                if (pdfs.Count > 0)
                {
                    botonPdf.SetActive(true);
                    Button pdfButton = botonPdf.GetComponent<Button>();
                    pdfButton.onClick.RemoveAllListeners();
                    pdfButton.onClick.AddListener(() => agregarContenidosDropdown(pdfs, "pdf", poiName));
                }



            botonCerrar.GetComponent<Button>().onClick.RemoveAllListeners();
                botonCerrar.GetComponent<Button>().onClick.AddListener(() => hacerNuevoReconocimiento());
            }
            else
            {
                MenuOptions.SetActive(false);
                PlayOptions.SetActive(false);
            }

        
    }

    public void hacerNuevoReconocimiento()
    {
        pausarContenidos();
        MenuOptions.SetActive(false);
        nombrePunto.text = "";
        namePoi.SetActive(false);

        DropdownElementos.ClearOptions();
        DropdownAnimaciones.ClearOptions();
        DropdownElementos.transform.gameObject.SetActive(false);
        DropdownAnimaciones.transform.gameObject.SetActive(false);

    }

    private void agregarContenidosDropdown(List<string> contenidos, string tipoContenido,string poiName)
    {
        DropdownElementos.transform.gameObject.SetActive(true);
        DropdownElementos.ClearOptions();

        //TODO aqui es donde debo buscar la descripcion de cada uno de los contenidos
        List<string> tipoContenidos=new List<string>();

        //TODO toca aqui cambiar para no poner solo video web, etc
        StartCoroutine(consultarDescripcionContenidos(tipoContenido,poiName,tipoContenidos,contenidos));
    }

    IEnumerator consultarDescripcionContenidos(string tipoContenido,string nombrePunto,List<string> tipoContenidos, List<string> contenidos)
    {
        descripciones.Clear();
        WWWForm form = new WWWForm();
        form.AddField("obtenerDescripcionContenidosNombre", "consulta");
        form.AddField("name", nombrePunto.Remove(nombrePunto.Length - 1));
        form.AddField("tipo", tipoContenido);
        Debug.Log(nombrePunto.Remove(nombrePunto.Length - 1));
        Debug.Log(tipoContenido);

        UnityWebRequest www = UnityWebRequest.Post(Main.dominio + "/UnitySQL.php", form);
        www.SendWebRequest();
        while (!www.isDone)
            yield return null;

        if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string results = www.downloadHandler.text;
            
            string [] resultadoDes = results.Split("|");

            descripciones.Add("Seleccionar contenido " + tipoContenido);
            for(int i = 0; i < resultadoDes.Length; i++)
            {
                Debug.Log(resultadoDes[i]);
                descripciones.Add(resultadoDes[i]);
            }

            Debug.Log(descripciones.Count);
            if (descripciones.Count > 0)
            {
                foreach (string contenido in descripciones)
                {
                    tipoContenidos.Add(contenido);
                }
            }
            else
            {
                int contador = 0;
                foreach (string contenido in contenidos)
                {
                    contador = contador + 1;
                    tipoContenidos.Add(tipoContenido + " " + (contador));
                }
            }

            DropdownElementos.AddOptions(tipoContenidos);
            DropdownElementos.onValueChanged.RemoveAllListeners();

            DropdownElementos.onValueChanged.AddListener(delegate { StartCoroutine(reproducirContenidos(tipoContenido, contenidos, nombrePunto)); });

        }
    }

    IEnumerator reproducirContenidos(string tipoContenido, List<string> contenidos, string poiName)
    {
        pausarContenidos();
        int index = DropdownElementos.value;
        if(index != 0)
        {
            string url = contenidos[index-1];
            Debug.Log("url del contenido:" + url);
            string urlWeb = url;

            if (url.Contains("https"))
            {
                url = url.Replace("https", "http");
            }


            UnityWebRequest www;
            LoadingPanel.SetActive(true);
            
            RawImage loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
            TextMeshProUGUI loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            RawImage closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
            ScrollAreaText.SetActive(false);

            loadingImage.gameObject.SetActive(true);
            closeLoadingImage.gameObject.SetActive(true);

            if (tipoContenido.Equals("audio"))
            {
                
                www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
                www.certificateHandler = null;
                

                www.SendWebRequest();
                

                while (!www.isDone)
                {
                    yield return null;
                    loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
                    loadingText.text = Main.obtenerValor("textLoading")+" ...";
                }

                if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(Main.obtenerValor("errorDownload")+":" + www.error);
                    loadingText.text = www.error;
                    loadingImage.gameObject.SetActive(false);
                    closeLoadingImage.gameObject.SetActive(true);
                }
                else
                {
                    LoadingPanel.SetActive(false);
                    AudioSource.playOnAwake = false;
                    AudioSource.clip = DownloadHandlerAudioClip.GetContent(www);

                    PlayOptions.SetActive(true);
                    MenuOptions.SetActive(false);


                    botonPlay.onClick.RemoveAllListeners();
                    botonPause.onClick.RemoveAllListeners();
                    botonStop.onClick.RemoveAllListeners();
                    botonClose.onClick.RemoveAllListeners();

                    botonInicio.SetActive(true);
                    botonPausa.SetActive(false);
                    botonParar.SetActive(false);

                    botonPlay.onClick.AddListener(() => iniciarAudio(AudioSource,botonPausa,botonInicio,botonParar));
                    botonPause.onClick.AddListener(() => pausarAudio(AudioSource, botonPausa, botonInicio,botonParar));
                    botonStop.onClick.AddListener(() => pararAudio(AudioSource, botonPausa, botonInicio,botonParar));
                    botonClose.onClick.AddListener(() => mostrarContenidos());

                    www.Dispose();
                }
            }
            else if (tipoContenido.Equals("imagen"))
            {
                www = UnityWebRequestTexture.GetTexture(url);
                www.certificateHandler = null;
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                    loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
                    loadingText.text = Main.obtenerValor("textLoading") + " ...";
                }

                if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error descarga imagen:" + www.error);
                    loadingText.text = www.error;
                    loadingImage.gameObject.SetActive(false);
                    closeLoadingImage.gameObject.SetActive(true);
                }
                else
                {
                    LoadingPanel.SetActive(false);
                        imageGPS.gameObject.SetActive(true);
                        imageGPS.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    www.Dispose();
                }
            }
            else if (tipoContenido.Equals("video"))
            {
                    imageGPS.gameObject.SetActive(true);
                    imageGPS.texture = whiteTexture;

                youtubePlayer = YoutubePlayer.GetComponent<YoutubePlayer>();

                bool youtube = false;
                if (url.Contains("youtu.be") || url.Contains("youtube"))
                {
                    youtube = true;
                    youtubePlayer = YoutubePlayer.GetComponent<YoutubePlayer>();
                    youtubePlayer.PreLoadVideo(url);

                    LoadingPanel.SetActive(false);
                    MenuOptions.SetActive(false);
                    PlayOptions.SetActive(true);


                    botonPlay.onClick.RemoveAllListeners();
                    botonPause.onClick.RemoveAllListeners();
                    botonStop.onClick.RemoveAllListeners();
                    botonClose.onClick.RemoveAllListeners();

                    botonInicio.SetActive(true);
                    botonPausa.SetActive(false);
                    botonParar.SetActive(false);

                    botonPlay.onClick.AddListener(() => iniciarVideo(youtubePlayer, botonPausa, botonInicio, botonParar,youtube));
                    botonPause.onClick.AddListener(() => pausarVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube));
                    botonStop.onClick.AddListener(() => pararVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube));
                    botonClose.onClick.AddListener(() => mostrarContenidos());
                }
                else
                {
                    youtubePlayer.videoPlayer.url = url;
                    LoadingPanel.SetActive(false);
                    MenuOptions.SetActive(false);
                    PlayOptions.SetActive(true);

                    

                    botonPlay.onClick.RemoveAllListeners();
                    botonPause.onClick.RemoveAllListeners();
                    botonStop.onClick.RemoveAllListeners();
                    botonClose.onClick.RemoveAllListeners();

                    botonInicio.SetActive(true);
                    botonPausa.SetActive(false);
                    botonParar.SetActive(false);

                    botonPlay.onClick.AddListener(() => iniciarVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube));
                    botonPause.onClick.AddListener(() => pausarVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube));
                    botonStop.onClick.AddListener(() => pararVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube));
                    botonClose.onClick.AddListener(() => mostrarContenidos());
                }
                
                

            }
            else if (tipoContenido.Equals("modelo"))
            {

                ModelOptions.SetActive(true);
                www = UnityWebRequest.Get(url);
                www.certificateHandler = null;
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                    loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
                    loadingText.text = Main.obtenerValor("textLoading")+" ...";
                }

                if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error descarga modelo:" + www.error);
                    loadingText.text = www.error;
                    loadingImage.gameObject.SetActive(false);
                    closeLoadingImage.gameObject.SetActive(true);
                }
                else
                {
                    if (url.Contains("sketchfab.com"))
                    {
                        Debug.Log(url);
                        string[] file = url.Split('/');
                        string sketchfabModel = null;
                        sketchfabModel = file[file.Length - 1];
                        string[] splitSketchfabModel = sketchfabModel.Split("-");
                        string modelID = splitSketchfabModel[splitSketchfabModel.Length - 1];
                        validarCredencialesSketchfab(modelID);
                        Button escalar = ModelOptions.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>();
                        Button rotar = ModelOptions.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.GetComponent<Button>();
                        Button cerrar = ModelOptions.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject.GetComponent<Button>();

                        escalar.onClick.RemoveAllListeners();
                        rotar.onClick.RemoveAllListeners();
                        cerrar.onClick.RemoveAllListeners();

                        SliderEscalarModelo.SetActive(false);
                        SliderRotarModelo.SetActive(false);
                        MenuOptions.SetActive(false);
                        escalar.onClick.AddListener(() => mostrarOcultarSliders("escalar"));
                        rotar.onClick.AddListener(() => mostrarOcultarSliders("rotar"));
                        cerrar.onClick.AddListener(() => mostrarContenidos());
                    }
                    else
                    {
                        string[] extensionFile = url.Split('/');
                        string fileWithExtension = null;
                        if (extensionFile != null)
                        {
                            imageGPS.color = new Color32(255, 255, 255, 0);
                            fileWithExtension = extensionFile[extensionFile.Length - 1];
                            var fullPath = Application.persistentDataPath + "/" + poiName + "-" + fileWithExtension;
                            File.WriteAllBytes(fullPath.Trim(), www.downloadHandler.data);
                            Button escalar = ModelOptions.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>();
                            Button rotar = ModelOptions.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.GetComponent<Button>();
                            Button cerrar = ModelOptions.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject.GetComponent<Button>();

                            escalar.onClick.RemoveAllListeners();
                            rotar.onClick.RemoveAllListeners();
                            cerrar.onClick.RemoveAllListeners();

                            SliderEscalarModelo.SetActive(false);
                            SliderRotarModelo.SetActive(false);
                            MenuOptions.SetActive(false);
                            escalar.onClick.AddListener(() => mostrarOcultarSliders("escalar"));
                            rotar.onClick.AddListener(() => mostrarOcultarSliders("rotar"));
                            cerrar.onClick.AddListener(() => mostrarContenidos());

                            StartCoroutine(cargarModelo3D(fullPath.Trim()));
                        }
                    }


                    
                }

            }
            else if (tipoContenido.Equals("web"))
            {
                LoadingPanel.SetActive(false);
                PlayOptions.SetActive(false);
                string extn = System.IO.Path.GetExtension(urlWeb);
                Debug.Log(extn);

                if (extn.ToUpper().Equals(".PDF"))
                {
                    pdfViewerObj.SetActive(true);
                    pdfViewer.LoadDocumentFromWeb(urlWeb);
                }
                else
                {
                    InAppBrowser.OpenURL(urlWeb);
                }

            }
            else if (tipoContenido.Equals("texto"))
            {

                ScrollAreaText.SetActive(true);
                loadingText.text = "";
                textContent.text = url;
                loadingImage.gameObject.SetActive(false);
                closeLoadingImage.gameObject.SetActive(true);
            }
            else if (tipoContenido.Equals("pdf"))
            {
                LoadingPanel.SetActive(false);
                PlayOptions.SetActive(false);
                if (url.Contains("assets.motivarch.online"))
                {
                    if (url.Contains("https"))
                    {
                        url = url.Replace("https", "http");
                    }
                }

                pdfViewerObj.SetActive(true);
                pdfViewer.LoadDocumentFromWeb(url);
            }
        }
        



        
    }



    private void validarCredencialesSketchfab(string urlModelo)
    {
        SketchfabAPI.GetAccessToken(sketchfabEmail, sketchfabPassword, (SketchfabResponse<SketchfabAccessToken> answer) =>
        {
            if (answer.Success)
            {
                SketchfabAPI.AuthorizeWithAccessToken(answer.Object);
                descargarModeloSketchfab(urlModelo);
            }
            else
            {

                Debug.Log("Error en las credenciales de sketchfab:" + answer.ToString()+"\n "+answer.ErrorMessage);
                loadingText.text = "Error en las credenciales de sketchfab:" + answer.ToString();
                loadingImage.gameObject.SetActive(false);
                closeLoadingImage.gameObject.SetActive(true);
            }

        });
    }

    private void descargarModeloSketchfab(string urlModelo)
    {

        model = model3d.gameObject;
        SketchfabAPI.GetModel(urlModelo, (resp) =>
        {
            StartCoroutine(cargandoModeloSketchfab(resp));
            if (resp.Success)
            {
                LoadingPanel.SetActive(false);
            }
            else
            {
                loadingImage.gameObject.SetActive(false);
                loadingText.text = resp.ErrorMessage.ToString();
                LoadingPanel.SetActive(true);
            }


            SketchfabModel modelo = resp.Object;


            SketchfabModelImporter.Import(modelo, (obj) =>
            {
                Debug.Log("cargando ");
                if (obj != null)
                {
                    obj.transform.parent = model.transform;
                    obj.transform.localPosition = new Vector3(0, 0, 0);
                    obj.transform.localEulerAngles = new Vector3(0, 0, 0);
                    //obj.transform.localScale = new Vector3(1, 1, 1);
                   
                    obj.AddComponent<LeanTwistRotate>();
                    obj.AddComponent<LeanDragTranslate>();
                    obj.AddComponent<LeanPinchScale>();



                    escalarX.onValueChanged.RemoveAllListeners();
                    escalarY.onValueChanged.RemoveAllListeners();
                    escalarZ.onValueChanged.RemoveAllListeners();
                    rotarX.onValueChanged.RemoveAllListeners();
                    rotarY.onValueChanged.RemoveAllListeners();
                    rotarZ.onValueChanged.RemoveAllListeners();

                    escalarX.onValueChanged.AddListener(delegate { escalarObjeto(obj); });
                    escalarY.onValueChanged.AddListener(delegate { escalarObjeto(obj); });
                    escalarZ.onValueChanged.AddListener(delegate { escalarObjeto(obj); });
                    rotarX.onValueChanged.AddListener(delegate { rotarObjeto(obj); });
                    rotarY.onValueChanged.AddListener(delegate { rotarObjeto(obj); });
                    rotarZ.onValueChanged.AddListener(delegate { rotarObjeto(obj); });

                }
                else{
                    loadingText.text = "Hubo problemas al cargar el modelo 3D";
                    LoadingPanel.SetActive(true);
                }
            });

           
        });
    }

    private IEnumerator cargandoModeloSketchfab(SketchfabResponse<SketchfabModel> resp)
    {
        while (!resp.Success)
        {
            loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
            loadingText.text = Main.obtenerValor("textLoading") + " ...";
            LoadingPanel.SetActive(false);

            yield return null;
        }
    }


    public void mostrarOcultarSliders(string opcion)
    {
        if (opcion.Equals("escalar"))
        {
            SliderEscalarModelo.SetActive(true);
            SliderRotarModelo.SetActive(false);
        }
        else if (opcion.Equals("rotar"))
        {
            SliderEscalarModelo.SetActive(false);
            SliderRotarModelo.SetActive(true);
        }
    }

    public void pausarContenidos()
    {
        if(AudioSource.clip != null)
        {
            AudioSource.clip = null;
        }

        imageGPS.color = new Color32(255, 255, 255, 255);
        imageGPS.texture = whiteTexture;

        youtubePlayer.Stop();
        youtubePlayer.videoPlayer.Stop();
        DropdownAnimaciones.transform.gameObject.SetActive(false);
        foreach(Transform child in model3d.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in ARCamera.transform)
        {
            if (!child.name.Equals("PositionFor2D") && !child.name.Equals("VideoBackground"))
            {
                GameObject.Destroy(child.gameObject);
            }
            
        }

        TextMeshProUGUI loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        loadingText.text = "";
        textContent.text = "";
        ScrollAreaText.SetActive(false);
        
        ModelOptions.SetActive(false);
        MenuOptions.SetActive(true);
            imageGPS.gameObject.SetActive(true);
            imageGPS.texture = whiteTexture;
            imageGPS.gameObject.SetActive(false);

        pdfViewerObj.SetActive(false);


    }

    public void iniciarAudio(AudioSource audio, GameObject botonPausa, GameObject botonInicio, GameObject botonParar)
    {
        if(!audio.isPlaying && audio.isActiveAndEnabled)
        {
            audio.Play();
            botonPausa.SetActive(true);
            botonInicio.SetActive(false);
            botonParar.SetActive(true);
        }
        
    }

    public void pausarAudio(AudioSource audio, GameObject botonPausa, GameObject botonInicio,GameObject botonParar)
    {
        if (audio.isPlaying)
        {
            audio.Pause();
            botonPausa.SetActive(false);
            botonInicio.SetActive(true);
            botonParar.SetActive(true);
        }
    }

    public void pararAudio(AudioSource audio, GameObject botonPausa, GameObject botonInicio, GameObject botonParar)
    {
            audio.Stop();
            botonPausa.SetActive(false);
            botonInicio.SetActive(true);
            botonParar.SetActive(false);
    }

    public void iniciarVideo(YoutubePlayer video, GameObject botonPausa, GameObject botonInicio, GameObject botonParar, bool youtube)
    {
 
            imageGPS.gameObject.SetActive(true);
            imageGPS.texture = YoutubeTexture;

        if (youtube)
        {
            video.Play();
        }
        else
        {
            video.videoPlayer.Play();
        }
            botonPausa.SetActive(true);
            botonInicio.SetActive(false);
            botonParar.SetActive(true);

    }

    public void pausarVideo(YoutubePlayer video, GameObject botonPausa, GameObject botonInicio, GameObject botonParar, bool youtube)
    {
            imageGPS.gameObject.SetActive(true);
            imageGPS.texture = YoutubeTexture;
        if (youtube)
        {
            video.Pause();
        }
        else
        {
            video.videoPlayer.Pause();
        }
        
            botonPausa.SetActive(false);
            botonInicio.SetActive(true);
            botonParar.SetActive(true);
    }

    public void pararVideo(YoutubePlayer video, GameObject botonPausa, GameObject botonInicio, GameObject botonParar, bool youtube)
    {
            imageGPS.gameObject.SetActive(true);
            imageGPS.texture = YoutubeTexture;
        if (youtube)
        {
            video.Stop();
        }
        else
        {
            video.videoPlayer.Stop();
        }
        
        botonPausa.SetActive(false);
        botonInicio.SetActive(true);
        botonParar.SetActive(false);
    }

    IEnumerator cargarModelo3D(string url)
    {
        yield return new WaitForSeconds(3f);
        cargarModelo(url);
    }

    public void cargarModelo (string url)
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();

        model = model3d.gameObject;

        

        string extension = Path.GetExtension(url);
        if (!extension.Contains("zip"))
        {
            AssetLoader.LoadModelFromFile(url, OnLoad, delegate (AssetLoaderContext assetLoaderContext) {
                var myGameObject = assetLoaderContext.RootGameObject;
                _rootGameObject = myGameObject;
                _rootGameObject.transform.parent = model.transform;
                _rootGameObject.transform.localPosition = new Vector3(0, 0, 0);
                _rootGameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                _rootGameObject.transform.localScale = new Vector3(1, 1, 1);

                _rootGameObject.AddComponent<LeanTwistRotate>();
                _rootGameObject.AddComponent<LeanDragTranslate>();
                _rootGameObject.AddComponent<LeanPinchScale>();

                

                escalarX.onValueChanged.RemoveAllListeners();
                escalarY.onValueChanged.RemoveAllListeners();
                escalarZ.onValueChanged.RemoveAllListeners();
                rotarX.onValueChanged.RemoveAllListeners();
                rotarY.onValueChanged.RemoveAllListeners();
                rotarZ.onValueChanged.RemoveAllListeners();

                escalarX.onValueChanged.AddListener(delegate { escalarObjeto(_rootGameObject); });
                escalarY.onValueChanged.AddListener(delegate { escalarObjeto(_rootGameObject); });
                escalarZ.onValueChanged.AddListener(delegate { escalarObjeto(_rootGameObject); });
                rotarX.onValueChanged.AddListener(delegate { rotarObjeto(_rootGameObject); });
                rotarY.onValueChanged.AddListener(delegate { rotarObjeto(_rootGameObject); });
                rotarZ.onValueChanged.AddListener(delegate { rotarObjeto(_rootGameObject); });

                FullPostLoadSetup();
            }, OnProgress, OnError, null, assetLoaderOptions);
        }
        else
        {
            AssetLoaderZip.LoadModelFromZipFile(url, OnLoad, delegate (AssetLoaderContext assetLoaderContext) {
                var myGameObject = assetLoaderContext.RootGameObject;
                _rootGameObject = myGameObject;
                _rootGameObject.transform.parent = model.transform;
                _rootGameObject.transform.localPosition = new Vector3(0, 0, 0);
                _rootGameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                _rootGameObject.transform.localScale = new Vector3(1, 1, 1);
                _rootGameObject.AddComponent<LeanTwistRotate>();
                _rootGameObject.AddComponent<LeanDragTranslate>();
                _rootGameObject.AddComponent<LeanPinchScale>();

                escalarX.onValueChanged.RemoveAllListeners();
                escalarY.onValueChanged.RemoveAllListeners();
                escalarZ.onValueChanged.RemoveAllListeners();
                rotarX.onValueChanged.RemoveAllListeners();
                rotarY.onValueChanged.RemoveAllListeners();
                rotarZ.onValueChanged.RemoveAllListeners();

                escalarX.onValueChanged.AddListener(delegate { escalarObjeto(_rootGameObject); });
                escalarY.onValueChanged.AddListener(delegate { escalarObjeto(_rootGameObject); });
                escalarZ.onValueChanged.AddListener(delegate { escalarObjeto(_rootGameObject); });
                rotarX.onValueChanged.AddListener(delegate { rotarObjeto(_rootGameObject); });
                rotarY.onValueChanged.AddListener(delegate { rotarObjeto(_rootGameObject); });
                rotarZ.onValueChanged.AddListener(delegate { rotarObjeto(_rootGameObject); });
                FullPostLoadSetup();
            }, OnProgress, OnError, null, assetLoaderOptions);
        }
    }

    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        RawImage loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        TextMeshProUGUI loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

        loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
        loadingText.text = Main.obtenerValor("textLoading")+" ...";
        LoadingPanel.SetActive(false);
    }

    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        RawImage loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        TextMeshProUGUI loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        RawImage closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();

        loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
        loadingText.text = Main.obtenerValor("textLoading")+$": {progress:P}";
        if(progress == 100)
        {
            LoadingPanel.SetActive(false);
        }
    }

    private void OnError(IContextualizedError obj)
    {
        RawImage loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        TextMeshProUGUI loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        RawImage closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();

        loadingImage.gameObject.SetActive(false);
        closeLoadingImage.gameObject.SetActive(true);
        loadingText.text = Main.obtenerValor("errorDownload")+$": {obj.GetInnerException()}";

    }

    private void FullPostLoadSetup()
    {
        if (_rootGameObject != null)
        {
            PostLoadSetup();
        }
    }

    private void PostLoadSetup()
    {
        var rootAnimation = _rootGameObject.GetComponent<Animation>();
        var skinnedMeshRenderers = _rootGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderers != null)
        {
            var hasBlendShapes = false;
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (!hasBlendShapes && skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    hasBlendShapes = true;
                }
            }
        }
        if (rootAnimation != null)
        {
            
            DropdownAnimaciones.transform.gameObject.SetActive(true);
            List<string> animations = new List<string>();
            animations.Add(Main.obtenerValor("labelSeleccionar")+" "+Main.obtenerValor("labelAnimacion"));
            
            foreach (AnimationState animationState in rootAnimation)
            {

                animations.Add(animationState.name);

            }

            if (animations.Count > 1)
            {
                DropdownAnimaciones.AddOptions(animations);
                string animacion = animations[DropdownAnimaciones.value];
                DropdownAnimaciones.onValueChanged.AddListener(delegate { ejecutarAnimacion(animacion); });
            }
            else
            {
                DropdownAnimaciones.transform.gameObject.SetActive(false);
            }
            
        }
    }

    public void ejecutarAnimacion(string animacion)
    {
        _rootGameObject.GetComponent<Animation>().Play(animacion);
    }

    public void escalarObjeto(GameObject objeto)
    {
        objeto.transform.localScale = new Vector3(escalarX.value, escalarY.value, escalarZ.value);
    }

    public void rotarObjeto(GameObject objeto)
    {
        objeto.transform.localRotation = Quaternion.Euler(rotarX.value, rotarY.value, rotarZ.value);
    }

    public void mostrarContenidos()
    {
        pausarContenidos();
        MenuOptions.SetActive(true);
        PlayOptions.SetActive(false);
    }


    void EscalarObjeto(float factorDeEscala)
    {
        // Calcula la nueva escala multiplicando la escala inicial por el factor de escala
        Vector3 nuevaEscala = escalaInicial * factorDeEscala;

        // Aplica la nueva escala al objeto tridimensional con una transición suave
        StartCoroutine(EscalarConTransicion(transform.localScale, nuevaEscala));
    }

    IEnumerator EscalarConTransicion(Vector3 escalaInicial, Vector3 escalaFinal)
    {
        float tiempoPasado = 0f;

        while (tiempoPasado < 1f)
        {
            tiempoPasado += Time.deltaTime * velocidadDeEscalado;

            // Interpola suavemente entre la escala inicial y final
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, tiempoPasado);

            yield return null;
        }
    }
}

