using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


[Serializable]
public class NameTag
{
    public string nombre;
}

[Serializable]
public class Tag
{
    public List<NameTag> tags;
}



public class CreatePoi : MonoBehaviour
{

    
    public GameObject createPoiScene;
    public GameObject PictureScene;
    public GameObject LoadingPanel;
    public GameObject ContenedorEtiquetasPoi;
    public GameObject ContenedorARImages;
    public GameObject ContenedorAudios;
    public GameObject ContenedorImagenes;
    public GameObject ContenedorVideos;
    public GameObject ContenedorModelos;
    public GameObject ContenedorWebs;
    public GameObject ContenedorTextos;
    public GameObject UI;

    public GameObject prefabEtiqueta;
    public GameObject prefabImagenAR;
    public GameObject prefabContenido;


    public TMP_InputField poiName;
    public ToggleGroup visualization;
    public ToggleGroup edition;
    public TMP_InputField latitude;
    public TMP_InputField longitude;


    RawImage loadingImage;
    RawImage arImage;
    TextMeshProUGUI loadingText;
    RawImage closeLoadingImage;
    List<string> etiquetasContenido = new List<string>();
    List<GameObject> imagenesAR = new List<GameObject>();
    List<GameObject> audios = new List<GameObject>();
    List<GameObject> imagenes = new List<GameObject>();
    List<GameObject> videos = new List<GameObject>();
    List<GameObject> modelos = new List<GameObject>();
    List<GameObject> webs = new List<GameObject>();
    List<GameObject> textos = new List<GameObject>();


    public void abrirCamara(RawImage image)
    {
        arImage = image;
        createPoiScene.SetActive(false);
        PictureScene.SetActive(true);
    }

    void Start()
    {
        loadingImage = LoadingPanel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        loadingText = LoadingPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        closeLoadingImage = LoadingPanel.transform.GetChild(2).gameObject.GetComponent<RawImage>();
        StartCoroutine(descargarEtiquetasPoi());
        StartCoroutine(descargarEtiquetasContenido());
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
    }


