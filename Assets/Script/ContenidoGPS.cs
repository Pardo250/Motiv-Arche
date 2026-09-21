using ARLocation;
using ARLocation.MapboxRoutes;
using Lean.Touch;
using LightShaft.Scripts;
using Paroxe.PdfRenderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static ARLocation.MapboxRoutes.Examples.Search.MenuController;
using static CloudContentManager;
using static OnlineMapsGoogleDirections;

public class ContenidoGPS : MonoBehaviour
{
    public static int posicion = 0;
    public static int totalElementosRuta = 0;
    public string MapboxToken = "pk.eyJ1IjoiZG1iZm0iLCJhIjoiY2tyYW9hdGMwNGt6dTJ2bzhieDg3NGJxNyJ9.qaQsMUbyu4iARFe0XB2SWg";
    public string googleMapsSinPoi = "https://maps.googleapis.com/maps/vt?pb=!1m5!1m4!1i{zoom}!2i{x}!3i{y}!4i256!2m3!1e0!2sm!3i610341528!3m17!2ses-US!3sUS!5e18!12m4!1e68!2m2!1sset!2sRoadmap!12m3!1e37!2m1!1ssmartmaps!12m4!1e26!2m2!1sstyles!2zcC52Om9uLHMudDoxfHAudjpvbixzLnQ6NXxwLnY6b24scy50OjJ8cC52Om9mZg!4e0";
    public string googleMapsConPoi = "https://maps.googleapis.com/maps/vt?pb=!1m5!1m4!1i{zoom}!2i{x}!3i{y}!4i256!2m3!1e0!2sm!3i610341528!3m17!2ses-US!3sUS!5e18!12m4!1e68!2m2!1sset!2sRoadmap!12m3!1e37!2m1!1ssmartmaps!12m4!1e26!2m2!1sstyles!2zcC52Om9uLHMudDoxfHAudjpvbixzLnQ6NXxwLnY6b24scy50OjJ8cC52Om9u!4e0";
    public GameObject MenuOptions;
    public GameObject PlayOptions;
    public TMP_Dropdown DropdownElementos;
    public TMP_Dropdown DropdownAnimaciones;
    public TMP_Dropdown DropdownRutas;
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
    public string googleAPIKey;

    public GameObject GPSContentPanel;
    public Camera ARCamera;
    public Toggle googleMapsPoiToggle;


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



    //Devuelve false si el reconocimiento es por gps y true si es por imagen
    bool gpsImageRecognition = false;

    private ARLocationProvider locationProvider;
    private OnlineMapsMarker playerMarker;
    private List<OnlineMapsMarker> placesMarker;
    private List<OnlineMapsMarker> rutaMarker;
    OnlineMapsLocationService locationService;
    Texture2D redPoi;
    public Texture2D playerMarkerImage;

    public GameObject prefabStar;
    public GameObject map;

    private GameObject tooltip;
    public RawImage imageGPS;

    public GameObject parentGPSObjects;

    List<string> descripciones = new List<string>();
    ListaRuta rutas = new ListaRuta();
    ListaOrdenRuta ordenRuta = new ListaOrdenRuta();
    List<Vector2> waypoints = new List<Vector2>();
    List<string> waypointsName = new List<string>();
    List<GameObject> stars = new List<GameObject>();
    Vector2 origin;
    Vector2 destino;
    string transporte;

    public PDFViewer pdfViewer;
    public GameObject pdfViewerObj;
    public GameObject MapboxRoute;
    public GameObject botonRutaAnterior;
    public GameObject botonRutaSiguiente;
    public AbstractRouteRenderer RoutePathRenderer;
    public AbstractRouteRenderer NextTargetPathRenderer;

    public MapboxRoute MapboxRoute2;
    public TextMeshProUGUI poiNameSelected;
    public GameObject routeOptions;
    public TextMeshProUGUI latLongUser;

    public Slider radioActivacionSlider;
    public TextMeshProUGUI radioTextSelected;
    private int radioActivacion = 1;

    public TMP_Dropdown activacionAR;


    RawImage loadingImage;
    TextMeshProUGUI loadingText;
    RawImage closeLoadingImage;

    TextMeshProUGUI textContent;

    Location destinoMapbox;
    private State s = new State();

    private List<RutaSeleccionada> puntosRutaSeleccionados = new List<RutaSeleccionada>();

    public enum LineType
    {
        Route,
        NextTarget
    }

    enum View
    {
        SearchMenu,
        Route,
    }

    string sketchfabEmail;
    string sketchfabPassword;


    [System.Serializable]
    private class State
    {
        public string QueryText = "";
        public List<GeocodingFeature> Results = new List<GeocodingFeature>();
        public View View = View.SearchMenu;
        public Location destination;
        public LineType LineType = LineType.NextTarget;
        public string ErrorMessage;
    }

    [System.Serializable]
    public class ListaRuta
    {
        public List<Ruta> rutas;
    }

    [System.Serializable]
    public class Ruta
    {
        public int id;
        public string nombre;
        public bool optimizar;
        public string transporte;
        public string creador;
    }

    [System.Serializable]
    public class ListaOrdenRuta
    {
        public List<OrdenRuta> ordenRutas;
    }

    [System.Serializable]
    public class OrdenRuta
    {
        public int id;
        public float ordenRuta;
        public string latLng;
        public string nombrePunto;
        public bool incluirPuntoRuta;

    }

    public void Awake()
    {
        locationProvider = ARLocationProvider.Instance;
    }