    public IEnumerator descargarEtiquetasPoi()
    {
        WWWForm www = new WWWForm();
        www.AddField("obtenerTagsPOI", "obtenerTagsPOI");
        www.AddField("lang", Web.SystemLanguage);

        string uri = Main.dominio + "/UnitySQL.php";

        UnityWebRequest request = UnityWebRequest.Post(uri, www);
        LoadingPanel.SetActive(false);
        request.SendWebRequest();

        while (!request.isDone)
        {
            yield return null;
            loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
            loadingText.text = "Descargando etiquetas punto de interes ...";
        }

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Unity Web request error:" + request.error);
            loadingText.text = request.error;
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
        }
        else
        {
            if (!request.downloadHandler.text.Trim().Equals(""))
            {
                string json = request.downloadHandler.text.Trim();
                Debug.Log(json);
                Tag tagsPOI = JsonUtility.FromJson<Tag>("{\"tags\":" + json + "}");
                for (int i = 0; i < tagsPOI.tags.Count; i++)
                {
                    GameObject prefab = Instantiate(prefabEtiqueta);
                    prefab.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = tagsPOI.tags[i].nombre;
                    prefab.transform.SetParent(ContenedorEtiquetasPoi.transform, false);
                }
            }
        }
    }


    
    public IEnumerator descargarEtiquetasContenido()
    {
        WWWForm www = new WWWForm();
        www.AddField("obtenerTagsContenido", "obtenerTagsContenido");
        www.AddField("lang", Web.SystemLanguage);

        string uri = Main.dominio + "/UnitySQL.php";

        UnityWebRequest request = UnityWebRequest.Post(uri, www);
        LoadingPanel.SetActive(false);
        request.SendWebRequest();

        while (!request.isDone)
        {
            yield return null;
            loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
            loadingText.text = "Descargando etiquetas contenido ...";
        }

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Unity Web request error:" + request.error);
            loadingText.text = request.error;
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
        }
        else
        {
            if (!request.downloadHandler.text.Trim().Equals(""))
            {
                string json = request.downloadHandler.text.Trim();
                Debug.Log(json);
                Tag tagsContenido = JsonUtility.FromJson<Tag>("{\"tags\":" + json + "}");
                for (int i = 0; i < tagsContenido.tags.Count; i++)
                {
                    etiquetasContenido.Add(tagsContenido.tags[i].nombre);
                }
            }
        }
    }

    public void agregarContenido(string tipo)
    {
        GameObject prefab = Instantiate(prefabContenido);
        TextMeshProUGUI tipoContenido = prefab.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        Button delete = prefab.transform.GetChild(0).gameObject.transform.GetChild(1).GetComponent<Button>();
        TMP_InputField descripcion = prefab.transform.GetChild(1).GetComponent<TMP_InputField>();
        TMP_InputField url = prefab.transform.GetChild(2).GetComponent<TMP_InputField>();
        GameObject contenedorEtiquetasContenido = prefab.transform.GetChild(3).Find("EtiquetasList/Scroll View/Viewport/Content").gameObject;
        
        foreach(string tag in etiquetasContenido)
        {
            GameObject etiqueta = Instantiate(prefabEtiqueta);
            etiqueta.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = tag;
            etiqueta.transform.SetParent(contenedorEtiquetasContenido.transform, false);
        }

        if (tipo.Equals("audio"))
        {
            tipoContenido.text = "audio";
            descripcion.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite descripcion del audio";
            url.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite url del audio";
            delete.onClick.AddListener(delegate { audios.Remove(prefab); DestroyImmediate(prefab); LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorAudios.GetComponent<RectTransform>()); });
            prefab.transform.SetParent(ContenedorAudios.transform,false);
            audios.Add(prefab);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorAudios.GetComponent<RectTransform>());
        }
        else if (tipo.Equals("imagen"))
        {
            tipoContenido.text = "imagen";
            descripcion.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite descripcion de la imagen";
            url.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite url de la imagen";
            delete.onClick.AddListener(delegate { imagenes.Remove(prefab); DestroyImmediate(prefab); LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorImagenes.GetComponent<RectTransform>()); });
            prefab.transform.SetParent(ContenedorImagenes.transform, false);
            imagenes.Add(prefab);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorImagenes.GetComponent<RectTransform>());
        }
        else if (tipo.Equals("video"))
        {
            tipoContenido.text = "video";
            descripcion.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite descripcion del video";
            url.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite url del video";
            delete.onClick.AddListener(delegate { videos.Remove(prefab); DestroyImmediate(prefab); LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorVideos.GetComponent<RectTransform>()); });
            prefab.transform.SetParent(ContenedorVideos.transform, false);
            videos.Add(prefab);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorVideos.GetComponent<RectTransform>());
        }
        else if (tipo.Equals("modelo"))
        {
            tipoContenido.text = "modelo 3D";
            descripcion.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite descripcion del modelo 3D";
            url.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite url del modelo 3D";
            delete.onClick.AddListener(delegate { modelos.Remove(prefab); DestroyImmediate(prefab); LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorModelos.GetComponent<RectTransform>()); });
            prefab.transform.SetParent(ContenedorModelos.transform, false);
            modelos.Add(prefab);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorModelos.GetComponent<RectTransform>());
        }
        else if (tipo.Equals("web"))
        {
            tipoContenido.text = "web";
            descripcion.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite descripcion del sitio web";
            url.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite url del sitio web";
            delete.onClick.AddListener(delegate { webs.Remove(prefab); DestroyImmediate(prefab); LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorWebs.GetComponent<RectTransform>()); });
            prefab.transform.SetParent(ContenedorWebs.transform, false);
            webs.Add(prefab);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorWebs.GetComponent<RectTransform>());
        }
        else if (tipo.Equals("texto"))
        {
            tipoContenido.text = "texto";
            descripcion.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite descripcion del texto";
            url.placeholder.GetComponent<TextMeshProUGUI>().text = "Digite texto";
            delete.onClick.AddListener(delegate { textos.Remove(prefab); DestroyImmediate(prefab); LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorTextos.GetComponent<RectTransform>()); });
            prefab.transform.SetParent(ContenedorTextos.transform, false);
            textos.Add(prefab);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorTextos.GetComponent<RectTransform>());
        }
        
    }

    public void agregarImagen()
    {
        GameObject arImage = Instantiate(prefabImagenAR);
        imagenesAR.Add(arImage);
        arImage.transform.SetParent(ContenedorARImages.transform, false);

        RawImage imagen = arImage.transform.GetChild(0).GetComponent<RawImage>();
        Button seleccionarImagen = arImage.transform.GetChild(1).GetComponent<Button>();
        Button tomarImagen = arImage.transform.GetChild(2).GetComponent<Button>();
        Button eliminarImagen = arImage.transform.GetChild(3).GetComponent<Button>();

        seleccionarImagen.onClick.AddListener(delegate { StartCoroutine(seleccionarImagenCarrete(imagen)); });
        tomarImagen.onClick.AddListener(()=> abrirCamara(imagen));
        eliminarImagen.onClick.AddListener(()=>eliminarImagenAR(arImage));

        LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorARImages.GetComponent<RectTransform>());
    }

    public IEnumerator seleccionarImagenCarrete(RawImage imagen)
    {
        yield return new WaitForEndOfFrame();
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);

            if (path != null)
            {
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }
                imagen.GetComponent<RawImage>().texture = texture;
            }
        }, "Select a PNG image", "image/png");

        Debug.Log("Permission result: " + permission);
    }

    public void eliminarImagenAR(GameObject arImage)
    {
        imagenesAR.Remove(arImage);
        DestroyImmediate(arImage); 
        LayoutRebuilder.ForceRebuildLayoutImmediate(ContenedorARImages.GetComponent<RectTransform>());
    }

    private IEnumerator ScreenShot(RawImage arImage)
    {
        yield return new WaitForEndOfFrame();
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);


        string name = "Screenshot" + System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        //pc
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../" + name, bytes);
        String ruta = Application.dataPath + "/../" + name;

        //Mobile
        NativeGallery.SaveImageToGallery(texture, "capturas de pantalla Motiv-ARCHE", name);

        UI.SetActive(true);
        PictureScene.SetActive(false);
        createPoiScene.SetActive(true);
        arImage.texture = NativeGallery.LoadImageAtPath(Application.dataPath + "/../" + name, -1, false, true, false);
    }

    public void takeScreenShot()
    {
        UI.SetActive(false);
        StartCoroutine(ScreenShot(arImage));
    }

    public void closeScreenshot()
    {
        arImage = null;
        createPoiScene.SetActive(true);
        PictureScene.SetActive(false);
    }

    private void OnMapClick()
    {
        // Get the coordinates under the cursor.
        double lng, lat;
        OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

        // Create a label for the marker.
        string label = "Posicion " + (OnlineMapsMarkerManager.CountItems + 1);
        if (OnlineMapsMarkerManager.CountItems >= 1)
        {
            OnlineMapsMarkerManager.RemoveAllItems();
        }
        // Create a new marker.
        OnlineMapsMarkerManager.CreateItem(lng, lat, label);
        latitude.text = lat.ToString();
        longitude.text = lng.ToString();

    }

    public void crearPuntoInteres()
    {
        string name = poiName.text;
        string email = Main.email;
        bool visualizationAll = true;
        bool editionAll = true;
        List<string> etiquetasPoiSeleccionadas = new List<string>();
        List<string> imagenesAR64 = new List<string>();
        double latitud=0;
        double longitud=0;
        List<Contenido> contenidos = new List<Contenido>();


        foreach (Toggle t in visualization.ActiveToggles())
        {
            if (t.isOn && t.name.Equals("RadioButtonSoloYo"))
            {
                visualizationAll = false;
            }
            
        }

        foreach (Toggle t in edition.ActiveToggles())
        {
            if (t.isOn && t.name.Equals("RadioButtonSoloYo"))
            {
                editionAll = false;
            }
            
        }

        for(int i = 0; i < ContenedorEtiquetasPoi.transform.childCount; i++)
        {
            if (ContenedorEtiquetasPoi.transform.GetChild(i).GetComponent<Toggle>().isOn)
            {
                etiquetasPoiSeleccionadas.Add(ContenedorEtiquetasPoi.transform.GetChild(i).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text);
            }
        }

        for(int i = 0; i < imagenesAR.Count; i++)
        {

            Texture2D textura = duplicateTexture((Texture2D)imagenesAR[i].transform.GetChild(0).GetComponent<RawImage>().texture);
            Texture2D text = new Texture2D(textura.width, textura.height, TextureFormat.RGB24, false);
            text.SetPixels(textura.GetPixels());
            text.Apply();
            byte[] imagenByte = text.EncodeToPNG();
            string imagen = System.Convert.ToBase64String(imagenByte);
            imagenesAR64.Add(imagen);
        }

        if(!latitude.text.Trim().Equals(""))
        latitud = Convert.ToDouble(latitude.text.Replace(".",","));
        if (!longitude.text.Trim().Equals(""))
            longitud = Convert.ToDouble(longitude.text.Replace(".", ","));

        for (int i = 0; i < audios.Count; i++)
        {
            Debug.Log(audios[i].transform.GetChild(1).name);
            string descripcion = audios[i].transform.GetChild(1).GetComponent<TMP_InputField>().text;
            string contenido = audios[i].transform.GetChild(2).GetComponent<TMP_InputField>().text;
            GameObject contenedorEtiquetasContenido = audios[i].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;
            List<string> etiquetas;
            etiquetas = ObtenerEtiquetasContenido(contenedorEtiquetasContenido);
            Contenido c = new Contenido(descripcion, contenido, "audio",etiquetas);
            contenidos.Add(c);
        }

        for (int i = 0; i < imagenes.Count; i++)
        {
            string descripcion = imagenes[i].transform.GetChild(1).GetComponent<TMP_InputField>().text;
            string contenido = imagenes[i].transform.GetChild(2).GetComponent<TMP_InputField>().text;
            GameObject contenedorEtiquetasContenido = imagenes[i].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;
            List<string> etiquetas;
            etiquetas = ObtenerEtiquetasContenido(contenedorEtiquetasContenido);
            Contenido c = new Contenido(descripcion, contenido, "imagen",etiquetas);
            contenidos.Add(c);
        }

        for (int i = 0; i < videos.Count; i++)
        {
            string descripcion = videos[i].transform.GetChild(1).GetComponent<TMP_InputField>().text;
            string contenido = videos[i].transform.GetChild(2).GetComponent<TMP_InputField>().text;
            GameObject contenedorEtiquetasContenido = videos[i].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;
            List<string> etiquetas;
            etiquetas = ObtenerEtiquetasContenido(contenedorEtiquetasContenido);
            Contenido c = new Contenido(descripcion, contenido, "video",etiquetas);
            contenidos.Add(c);
        }

        for (int i = 0; i < modelos.Count; i++)
        {
            string descripcion = modelos[i].transform.GetChild(1).GetComponent<TMP_InputField>().text;
            string contenido = modelos[i].transform.GetChild(2).GetComponent<TMP_InputField>().text;
            GameObject contenedorEtiquetasContenido = modelos[i].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;
            List<string> etiquetas;
            etiquetas = ObtenerEtiquetasContenido(contenedorEtiquetasContenido);
            Contenido c = new Contenido(descripcion, contenido, "modelo",etiquetas);
            contenidos.Add(c);
        }

        for (int i = 0; i < webs.Count; i++)
        {
            string descripcion = webs[i].transform.GetChild(1).GetComponent<TMP_InputField>().text;
            string contenido = webs[i].transform.GetChild(2).GetComponent<TMP_InputField>().text;
            GameObject contenedorEtiquetasContenido = webs[i].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;
            List<string> etiquetas;
            etiquetas = ObtenerEtiquetasContenido(contenedorEtiquetasContenido);
            Contenido c = new Contenido(descripcion, contenido, "web",etiquetas);
            contenidos.Add(c);
        }

        for (int i = 0; i < textos.Count; i++)
        {
            string descripcion = textos[i].transform.GetChild(1).GetComponent<TMP_InputField>().text;
            string contenido = textos[i].transform.GetChild(2).GetComponent<TMP_InputField>().text;
            GameObject contenedorEtiquetasContenido = textos[i].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;
            List<string> etiquetas;
            etiquetas = ObtenerEtiquetasContenido(contenedorEtiquetasContenido);
            Contenido c = new Contenido(descripcion, contenido, "texto",etiquetas);
            contenidos.Add(c);
        }

        Poi newPoi = new Poi(name, visualizationAll, editionAll, etiquetasPoiSeleccionadas, imagenesAR64, latitud, longitud, contenidos, email);
        CrearPoi(newPoi);
    }

    

    public List<String> ObtenerEtiquetasContenido(GameObject contenedorEtiquetasContenido)
    {
        List<string> etiquetasSeleccionadas = new List<string>();
        for(int i = 0; i < contenedorEtiquetasContenido.transform.childCount; i++)
        {
            if (contenedorEtiquetasContenido.transform.GetChild(i).GetComponent<Toggle>().isOn)
            {
                etiquetasSeleccionadas.Add(contenedorEtiquetasContenido.transform.GetChild(i).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text);
            }
        }

        return etiquetasSeleccionadas;
    }

    public void CrearPoi(Poi newPoi)
    {

        string uri = Main.dominio + "/crear-punto-interes-validacion.php";
        string base64 = "";
        string tags = "";
        string contenidos = "";

        WWWForm www = new WWWForm();
        www.AddField("crearPuntoInteres", "crearPuntoInteres");
        www.AddField("targetname", newPoi.getNombre());
        www.AddField("email", newPoi.getEmail());

        foreach (string a in newPoi.getImagenesAR())
        {
            base64 += a + "|";
        }

        if (!base64.Equals(""))
        {
            base64 = base64.Substring(0, base64.Length - 1);
            www.AddField("base64", base64);
        }

        if (newPoi.getEdicion())
        {
            www.AddField("editable", newPoi.getEdicion().ToString());
        }
 
        if (newPoi.getVisualizacion())
        {
            www.AddField("activo", newPoi.getVisualizacion().ToString());
        }

        
        foreach(string a in newPoi.getEtiquetas())
        {
            tags += a + ",";
        }

        if (!tags.Equals(""))
        {
            tags = tags.Substring(0, tags.Length - 1);
            www.AddField("tags", tags);
        }
        
        foreach (Contenido c in newPoi.getContenidos())
        {
            string etiquetasContenido = "";
            foreach(string e in c.getEtiquetas())
            {
                etiquetasContenido += e + ",";
            }

            if (!etiquetasContenido.Equals(""))
            {
                etiquetasContenido = etiquetasContenido.Substring(0, etiquetasContenido.Length - 1);
                contenidos += c.getDescripcion() + "|" + c.getContenido() + "|" + c.getTipo() + "|" + etiquetasContenido + "||";
            }
            else
            {
                contenidos += c.getDescripcion() + "|" + c.getContenido() + "|" + c.getTipo() + "||";
            }
            
        }

        if (!contenidos.Equals(""))
        {
            contenidos = contenidos.Substring(0, contenidos.Length - 2);
            www.AddField("contenidos", contenidos);
        }

        if (OnlineMapsMarkerManager.CountItems >= 1)
        {
            www.AddField("latMap", newPoi.getLatitud().ToString().Replace(",","."));
            www.AddField("lngMap", newPoi.getLongitud().ToString().Replace(",", "."));
        }else if(!latitude.text.Trim().Equals("") && longitude.text.Trim().Equals(""))
        {
            www.AddField("latMap", latitude.text.Trim().Replace(",", "."));
            www.AddField("lngMap", longitude.text.Trim().Replace(",", "."));
        }
        StartCoroutine(crearPeticion(uri, www));
    }


    IEnumerator crearPeticion(string uri, WWWForm www)
    {
        UnityWebRequest request = UnityWebRequest.Post(uri, www);
        LoadingPanel.SetActive(false);
        request.SendWebRequest();


        LoadingPanel.SetActive(true);
        loadingImage.gameObject.SetActive(true);
        closeLoadingImage.gameObject.SetActive(false);
        while (!request.isDone)
        {
            yield return null;
            loadingImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
            loadingText.text = "Creando punto de interes ...";
        }

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Unity Web request error:" + request.error);
            loadingText.text = request.error;
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
        }
        else
        {
            LoadingPanel.SetActive(true);
            loadingImage.gameObject.SetActive(false);
            closeLoadingImage.gameObject.SetActive(true);
            string response = request.downloadHandler.text.Trim();
            if (response.Equals("ok"))
            {
                loadingText.text = "Se ha creado el punto de interes correctamente";
            }
            else
            {
                loadingText.text = response;
            }
            Debug.Log(response);
        }
    }

    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

}