    public void Update()
    {
        /*if (playerMarker != null)
        {
            latLongUser.text = ARLocationProvider.Instance.CurrentLocation.latitude + " " + ARLocationProvider.Instance.CurrentLocation.longitude;
        }*/
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
                string[] credenciales = response.Split("|");
                sketchfabEmail = credenciales[0];
                sketchfabPassword = credenciales[1];
            }
        }
    }

    public void Start()
    {
        Main.contenidosHotspot.Clear();
        posicion = 0;
        totalElementosRuta = 0;
        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
        textContent = ScrollAreaText.transform.GetChild(0).gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (string.IsNullOrEmpty(googleAPIKey)) Debug.LogWarning("Please specify Google API Key");
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


        googleMapsPoiToggle.onValueChanged.AddListener(delegate { ActivarDesactivarPoiGoogle(googleMapsPoiToggle); });
        radioActivacionSlider.onValueChanged.AddListener(delegate { CambiarRadioActivacion(radioActivacionSlider); });

        activacionAR.onValueChanged.AddListener(delegate { ActivarHotspotsSeleccionados(activacionAR); });

        placesMarker = new List<OnlineMapsMarker>();
        rutaMarker = new List<OnlineMapsMarker>();
        redPoi = Resources.Load<Texture2D>("redMarker");
        // Create a new marker.


        // Get instance of LocationService.
        locationService = OnlineMapsLocationService.instance;



        if (locationService == null)
        {
            LoadingPanel.SetActive(true);
            loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
            loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
            ScrollAreaText.SetActive(false);
            loadingText.text = "Location Service not found.\nAdd Location Service Component (Component / Infinity Code / Online Maps / Plugins / Location Service).";
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
            Debug.LogError(
                "Location Service not found.\nAdd Location Service Component (Component / Infinity Code / Online Maps / Plugins / Location Service).");
            return;
        }

        locationService.OnLocationInited = createPlayerMarker;

        // Subscribe to the change location event.
        locationService.OnLocationChanged += OnLocationChanged;
        OnlineMapsLocationService.instance.OnCompassChanged += OnCompassChanged;

        descargarPosicionGPS();
        StartCoroutine(obtenerCredencialesSketchfab());




    }

    public void ActivarDesactivarPoiGoogle(Toggle googleToogle)
    {
        if (googleToogle.isOn)
        {
            OnlineMaps.instance.customProviderURL = googleMapsConPoi;

        }
        else
        {
            OnlineMaps.instance.customProviderURL = googleMapsSinPoi;
        }
        OnlineMaps.instance.RedrawImmediately();
    }



    public void CambiarRadioActivacion(Slider radioActivacion)
    {
        this.radioActivacion = (int)radioActivacion.value;
        radioTextSelected.text = "Radio activacion " + this.radioActivacion + " m";
        if (parentGPSObjects.transform.childCount > 0)
        {
            foreach (Transform child in parentGPSObjects.transform)
            {
                if (child.gameObject != null)
                {
                    child.gameObject.GetComponent<Hotspot>().HotspotSettings.ActivationRadius = this.radioActivacion;
                    child.gameObject.GetComponent<Hotspot>().HotspotSettings.DeactivationRadius = this.radioActivacion + 5;
                }
            }
        }
    }



    public void ActivarHotspotsSeleccionados(TMP_Dropdown activacionAR)
    {

        if (activacionAR.value == 0)
        {
            if (parentGPSObjects.transform.childCount > 0)
            {
                foreach (Transform child in parentGPSObjects.transform)
                {
                    if (child.gameObject != null)
                        child.gameObject.SetActive(false);
                }
            }

            foreach (RutaSeleccionada a in puntosRutaSeleccionados)
            {
                ActivarDesactivarHotspot(a.GetNombrePunto());
            }

            //si es solo los de la ruta
            radioActivacionSlider.gameObject.SetActive(true);
        }
        else if (activacionAR.value == 1)
        {
            //desactivar todos los hotspots

            if (parentGPSObjects.transform.childCount > 0)
            {
                foreach (Transform child in parentGPSObjects.transform)
                {
                    if (child.gameObject != null)
                        child.gameObject.SetActive(false);
                }
            }

            radioActivacionSlider.gameObject.SetActive(false);
        }
        else if (activacionAR.value == 2)
        {
            //activar todos los hotspots

            if (parentGPSObjects.transform.childCount > 0)
            {
                foreach (Transform child in parentGPSObjects.transform)
                {
                    if (child.gameObject != null)
                        child.gameObject.SetActive(true);
                }
            }

            radioActivacionSlider.gameObject.SetActive(true);
        }
    }

    public void cargarContenidos(string poiName, bool imageRecognition)
    {
        reiniciarContenidos();
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


        if (contents)
        {
            Debug.Log("si tiene contenidos");
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
                Debug.Log(webs);
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
            //MenuOptions.SetActive(false);
            botonAudio.SetActive(false);
            botonImagen.SetActive(false);
            botonVideo.SetActive(false);
            botonModelo.SetActive(false);
            botonWeb.SetActive(false);
            botonTexto.SetActive(false);
            botonPdf.SetActive(false);
            PlayOptions.SetActive(false);
        }

        for (int i = 0; i < puntosRutaSeleccionados.Count; i++)
        {
            if (puntosRutaSeleccionados[i].GetNombrePunto().Trim().Equals(poiName))
            {
                posicion = i;
                poiNameSelected.text = puntosRutaSeleccionados[i].GetOrdenRuta() + " " + poiName;

            }
        }


        botonRutaAnterior.SetActive(true);
        botonRutaSiguiente.SetActive(true);
        if (posicion == 0)
        {
            botonRutaAnterior.SetActive(false);
        }

        if (posicion == puntosRutaSeleccionados.Count - 1)
        {
            botonRutaSiguiente.SetActive(false);
        }


    }

    public void hacerNuevoReconocimiento()
    {
        pausarContenidos();
        MenuOptions.SetActive(false);

        DropdownElementos.ClearOptions();
        DropdownAnimaciones.ClearOptions();
        DropdownElementos.transform.gameObject.SetActive(false);
        DropdownAnimaciones.transform.gameObject.SetActive(false);

    }


    public void reiniciarContenidos()
    {
        pausarContenidos();
        DropdownElementos.ClearOptions();
        DropdownAnimaciones.ClearOptions();
        DropdownElementos.transform.gameObject.SetActive(false);
        DropdownAnimaciones.transform.gameObject.SetActive(false);

    }

    private void agregarContenidosDropdown(List<string> contenidos, string tipoContenido, string poiName)
    {
        DropdownElementos.transform.gameObject.SetActive(true);
        DropdownElementos.ClearOptions();

        //TODO aqui es donde debo buscar la descripcion de cada uno de los contenidos
        List<string> tipoContenidos = new List<string>();

        //TODO toca aqui cambiar para no poner solo video web, etc
        StartCoroutine(consultarDescripcionContenidos(tipoContenido, poiName, tipoContenidos, contenidos));




    }

    IEnumerator consultarDescripcionContenidos(string tipoContenido, string nombrePunto, List<string> tipoContenidos, List<string> contenidos)
    {
        descripciones.Clear();
        WWWForm form = new WWWForm();
        form.AddField("obtenerDescripcionContenidosNombre", "consulta");
        form.AddField("name", nombrePunto);
        form.AddField("tipo", tipoContenido);
        Debug.Log("nombre punto:" + nombrePunto);
        Debug.Log("tipo contenido:" + tipoContenido);

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
            Debug.Log(results);
            string[] resultadoDes = results.Split("|");
            descripciones.Add("Seleccionar " + tipoContenido);
            for (int i = 0; i < resultadoDes.Length; i++)
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
        if (index != 0)
        {
            string url = contenidos[index - 1];
            Debug.Log("url del contenido:" + url);
            string urlWeb = url;

            if (url.Contains("https"))
            {
                url = url.Replace("https", "http");
            }


            UnityWebRequest www;
            LoadingPanel.SetActive(true);

            loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
            loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
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
                    loadingText.text = Main.obtenerValor("textLoading") + " ...";
                }

                if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(Main.obtenerValor("errorDownload") + ":" + www.error);
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

                    botonPlay.onClick.AddListener(() => iniciarAudio(AudioSource, botonPausa, botonInicio, botonParar));
                    botonPause.onClick.AddListener(() => pausarAudio(AudioSource, botonPausa, botonInicio, botonParar));
                    botonStop.onClick.AddListener(() => pararAudio(AudioSource, botonPausa, botonInicio, botonParar));
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

                    botonPlay.onClick.AddListener(() => iniciarVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube, ""));
                    botonPause.onClick.AddListener(() => pausarVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube));
                    botonStop.onClick.AddListener(() => pararVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube));
                    botonClose.onClick.AddListener(() => mostrarContenidos());
                }
                else
                {

                    //youtubePlayer.videoPlayer.url = url;
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

                    botonPlay.onClick.AddListener(() => iniciarVideo(youtubePlayer, botonPausa, botonInicio, botonParar, youtube, url));
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
                    loadingText.text = Main.obtenerValor("textLoading") + " ...";
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

                Debug.Log("Error en las credenciales de sketchfab:" + answer.ToString() + "\n " + answer.ErrorMessage);
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
            SketchfabModelImporter.Import(resp.Object, (obj) =>
            {
                if (obj != null)
                {
                    obj.transform.parent = ARCamera.transform;
                    obj.transform.localPosition = new Vector3(0, 0, 1200);
                    obj.transform.parent = model.transform;
                    obj.transform.localEulerAngles = new Vector3(0, 0, 0);
                    //obj.transform.localScale = new Vector3(10, 10, 10);


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
                else
                {
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
        pdfViewerObj.SetActive(false);

        if (AudioSource.clip != null)
        {
            AudioSource.clip = null;
        }


        imageGPS.color = new Color32(255, 255, 255, 255);
        imageGPS.texture = whiteTexture;

        youtubePlayer.Stop();
        youtubePlayer.videoPlayer.Stop();
        DropdownAnimaciones.transform.gameObject.SetActive(false);
        foreach (Transform child in model3d.transform)
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

        loadingText.text = "";
        textContent.text = "";
        ScrollAreaText.SetActive(false);
        ModelOptions.SetActive(false);
        MenuOptions.SetActive(true);

        //PanelAugmented.GetComponent<RawImage>().texture = YoutubeTexture;
        imageGPS.gameObject.SetActive(true);
        imageGPS.texture = whiteTexture;
        imageGPS.gameObject.SetActive(false);




    }

    public void iniciarAudio(AudioSource audio, GameObject botonPausa, GameObject botonInicio, GameObject botonParar)
    {
        if (!audio.isPlaying && audio.isActiveAndEnabled)
        {
            audio.Play();
            botonPausa.SetActive(true);
            botonInicio.SetActive(false);
            botonParar.SetActive(true);
        }

    }

    public void pausarAudio(AudioSource audio, GameObject botonPausa, GameObject botonInicio, GameObject botonParar)
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

    public void iniciarVideo(YoutubePlayer video, GameObject botonPausa, GameObject botonInicio, GameObject botonParar, bool youtube, string url)
    {

        imageGPS.gameObject.SetActive(true);
        imageGPS.texture = YoutubeTexture;

        if (youtube)
        {
            video.debug = true;
            video.Play();
        }
        else
        {
            video.videoPlayer.errorReceived += ErrorVideo;
            DownloadVideo(url, video.videoPlayer);
        }
        botonPausa.SetActive(true);
        botonInicio.SetActive(false);
        botonParar.SetActive(true);




    }

    public void ErrorVideo(VideoPlayer source, string message)
    {
        LoadingPanel.SetActive(true);
        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
        loadingText.text = message;
        loadingImage.gameObject.SetActive(false);
        closeLoadingImage.gameObject.SetActive(true);
        source.errorReceived -= ErrorVideo;
    }

    public void DownloadVideo(string url, VideoPlayer source)
    {
        Debug.Log("descargando video");
        Debug.Log(url);
        source.source = VideoSource.Url;
        source.playOnAwake = false;
        source.url = url;
        source.Play();
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

    public void cargarModelo(string url)
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();

        model = model3d.gameObject;



        string extension = Path.GetExtension(url);
        if (!extension.Contains("zip"))
        {
            AssetLoader.LoadModelFromFile(url, OnLoad, delegate (AssetLoaderContext assetLoaderContext) {
                var myGameObject = assetLoaderContext.RootGameObject;
                _rootGameObject = myGameObject;
                _rootGameObject.transform.parent = ARCamera.transform;
                _rootGameObject.transform.localPosition = new Vector3(0, 0, 1200);
                _rootGameObject.transform.parent = model.transform;
                _rootGameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                _rootGameObject.transform.localScale = new Vector3(10, 10, 10);


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
                _rootGameObject.transform.parent = ARCamera.transform;
                _rootGameObject.transform.localPosition = new Vector3(0, 0, 1200);

                _rootGameObject.transform.parent = model.transform;

                _rootGameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                _rootGameObject.transform.localScale = new Vector3(10, 10, 10);

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

        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

        loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
        loadingText.text = Main.obtenerValor("textLoading") + " ...";
        LoadingPanel.SetActive(false);
    }

    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();

        loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
        loadingText.text = Main.obtenerValor("textLoading") + $": {progress:P}";
        if (progress == 100)
        {
            LoadingPanel.SetActive(false);
        }
    }

    private void OnError(IContextualizedError obj)
    {
        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();

        loadingImage.gameObject.SetActive(false);
        closeLoadingImage.gameObject.SetActive(true);
        loadingText.text = Main.obtenerValor("errorDownload") + $": {obj.GetInnerException()}";

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
            animations.Add(Main.obtenerValor("labelSeleccionar") + " " + Main.obtenerValor("labelAnimacion"));

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





    //-----------------------------------------------------------------------




    public void descargarPosicionGPS()
    {
        StartCoroutine(descargarPosicionesGeograficas());
        StartCoroutine(obtenerRutas());

    }

    public IEnumerator descargarPosicionesGeograficas()
    {

        WWWForm formphp = new WWWForm();
        formphp.AddField("obtenerGPSContenidos", "obtenerGPSContenidos");
        UnityWebRequest www = UnityWebRequest.Post(Main.dominio + "/UnitySQL.php", formphp);
        www.SendWebRequest();
        while (!www.downloadHandler.isDone)
        {

            yield return null;
        }

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string response = www.downloadHandler.text;
            Debug.Log("RESPUESTA:" + response);
            if (!response.Trim().Equals(""))
            {
                ListaGPSContenido posGPS = JsonUtility.FromJson<ListaGPSContenido>("{\"gps\":" + www.downloadHandler.text + "}");
                List<PosicionGeograficaContenido> resultado = posGPS.gps;


                if (resultado.Count == 1)
                {
                    GPSContenidos a = new GPSContenidos();
                    a.name = resultado[0].name;
                    a.latitud = resultado[0].latitud;
                    a.longitud = resultado[0].longitud;

                    a.audios = new List<ContenidoInfo>();
                    a.imagenes = new List<ContenidoInfo>();
                    a.videos = new List<ContenidoInfo>();
                    a.modelos = new List<ContenidoInfo>();
                    a.webs = new List<ContenidoInfo>();
                    a.textos = new List<ContenidoInfo>();
                    a.pdfs = new List<ContenidoInfo>();

                    if (resultado[0].tipo_contenido.Equals("audio"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.audios.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("imagen"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.imagenes.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("video"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.videos.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("modelo"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.modelos.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("web"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.webs.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("texto"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.textos.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("pdf"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.pdfs.Add(c);
                    }

                    Main.contenidosHotspot.Add(a);
                    crearHotspot(a.name, a.latitud, a.longitud);

                }
                else
                {

                    GPSContenidos a = new GPSContenidos();
                    a.name = resultado[0].name;
                    a.latitud = resultado[0].latitud;
                    a.longitud = resultado[0].longitud;

                    a.audios = new List<ContenidoInfo>();
                    a.imagenes = new List<ContenidoInfo>();
                    a.videos = new List<ContenidoInfo>();
                    a.modelos = new List<ContenidoInfo>();
                    a.webs = new List<ContenidoInfo>();
                    a.textos = new List<ContenidoInfo>();
                    a.pdfs = new List<ContenidoInfo>();

                    if (resultado[0].tipo_contenido.Equals("audio"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.audios.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("imagen"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.imagenes.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("video"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.videos.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("modelo"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.modelos.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("web"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.webs.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("texto"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.textos.Add(c);
                    }
                    else if (resultado[0].tipo_contenido.Equals("pdf"))
                    {
                        ContenidoInfo c = new ContenidoInfo();
                        c.url = resultado[0].url;
                        a.pdfs.Add(c);
                    }

                    for (int i = 1; i < resultado.Count; i++)
                    {
                        if (resultado[i - 1].name.Equals(resultado[i].name) && resultado[i - 1].latitud == resultado[i].latitud && resultado[i - 1].longitud == resultado[i].longitud)
                        {

                            if (resultado[i].tipo_contenido.Equals("audio"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.audios.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("imagen"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.imagenes.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("video"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.videos.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("modelo"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.modelos.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("web"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.webs.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("texto"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.textos.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("pdf"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.pdfs.Add(c);
                            }

                            if (resultado.Count == (i + 1))
                            {


                                Main.contenidosHotspot.Add(a);
                                crearHotspot(a.name, a.latitud, a.longitud);
                            }
                        }
                        else
                        {

                            Main.contenidosHotspot.Add(a);
                            crearHotspot(a.name, a.latitud, a.longitud);
                            a = new GPSContenidos();
                            a.name = resultado[i].name;
                            a.latitud = resultado[i].latitud;
                            a.longitud = resultado[i].longitud;

                            a.audios = new List<ContenidoInfo>();
                            a.imagenes = new List<ContenidoInfo>();
                            a.videos = new List<ContenidoInfo>();
                            a.modelos = new List<ContenidoInfo>();
                            a.webs = new List<ContenidoInfo>();
                            a.textos = new List<ContenidoInfo>();
                            a.pdfs = new List<ContenidoInfo>();


                            if (resultado[i].tipo_contenido.Equals("audio"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.audios.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("imagen"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.imagenes.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("video"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.videos.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("modelo"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.modelos.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("web"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.webs.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("texto"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.textos.Add(c);
                            }
                            else if (resultado[i].tipo_contenido.Equals("pdf"))
                            {
                                ContenidoInfo c = new ContenidoInfo();
                                c.url = resultado[i].url;
                                a.pdfs.Add(c);
                            }

                            if (resultado.Count == (i + 1))
                            {

                                Main.contenidosHotspot.Add(a);
                                crearHotspot(a.name, a.latitud, a.longitud);
                            }

                        }


                    }
                }
            }
        }

    }

    public void eliminaPuntosARGPS()
    {

        if (parentGPSObjects.transform.childCount > 0)
        {
            foreach (Transform child in parentGPSObjects.transform)
            {
                if (child.gameObject != null)
                    child.gameObject.SetActive(false);
            }
        }

        if (placesMarker.Count > 0)
        {
            foreach (OnlineMapsMarker marker in placesMarker)
            {
                if (marker != null)
                    OnlineMapsMarkerManager.RemoveItem(marker, true);
            }

            placesMarker.Clear();

            // Redraw map.
            // OnlineMaps.instance.Redraw();
        }
    }


    private void OnLocationChanged(Vector2 position)
    {
        playerMarker.position = position;
        OnlineMaps.instance.Redraw();
    }

    private void createPlayerMarker()
    {

        playerMarker = OnlineMapsMarkerManager.CreateItem(OnlineMapsLocationService.instance.position, playerMarkerImage, "usuario");
        OnlineMaps.instance.position = playerMarker.position;
        playerMarker.scale = 1.5f;

    }

    /// <summary>
    /// This method is called when the compass value is changed.
    /// </summary>
    /// <param name="f">New compass value (0-1)</param>
    private void OnCompassChanged(float f)
    {
        playerMarker.rotation = f;

        //map.transform.rotation = Quaternion.Euler(0, 0, (f * 360));

    }

    private void OnDrawTooltip(OnlineMapsMarkerBase marker)
    {
        Debug.Log(marker.label);
        Debug.Log(marker.position.x);
        Debug.Log(marker.position.y);

        MenuOptions.SetActive(true);
        GameObject botonAudio = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        GameObject botonImagen = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;
        GameObject botonVideo = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject;
        GameObject botonModelo = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject;
        GameObject botonWeb = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(4).gameObject;
        GameObject botonTexto = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(5).gameObject;
        GameObject botonPdf = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(6).gameObject;
        GameObject botonCerrar = MenuOptions.transform.GetChild(0).gameObject.transform.GetChild(7).gameObject;

        routeOptions.SetActive(true);
        botonRutaAnterior.SetActive(false);
        botonRutaSiguiente.SetActive(false);
        botonAudio.SetActive(false);
        botonImagen.SetActive(false);
        botonVideo.SetActive(false);
        botonModelo.SetActive(false);
        botonWeb.SetActive(false);
        botonTexto.SetActive(false);
        botonPdf.SetActive(false);
        botonCerrar.SetActive(true);

        // Here you draw the tooltip for the marker.
        contents = false;
        gpsImageRecognition = false;
        GPSContentPanel.SetActive(true);
        string poiName = marker.label.Trim();
        obtenerContenidosGPS(poiName);
        poiNameSelected.text = poiName;

        for (int i = 0; i < puntosRutaSeleccionados.Count; i++)
        {
            if (puntosRutaSeleccionados[i].GetNombrePunto().Trim().Equals(poiName))
            {
                posicion = i;
                poiNameSelected.text = puntosRutaSeleccionados[i].GetOrdenRuta() + " " + poiName;

            }
        }

        botonRutaAnterior.SetActive(true);
        botonRutaSiguiente.SetActive(true);
        if (posicion == 0)
        {
            botonRutaAnterior.SetActive(false);
        }
        if (posicion == puntosRutaSeleccionados.Count - 1)
        {
            botonRutaSiguiente.SetActive(false);
        }

        if (calcularRutaPunto(poiName))
        {
            calcularRutaMapbox(marker.position.x, marker.position.y, poiName);
        }

    }


    public void obtenerContenidosGPS(string poiName)
    {
        reiniciarContenidos();
        Debug.Log("obtener contenidos de:" + poiName);
        GPSContentPanel.SetActive(false);
        audios.Clear();
        imagenes.Clear();
        videos.Clear();
        modelos.Clear();
        webs.Clear();
        textos.Clear();
        pdfs.Clear();
        contents = false;
        foreach (GPSContenidos a in Main.contenidosHotspot)
        {
            if (a.name.Equals(poiName))
            {


                foreach (ContenidoInfo c in a.audios)
                {
                    contents = true;
                    audios.Add(c.url);
                }

                foreach (ContenidoInfo c in a.imagenes)
                {
                    contents = true;
                    imagenes.Add(c.url);
                }

                foreach (ContenidoInfo c in a.videos)
                {
                    contents = true;
                    videos.Add(c.url);
                }

                foreach (ContenidoInfo c in a.modelos)
                {
                    contents = true;
                    modelos.Add(c.url);
                }

                foreach (ContenidoInfo c in a.webs)
                {
                    contents = true;
                    webs.Add(c.url);
                }

                foreach (ContenidoInfo c in a.textos)
                {
                    contents = true;
                    textos.Add(c.url);
                }
                foreach (ContenidoInfo c in a.pdfs)
                {
                    contents = true;
                    pdfs.Add(c.url);
                }


            }
        }

        cargarContenidos(poiName, false);
    }


    public void dibujarRutaGoogle()
    {
        if (stars != null && stars.Count > 0)
        {
            foreach (GameObject a in stars)
            {
                a.Destroy();
            }
            stars.Clear();
        }

        if (playerMarker == null)
        {
            locationService.OnLocationInited = createPlayerMarker;
        }
        posicion = 0;
        totalElementosRuta = 0;

        if (placesMarker != null && placesMarker.Count > 0)
        {
            foreach (OnlineMapsMarker marker in placesMarker)
            {
                if (marker != null)
                    OnlineMapsMarkerManager.RemoveItem(marker, true);
            }
            placesMarker.Clear();
        }


        if (parentGPSObjects.transform.childCount > 0)
        {
            //si la activacion es todos, no desactivar los hotspoot (el 2 hace referencia a que los hotspot estan siempre activos)
            if (activacionAR.value != 2)
            {
                foreach (Transform child in parentGPSObjects.transform)
                {
                    if (child.gameObject != null)
                        child.gameObject.SetActive(false);
                }
            }

        }

        botonRutaAnterior.SetActive(false);
        if (map.GetComponent<RawImage>().color.a == 1)
        {
            botonRutaSiguiente.SetActive(false);

        }
        else
        {
            //botonRutaSiguiente.SetActive(true);
        }
        routeOptions.SetActive(true);
        poiNameSelected.gameObject.SetActive(true);
        botonRutaSiguiente.SetActive(true);

        if (rutaMarker.Count > 0)
        {
            foreach (OnlineMapsMarker marker in rutaMarker)
            {
                if (marker != null)
                    OnlineMapsMarkerManager.RemoveItem(marker, true);
            }

            rutaMarker.Clear();
        }

        if (DropdownRutas.value > 0)
        {
            Params mapPar = new Params(origin, destino);
            if (transporte.Equals("WALKING"))
            {
                mapPar.mode = Mode.walking;
            }
            else
            {
                mapPar.mode = Mode.driving;
            }

            mapPar.key = googleAPIKey;
            mapPar.waypoints = waypoints;
            Debug.Log(waypoints.ToString());
            OnlineMapsGoogleDirections request = OnlineMapsGoogleDirections.Find(mapPar);
            request.OnComplete += OnGoogleDirectionsComplete;
            request.Send();
            float zoom = 18;
            while (!OnlineMaps.instance.InMapView(origin.x, origin.y) && !OnlineMaps.instance.InMapView(origin.x, origin.y))
            {
                OnlineMaps.instance.SetPositionAndZoom((origin.x + destino.x) / 2, (origin.y + destino.y) / 2, zoom);
                zoom = zoom - 0.5f;
            }

            string poiTexture;
            OnlineMapsMarker poiMarker;

            String originName = "";
            String destinoName = "";

            poiNameSelected.text = puntosRutaSeleccionados[0].GetOrdenRuta() + " " + puntosRutaSeleccionados[0].GetNombrePunto();
            for (int i = 0; i < puntosRutaSeleccionados.Count; i++)
            {

                ActivarDesactivarHotspot(puntosRutaSeleccionados[i].GetNombrePunto());
                //crearHotspot(puntosRutaSeleccionados[i].GetNombrePunto());
                if (puntosRutaSeleccionados[i].GetIncluirPuntoRuta())
                {
                    if (originName.Equals("") && (puntosRutaSeleccionados[i].GetOrdenRuta()) % 1 == 0)
                    {
                        originName = puntosRutaSeleccionados[i].GetNombrePunto();
                    }

                    if ((puntosRutaSeleccionados[i].GetOrdenRuta()) % 1 == 0)
                    {
                        destinoName = puntosRutaSeleccionados[i].GetNombrePunto();
                    }

                    poiMarker = OnlineMapsMarkerManager.CreateItem(puntosRutaSeleccionados[i].GetLat(), puntosRutaSeleccionados[i].GetLng(), redPoi, puntosRutaSeleccionados[i].GetNombrePunto());
                    poiMarker.scale = 1.5f;
                    poiMarker.OnDrawTooltip = OnDrawTooltip;
                    placesMarker.Add(poiMarker);
                    poiTexture = "poi" + (int)puntosRutaSeleccionados[i].GetOrdenRuta();
                    OnlineMapsMarker marker = OnlineMapsMarkerManager.CreateItem(puntosRutaSeleccionados[i].GetLat(), puntosRutaSeleccionados[i].GetLng(), Resources.Load<Texture2D>(poiTexture), puntosRutaSeleccionados[i].GetNombrePunto());
                    marker.scale = 1.5f;
                    rutaMarker.Add(marker);
                }

            }


            ARLocationManager.Instance.Restart();

            if (waypoints.Count > 0)
            {
                Debug.Log("latitud primer punto: " + origin.x + " longitud primer punto: " + origin.y);
                calcularRutaMapbox(origin.x, origin.y, originName);
                Debug.Log("nombre del primer punto>" + originName);
                obtenerContenidosGPS(originName);
            }


            totalElementosRuta = puntosRutaSeleccionados.Count;
            Debug.Log(playerMarker.position);
            ARLocationManager.Instance.ResetARSession();

        }
        else
        {
            totalElementosRuta = 0;
            posicion = 0;
            LoadingPanel.SetActive(true);

            loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
            loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();

            loadingText.text = "Selecciona la ruta a calcular";
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
            routeOptions.SetActive(false);
            botonRutaAnterior.SetActive(false);
            botonRutaSiguiente.SetActive(false);
        }

    }


    public void dispararEventoHotspot(string name)
    {
        OnlineMaps.instance.position = playerMarker.position;
        OnlineMaps.instance.Redraw();
        Debug.Log("se ha detectado el punto con el nombre: " + name);
        LoadingPanel.SetActive(true);
        loadingText.text = "se ha detectado el punto con el nombre: " + name;

        loadingImage.gameObject.SetActive(false);
        closeLoadingImage.gameObject.SetActive(true);
        obtenerContenidosGPS(name);
    }


    public void ActivarDesactivarHotspot(string name)
    {

        if (parentGPSObjects.transform.childCount > 0)
        {
            foreach (Transform child in parentGPSObjects.transform)
            {
                if (child.gameObject.name.Trim().Equals(name.Trim()))
                {
                    child.gameObject.SetActive(true);
                    break;
                }

            }
        }
    }

    public void crearHotspot(string nombre, string latitud, string longitud)
    {

        Debug.Log("nombre de contenidos hotspot:>" + nombre.Trim());
        float latitudHotspot = float.Parse(latitud, CultureInfo.InvariantCulture.NumberFormat);
        float longitudHotspot = float.Parse(longitud, CultureInfo.InvariantCulture.NumberFormat);
        Location loc = new Location(latitudHotspot, longitudHotspot, 0);
        loc.Label = nombre.Trim();
        loc.AltitudeMode = AltitudeMode.GroundRelative;

        Hotspot.HotspotSettingsData settings = new Hotspot.HotspotSettingsData();
        settings.Prefab = prefabStar;
        settings.ActivationRadius = radioActivacion;
        settings.DeactivateOnLeave = true;
        settings.DeactivationRadius = radioActivacion + 5;
        settings.Reactivate = true;

        Debug.Log("latitud hotspot>" + loc.Latitude);
        Debug.Log("longitud hotspot>" + loc.Longitude);
        GameObject hotspot = Hotspot.CreateHotspotGameObject(loc, settings, nombre.Trim());


        hotspot.transform.parent = parentGPSObjects.transform;

        Debug.Log("agregar listener evento acercamiento");

        hotspot.GetComponent<Hotspot>().LocationSettings.LocationInput.Location.Label = nombre.Trim();
        hotspot.GetComponent<Hotspot>().LocationSettings.LocationInput.Location.Latitude = latitudHotspot;
        hotspot.GetComponent<Hotspot>().LocationSettings.LocationInput.Location.Longitude = longitudHotspot;
        //TODO AQUI PODR�A MODIFICAR EL M�TODO PARA PASAR EL PARAMETRO DE LATITUD Y LONGITUD Y QUE SE CENTRE EL PUNTO AL MOMENTO DE ACTIVARLO 
        hotspot.GetComponent<Hotspot>().OnHotspotActivated.AddListener(delegate { dispararEventoHotspot(nombre.Trim()); });


        var opts = new PlaceAtLocation.PlaceAtOptions()
        {
            HideObjectUntilItIsPlaced = false,
            MaxNumberOfLocationUpdates = 2,
            MovementSmoothing = 0.1f,
            UseMovingAverage = false
        };

        hotspot.SetActive(false);
        /*
        GameObject element = PlaceAtLocation.CreatePlacedInstance(prefabStar, loc, opts);
        stars.Add(element);
        element.GetComponent<PlaceAtLocation>().LocationOptions.LocationInput.Location.Label = a.name.Trim();
        element.GetComponent<PlaceAtLocation>().LocationOptions.LocationInput.Location.Latitude = latitudHotspot;
        element.GetComponent<PlaceAtLocation>().LocationOptions.LocationInput.Location.Longitude = longitudHotspot;
        break;
        */
    }

    /// <summary>
    /// This method is called when the response from Google Directions API is received
    /// </summary>
    /// <param name="response">Response from Google Direction API</param>
    private void OnGoogleDirectionsComplete(string response)
    {
        Debug.Log(response);

        // Try load result
        OnlineMapsGoogleDirectionsResult result = OnlineMapsGoogleDirections.GetResult(response);
        if (result == null || result.routes.Length == 0) return;

        // Get the first route
        OnlineMapsGoogleDirectionsResult.Route route = result.routes[0];

        // Draw route on the map
        OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingLine(route.overview_polyline, Color.red, 3));


        // Calculate the distance
        int distance = route.legs.Sum(l => l.distance.value); // meters

        // Calculate the duration
        int duration = route.legs.Sum(l => l.duration.value); // seconds

        // Log distane and duration
        Debug.Log("Distance: " + distance + " meters, or " + (distance / 1000f).ToString("F2") + " km");
        Debug.Log("Duration: " + duration + " sec, or " + (duration / 60f).ToString("F1") + " min, or " + (duration / 3600f).ToString("F1") + " hours");

    }


    IEnumerator obtenerOrdenRuta()
    {
        hacerNuevoReconocimiento();
        //TODO oocultar donde sale el nombre

        Debug.Log(DropdownRutas.value);
        if (DropdownRutas.value > 0)
        {
            string nombreRuta = rutas.rutas[DropdownRutas.value - 1].nombre;
            string transporte = rutas.rutas[DropdownRutas.value - 1].transporte;
            int id = rutas.rutas[DropdownRutas.value - 1].id;

            Debug.Log(nombreRuta);
            Debug.Log(transporte);
            WWWForm form = new WWWForm();
            //TODO cambiar por obtenerOrdenRuta en el servidor y aqui
            form.AddField("obtenerOrdenRuta2", "ordenRuta");
            form.AddField("id", id);
            this.transporte = transporte;

            UnityWebRequest www = UnityWebRequest.Post(Main.dominio + "/UnitySQL.php", form);
            www.SendWebRequest();
            while (!www.isDone)
                yield return null;

            if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                loadingText.text = www.error;
                loadingImage.gameObject.SetActive(false);
                closeLoadingImage.gameObject.SetActive(true);
            }
            else
            {
                waypoints.Clear();
                waypointsName.Clear();
                puntosRutaSeleccionados.Clear();
                ordenRuta = JsonUtility.FromJson<ListaOrdenRuta>("{\"ordenRutas\":" + www.downloadHandler.text + "}");

                Debug.Log(www.downloadHandler.text);
                for (int i = 0; i < ordenRuta.ordenRutas.Count; i++)
                {
                    int rutaId = ordenRuta.ordenRutas[i].id;
                    float orden = ordenRuta.ordenRutas[i].ordenRuta;
                    string latLngString = ordenRuta.ordenRutas[i].latLng;
                    string[] latLng = latLngString.Split(",");
                    string waypointName = ordenRuta.ordenRutas[i].nombrePunto;
                    bool incluirPuntoRuta = ordenRuta.ordenRutas[i].incluirPuntoRuta;
                    Debug.Log(ordenRuta.ordenRutas[i].id + "|" + ordenRuta.ordenRutas[i].ordenRuta + "|" + ordenRuta.ordenRutas[i].latLng + "|" + ordenRuta.ordenRutas[i].nombrePunto + "|" + ordenRuta.ordenRutas[i].incluirPuntoRuta);


                    RutaSeleccionada poi = new RutaSeleccionada(rutaId, orden, float.Parse(latLng[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(latLng[0], CultureInfo.InvariantCulture.NumberFormat), waypointName, incluirPuntoRuta);
                    puntosRutaSeleccionados.Add(poi);
                }
                crearWaypoints(puntosRutaSeleccionados);
            }
        }
    }

    public void crearWaypoints(List<RutaSeleccionada> ruta)
    {

        float mayor = 0.0f;

        for (int i = 0; i < ruta.Count; i++)
        {
            if ((ruta[i].GetOrdenRuta() % 1) == 0 && ruta[i].GetOrdenRuta() > mayor)
            {
                mayor = ruta[i].GetOrdenRuta();
            }
        }

        for (int i = 0; i < ruta.Count; i++)
        {
            Debug.Log("indice mayor: " + mayor);
            Debug.Log("orden ruta " + ruta[i].GetOrdenRuta() + "|" + ruta[i].GetNombrePunto());
            if ((ruta[i].GetOrdenRuta() % 1) == 0 && ruta[i].GetOrdenRuta() == 1.0)
            {
                origin = new Vector2(ruta[i].GetLat(), ruta[i].GetLng());
                Debug.Log("origen:" + origin);
            }
            else if ((ruta[i].GetOrdenRuta() % 1) == 0 && ruta[i].GetOrdenRuta() == mayor)
            {
                destino = new Vector2(ruta[i].GetLat(), ruta[i].GetLng());
                Debug.Log("destino:" + destino);
            }
            else if ((ruta[i].GetOrdenRuta() % 1) == 0)
            {
                Vector2 pos = new Vector2(ruta[i].GetLat(), ruta[i].GetLng());
                Debug.Log("waypoint:" + pos);
                waypoints.Add(pos);
            }
        }
    }




    IEnumerator obtenerRutas()
    {
        WWWForm form = new WWWForm();
        form.AddField("obtenerRutas", "rutas");

        UnityWebRequest www = UnityWebRequest.Post(Main.dominio + "/UnitySQL.php", form);
        www.SendWebRequest();
        while (!www.isDone)
            yield return null;

        if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            loadingText.text = www.error;
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            rutas = JsonUtility.FromJson<ListaRuta>("{\"rutas\":" + www.downloadHandler.text + "}");
            List<string> nombresRuta = new List<string>();
            nombresRuta.Add("Seleccionar ruta");
            for (int i = 0; i < rutas.rutas.Count; i++)
            {
                nombresRuta.Add(rutas.rutas[i].nombre.ToString());

            }
            DropdownRutas.AddOptions(nombresRuta);
            DropdownRutas.onValueChanged.RemoveAllListeners();

            DropdownRutas.onValueChanged.AddListener(delegate { StartCoroutine(obtenerOrdenRuta()); });
        }
    }

    //TODO CENTRAR
    public void centrarPosicion(double latitud, double longitud)
    {
        //OnlineMaps.instance.SetPosition(longitud, latitud);
        Debug.Log("CENTRAR POSICION: " + latitud + "/" + longitud);
        OnlineMaps.instance.SetPositionAndZoom(longitud, latitud, 14);
    }


    public void obtenerRuta(int seleccionSiguienteAnterior)
    {
        if (puntosRutaSeleccionados.Count > 0)
        {

            routeOptions.SetActive(true);
            botonRutaAnterior.SetActive(true);
            botonRutaSiguiente.SetActive(true);

            Debug.Log("seleccion:" + seleccionSiguienteAnterior);
            if (seleccionSiguienteAnterior == 1)
            {
                Debug.Log("siguiente punto:" + (posicion + 1));
                if ((posicion + 1) < puntosRutaSeleccionados.Count)
                {
                    posicion = posicion + 1;
                    double longitud = puntosRutaSeleccionados[posicion].GetLng();
                    double latitud = puntosRutaSeleccionados[posicion].GetLat();
                    Debug.Log("LATITUD Y LONGITUD DEL PUNTO:" + latitud + "/" + longitud);
                    string name = puntosRutaSeleccionados[posicion].GetNombrePunto();


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
                    GPSContentPanel.SetActive(true);
                    obtenerContenidosGPS(name);
                    poiNameSelected.text = puntosRutaSeleccionados[posicion].GetOrdenRuta() + " " + name;



                    if (calcularRutaPunto(name))
                    {
                        calcularRutaMapbox(latitud, longitud, name);
                    }
                    else
                    {
                        centrarPosicion(longitud, latitud);
                    }
                }
                else if ((posicion + 1) == puntosRutaSeleccionados.Count)
                {
                    posicion = posicion + 1;
                    string name = puntosRutaSeleccionados[posicion].GetNombrePunto();

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
                    GPSContentPanel.SetActive(true);
                    obtenerContenidosGPS(name);
                    poiNameSelected.text = puntosRutaSeleccionados[posicion].GetOrdenRuta() + " " + name;



                    calcularRutaMapbox(destino.x, destino.y, name);




                    botonRutaSiguiente.SetActive(false);
                }

            }
            else if (seleccionSiguienteAnterior == 2)
            {
                Debug.Log("anterior punto:" + (posicion - 1));
                if ((posicion - 1) >= 0)
                {
                    posicion = posicion - 1;
                    double longitud = puntosRutaSeleccionados[posicion].GetLng();
                    double latitud = puntosRutaSeleccionados[posicion].GetLat();
                    string name = puntosRutaSeleccionados[posicion].GetNombrePunto();
                    Debug.Log("LATITUD Y LONGITUD DEL PUNTO:" + latitud + "/" + longitud);

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
                    GPSContentPanel.SetActive(true);
                    obtenerContenidosGPS(name);
                    poiNameSelected.text = puntosRutaSeleccionados[posicion].GetOrdenRuta() + " " + name;

                    if (calcularRutaPunto(name))
                    {
                        calcularRutaMapbox(latitud, longitud, name);
                    }
                    else
                    {
                        centrarPosicion(longitud, latitud);
                    }
                    if (posicion == 0)
                    {
                        botonRutaAnterior.SetActive(false);
                    }
                }
                else
                {
                    botonRutaAnterior.SetActive(false);
                }
            }
        }
        else
        {
            botonRutaAnterior.SetActive(false);
            botonRutaSiguiente.SetActive(false);
        }

    }


    public bool calcularRutaPunto(string name)
    {
        bool calcular = false;
        for (int i = 0; i < puntosRutaSeleccionados.Count; i++)
        {
            if (puntosRutaSeleccionados[i].GetNombrePunto().Equals(name))
            {
                calcular = puntosRutaSeleccionados[i].GetIncluirPuntoRuta();
                break;
            }
        }

        return calcular;
    }
    public void calcularRutaMapbox(double latitud, double longitud, string name)
    {
        MapboxRoute script = MapboxRoute.GetComponent<MapboxRoute>();
        Location poi = script.Settings.RouteSettings.To.Location;
        poi.Label = name;
        poi.Latitude = longitud;
        poi.Longitude = latitud;
        script.Settings.LoadRouteAtStartup = true;
        script.LoadRoute();
        destinoMapbox = new Location(longitud, latitud);
        loadRoute(ARLocationProvider.Instance.CurrentLocation.ToLocation());
        Debug.Log("LATITUD Y LONGITUD DEL PUNTO:" + latitud + "/" + longitud);
        centrarPosicion(longitud, latitud);


    }

    private void loadRoute(Location _)
    {
        if (destinoMapbox != null)
        {
            var api = new MapboxApi(MapboxToken);
            var loader = new RouteLoader(api);
            StartCoroutine(
                    loader.LoadRoute(
                        new RouteWaypoint { Type = RouteWaypointType.UserLocation },
                        new RouteWaypoint { Type = RouteWaypointType.Location, Location = destinoMapbox },
                        (err, res) =>
                        {
                            if (err != null)
                            {
                                loadingText.text = err;
                                loadingImage.gameObject.SetActive(false);
                                closeLoadingImage.gameObject.SetActive(true);
                                return;
                            }
                            currentPathRenderer.enabled = true;
                            MapboxRoute2.RoutePathRenderer = currentPathRenderer;
                            bool resultado = MapboxRoute2.BuildRoute(res);
                            if (!resultado)
                            {
                                //TODO mensaje de error donde se mencione que no hay camino para ir desde donde se encuentra el usuario hasta la ruta seleccionada
                                loadingText.text = "No hay camino para el usuario";
                                loadingImage.gameObject.SetActive(false);
                                closeLoadingImage.gameObject.SetActive(true);
                            }
                            else
                            {
                                Debug.Log("EL CAMINO ES : EL USUARIO: " + RouteWaypointType.UserLocation + " /EL DESTINO: " + destinoMapbox.Latitude + "/" + destinoMapbox.Longitude);
                            }

                        }));
        }
    }


    private AbstractRouteRenderer currentPathRenderer => s.LineType == LineType.Route ? RoutePathRenderer : NextTargetPathRenderer;

    public LineType PathRendererType
    {
        get => s.LineType;
        set
        {
            if (value != s.LineType)
            {
                currentPathRenderer.enabled = false;
                s.LineType = value;
                currentPathRenderer.enabled = true;
                MapboxRoute2.RoutePathRenderer = currentPathRenderer;
            }
        }
    }

}
