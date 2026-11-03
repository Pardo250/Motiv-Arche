using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

// Registro en campo: permite tomar fotos de los murales desde la app y guardarlas en el celular
// con el mural, la ubicación GPS, la fecha y una nota. Funciona sin señal; solo la lista de murales
// necesita internet la primera vez. Agrega su propio botón al menú y construye su interfaz por código.
public class RegistroCampo : MonoBehaviour
{
    // Ruta "Ruta de la Memoria - A Pie".
    const int IdRuta = 189;
    // En true el botón también aparece para invitados. Dejar en false en el APK para la comunidad.
    const bool VisibleParaInvitados = true;
    // Cada mural tiene su propio álbum en la galería del celular.
    const string PrefijoAlbum = "Motiv-Arche - ";

    [Serializable]
    class PuntoJson
    {
        public string ordenRuta;
        public string latLng;
        public string nombrePunto;
    }

    [Serializable]
    class ListaPuntosJson
    {
        public List<PuntoJson> puntos;
    }

    class Mural
    {
        public string orden;
        public string nombre;
        public bool tieneCoordenadas;
        public double latitud;
        public double longitud;
        public Text etiqueta;
    }

    readonly List<Mural> murales = new List<Mural>();
    string carpeta;
    string archivoRuta;
    string archivoRegistro;
    Font fuente;

    GameObject panelLista;
    GameObject panelFoto;
    RectTransform contenidoLista;
    Text textoEstadoLista;
    Text textoTituloFoto;
    Text textoGps;
    Text textoEstadoFoto;
    RawImage vistaCamara;
    RectTransform areaCamara;
    InputField campoNota;
    InputField campoOtroPunto;

    GameObject panelGaleria;
    GameObject panelGrande;
    RectTransform contenidoGaleria;
    GridLayoutGroup rejillaGaleria;
    Text textoTituloGaleria;
    Text textoEstadoGaleria;
    RawImage imagenGrande;
    RectTransform areaGrande;
    Text textoInfoGrande;
    Text textoBotonBorrar;
    string rutaGrande;
    bool confirmandoBorrado;
    Texture2D texturaGrande;
    readonly List<Texture2D> miniaturas = new List<Texture2D>();
    Coroutine cargaMiniaturas;

    WebCamTexture camara;
    Mural muralActual;
    float proximoRefresco;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Registrar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "MenuScene") AgregarBotonMenu();
    }

    static void AgregarBotonMenu()
    {
        if (!VisibleParaInvitados && Main.esInvitado) return;

        GameObject menu = GameObject.Find("Canvas/MenuItems");
        Transform plantilla = menu != null ? menu.transform.Find("GPSOption") : null;
        if (plantilla == null)
        {
            Debug.LogWarning("RegistroCampo: no se encontró GPSOption en el menú.");
            return;
        }

        GameObject opcion = Instantiate(plantilla.gameObject, menu.transform);
        opcion.name = "RegistroCampoOption";

        TMP_Text texto = opcion.GetComponentInChildren<TMP_Text>(true);
        if (texto != null) texto.text = "Registrar murales";

        Button boton = opcion.GetComponentInChildren<Button>(true);
        if (boton == null) return;
        boton.onClick = new Button.ButtonClickedEvent();
        boton.onClick.AddListener(() => new GameObject("RegistroCampo").AddComponent<RegistroCampo>());
    }

    void Awake()
    {
        carpeta = Path.Combine(Application.persistentDataPath, "RegistroCampo");
        Directory.CreateDirectory(carpeta);
        archivoRuta = Path.Combine(carpeta, "ruta.json");
        archivoRegistro = Path.Combine(carpeta, "registro.csv");
        fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");
        OrdenarFotosAntiguas();
        ConstruirInterfaz();
    }

    void Start()
    {
        StartCoroutine(IniciarGps());
        StartCoroutine(CargarMurales());
    }

    void OnDestroy()
    {
        DetenerCamara();
        LiberarMiniaturas();
        LiberarGrande();
        Input.location.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelGrande.activeSelf) CerrarGrande();
            else if (panelGaleria.activeSelf) CerrarGaleria();
            else if (panelFoto.activeSelf) VolverALista();
            else Destroy(gameObject);
            return;
        }

        if (Time.unscaledTime < proximoRefresco) return;
        proximoRefresco = Time.unscaledTime + 1f;
        ActualizarGps();
        if (panelLista.activeSelf) ActualizarDistancias();
    }

    // ---------- GPS ----------

    IEnumerator IniciarGps()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            float espera = 0f;
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && espera < 15f)
            {
                espera += Time.unscaledDeltaTime;
                yield return null;
            }
        }
#endif
        Input.location.Start(5f, 1f);
        yield break;
    }

    bool HayGps { get { return Input.location.status == LocationServiceStatus.Running; } }

    void ActualizarGps()
    {
        if (textoGps == null) return;

        if (HayGps)
        {
            LocationInfo l = Input.location.lastData;
            textoGps.text = "GPS: " + Numero(l.latitude, 6) + ", " + Numero(l.longitude, 6) + "  (±" + Numero(l.horizontalAccuracy, 0) + " m)";
        }
        else if (Input.location.status == LocationServiceStatus.Failed || !Input.location.isEnabledByUser)
        {
            textoGps.text = "GPS apagado o sin permiso. La foto se guarda sin coordenadas.";
        }
        else
        {
            textoGps.text = "Buscando GPS...";
        }
    }

    void ActualizarDistancias()
    {
        if (!HayGps) return;

        LocationInfo l = Input.location.lastData;
        Mural cercano = null;
        double menor = double.MaxValue;
        foreach (Mural m in murales.Where(x => x.tieneCoordenadas && x.etiqueta != null))
        {
            double d = Distancia(l.latitude, l.longitude, m.latitud, m.longitud);
            if (d < menor)
            {
                menor = d;
                cercano = m;
            }
        }

        foreach (Mural m in murales.Where(x => x.tieneCoordenadas && x.etiqueta != null))
        {
            double d = Distancia(l.latitude, l.longitude, m.latitud, m.longitud);
            m.etiqueta.text = TextoMural(m) + "\n" + (m == cercano ? "► Más cercano · " : "") + "a " + Numero(d, 0) + " m";
        }
    }

    static double Distancia(double lat1, double lng1, double lat2, double lng2)
    {
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLng = (lng2 - lng1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return 6371000.0 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // ---------- Lista de murales ----------

    IEnumerator CargarMurales()
    {
        textoEstadoLista.text = "Cargando la lista de murales...";

        WWWForm form = new WWWForm();
        form.AddField("obtenerOrdenRuta2", "ordenRuta");
        form.AddField("id", IdRuta);

        using (UnityWebRequest www = UnityWebRequest.Post(Main.dominio + "/UnitySQL.php", form))
        {
            www.timeout = 10;
            yield return www.SendWebRequest();

            bool ok = www.result == UnityWebRequest.Result.Success && LeerMurales(www.downloadHandler.text);
            if (ok)
            {
                File.WriteAllText(archivoRuta, www.downloadHandler.text);
                textoEstadoLista.text = murales.Count + " murales. Lista guardada para usarla sin señal.";
            }
            else if (File.Exists(archivoRuta) && LeerMurales(File.ReadAllText(archivoRuta)))
            {
                textoEstadoLista.text = "Sin conexión: " + murales.Count + " murales de la lista guardada.";
            }
            else
            {
                textoEstadoLista.text = "Sin lista: abre esta pantalla una vez con internet, o escribe el nombre abajo.";
            }
        }

        DibujarLista();
    }

    bool LeerMurales(string json)
    {
        if (string.IsNullOrEmpty(json) || !json.TrimStart().StartsWith("[")) return false;

        ListaPuntosJson lista;
        try
        {
            lista = JsonUtility.FromJson<ListaPuntosJson>("{\"puntos\":" + json + "}");
        }
        catch (ArgumentException)
        {
            return false;
        }
        if (lista == null || lista.puntos == null || lista.puntos.Count == 0) return false;

        murales.Clear();
        foreach (PuntoJson p in lista.puntos)
        {
            string nombre = p.nombrePunto == null ? "" : p.nombrePunto.Trim();
            // La ruta puede repetir un mural; basta una entrada por nombre.
            if (nombre.Length == 0 || murales.Any(m => m.nombre == nombre)) continue;

            Mural mural = new Mural { orden = p.ordenRuta, nombre = nombre };
            string[] partes = p.latLng == null ? new string[0] : p.latLng.Split(',');
            mural.tieneCoordenadas = partes.Length == 2 &&
                double.TryParse(partes[0], NumberStyles.Float, CultureInfo.InvariantCulture, out mural.latitud) &&
                double.TryParse(partes[1], NumberStyles.Float, CultureInfo.InvariantCulture, out mural.longitud);
            murales.Add(mural);
        }
        return murales.Count > 0;
    }

    void DibujarLista()
    {
        foreach (Transform hijo in contenidoLista) Destroy(hijo.gameObject);

        foreach (Mural mural in murales)
        {
            Mural m = mural;
            Button boton = CrearBoton(contenidoLista, TextoMural(m), new Color(0.93f, 0.93f, 0.93f), Color.black, () => AbrirFoto(m));
            boton.gameObject.AddComponent<LayoutElement>().minHeight = 150;
            m.etiqueta = boton.GetComponentInChildren<Text>();
            m.etiqueta.fontSize = 38;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contenidoLista);
        ActualizarDistancias();
    }

    string TextoMural(Mural m)
    {
        int fotos = ContarFotos(m.nombre);
        string prefijo = string.IsNullOrEmpty(m.orden) ? "" : m.orden + ". ";
        return prefijo + m.nombre + (fotos > 0 ? "  (" + fotos + (fotos == 1 ? " foto)" : " fotos)") : "");
    }

    int ContarFotos(string nombre)
    {
        return FotosDe(nombre).Length;
    }

    string CarpetaDe(string nombre)
    {
        return Path.Combine(carpeta, "Fotos", NombreArchivoSeguro(nombre));
    }

    // Fotos guardadas de un mural, de la más reciente a la más antigua.
    string[] FotosDe(string nombre)
    {
        string carpetaMural = CarpetaDe(nombre);
        if (!Directory.Exists(carpetaMural)) return new string[0];
        return Directory.GetFiles(carpetaMural, "*.jpg").OrderByDescending(f => f, StringComparer.Ordinal).ToArray();
    }

    // Las primeras versiones guardaban todas las fotos juntas; las pasa a la carpeta de su mural.
    void OrdenarFotosAntiguas()
    {
        foreach (string foto in Directory.GetFiles(carpeta, "*.jpg"))
        {
            string nombreArchivo = Path.GetFileName(foto);
            Match m = Regex.Match(nombreArchivo, @"^(.+?)_\d{4}-\d{2}-\d{2}_");
            if (!m.Success) continue;

            string destino = CarpetaDe(m.Groups[1].Value);
            Directory.CreateDirectory(destino);
            File.Move(foto, Path.Combine(destino, nombreArchivo));
        }
    }

    void AgregarOtroPunto()
    {
        string nombre = campoOtroPunto.text.Trim();
        if (nombre.Length == 0)
        {
            textoEstadoLista.text = "Escribe el nombre del punto.";
            return;
        }

        campoOtroPunto.text = "";
        AbrirFoto(new Mural { nombre = nombre });
    }

    void CopiarRegistro()
    {
        if (!File.Exists(archivoRegistro))
        {
            textoEstadoLista.text = "Todavía no hay fotos registradas.";
            return;
        }

        GUIUtility.systemCopyBuffer = File.ReadAllText(archivoRegistro);
        textoEstadoLista.text = "Registro copiado. Pégalo en una nota o en WhatsApp para guardarlo.";
    }

    // ---------- Cámara y fotos ----------

    void AbrirFoto(Mural mural)
    {
        muralActual = mural;
        textoTituloFoto.text = mural.nombre;
        textoEstadoFoto.text = "";
        campoNota.text = "";
        panelLista.SetActive(false);
        panelFoto.SetActive(true);
        ActualizarGps();
        StartCoroutine(IniciarCamara());
    }

    void VolverALista()
    {
        DetenerCamara();
        panelFoto.SetActive(false);
        panelLista.SetActive(true);
        DibujarLista();
    }

    IEnumerator IniciarCamara()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            float espera = 0f;
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && espera < 15f)
            {
                espera += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                textoEstadoFoto.text = "Se necesita permiso de cámara para tomar fotos.";
                yield break;
            }
        }
#endif
        WebCamDevice[] dispositivos = WebCamTexture.devices;
        if (dispositivos.Length == 0)
        {
            textoEstadoFoto.text = "No se encontró ninguna cámara.";
            yield break;
        }

        string nombreCamara = dispositivos[0].name;
        foreach (WebCamDevice d in dispositivos)
        {
            if (!d.isFrontFacing)
            {
                nombreCamara = d.name;
                break;
            }
        }

        camara = new WebCamTexture(nombreCamara, 1920, 1080);
        camara.Play();
        vistaCamara.texture = camara;

        // El ancho real se conoce solo cuando llega el primer cuadro.
        float limite = Time.unscaledTime + 5f;
        while (camara != null && camara.width <= 16 && Time.unscaledTime < limite) yield return null;
        AjustarVista();
    }

    void AjustarVista()
    {
        if (camara == null || camara.width <= 16) return;

        int angulo = camara.videoRotationAngle;
        bool girada = angulo == 90 || angulo == 270;
        float anchoImagen = girada ? camara.height : camara.width;
        float altoImagen = girada ? camara.width : camara.height;
        float escala = Mathf.Min(areaCamara.rect.width / anchoImagen, areaCamara.rect.height / altoImagen);

        RectTransform rt = vistaCamara.rectTransform;
        rt.sizeDelta = new Vector2(camara.width * escala, camara.height * escala);
        rt.localEulerAngles = new Vector3(0, 0, -angulo);
        rt.localScale = new Vector3(1, camara.videoVerticallyMirrored ? -1 : 1, 1);
    }

    void DetenerCamara()
    {
        if (camara != null)
        {
            camara.Stop();
            Destroy(camara);
            camara = null;
        }
        if (vistaCamara != null) vistaCamara.texture = null;
    }

    void TomarFoto()
    {
        if (camara == null || !camara.isPlaying || camara.width <= 16)
        {
            textoEstadoFoto.text = "La cámara todavía no está lista.";
            return;
        }

        Texture2D imagen = CopiarCuadroDerecho();
        byte[] jpg = imagen.EncodeToJPG(90);
        Destroy(imagen);

        DateTime ahora = DateTime.Now;
        bool conGps = HayGps;
        LocationInfo l = conGps ? Input.location.lastData : default(LocationInfo);

        string nombreArchivo = NombreArchivoSeguro(muralActual.nombre) + "_" + ahora.ToString("yyyy-MM-dd_HH-mm-ss") +
            (conGps ? "_" + Numero(l.latitude, 6) + "_" + Numero(l.longitude, 6) : "") + ".jpg";

        try
        {
            string carpetaMural = CarpetaDe(muralActual.nombre);
            Directory.CreateDirectory(carpetaMural);
            File.WriteAllBytes(Path.Combine(carpetaMural, nombreArchivo), jpg);
            AgregarAlRegistro(ahora, conGps, l, nombreArchivo);
        }
        catch (IOException e)
        {
            Debug.LogError("RegistroCampo: " + e.Message);
            textoEstadoFoto.text = "No se pudo guardar la foto: " + e.Message;
            return;
        }

        int fotos = ContarFotos(muralActual.nombre);
        string resumen = "Foto " + fotos + " guardada" + (conGps ? "" : " sin coordenadas");
        textoEstadoFoto.text = resumen + ".";
        campoNota.text = "";

        // Copia en la galería para poder pasar las fotos al computador.
        NativeGallery.SaveImageToGallery(jpg, PrefijoAlbum + NombreArchivoSeguro(muralActual.nombre), nombreArchivo, (exito, ruta) =>
        {
            if (textoEstadoFoto != null)
                textoEstadoFoto.text = resumen + (exito ? " y copiada a la galería." : ". No se pudo copiar a la galería; sigue guardada en la app.");
        });
    }

    Texture2D CopiarCuadroDerecho()
    {
        int w = camara.width, h = camara.height;
        Color32[] origen = camara.GetPixels32();
        int angulo = camara.videoRotationAngle;
        bool espejo = camara.videoVerticallyMirrored;
        bool girada = angulo == 90 || angulo == 270;
        int dw = girada ? h : w;
        int dh = girada ? w : h;
        Color32[] destino = new Color32[dw * dh];

        for (int y = 0; y < h; y++)
        {
            int fy = espejo ? h - 1 - y : y;
            for (int x = 0; x < w; x++)
            {
                int nx, ny;
                switch (angulo)
                {
                    case 90: nx = fy; ny = w - 1 - x; break;
                    case 180: nx = w - 1 - x; ny = h - 1 - fy; break;
                    case 270: nx = h - 1 - fy; ny = x; break;
                    default: nx = x; ny = fy; break;
                }
                destino[ny * dw + nx] = origen[y * w + x];
            }
        }

        Texture2D imagen = new Texture2D(dw, dh, TextureFormat.RGB24, false);
        imagen.SetPixels32(destino);
        imagen.Apply();
        return imagen;
    }

    void AgregarAlRegistro(DateTime fecha, bool conGps, LocationInfo l, string nombreArchivo)
    {
        bool nuevo = !File.Exists(archivoRegistro);
        StringBuilder linea = new StringBuilder();
        if (nuevo) linea.AppendLine("fecha,mural,latitud,longitud,precision_m,archivo,nota");
        linea.Append(fecha.ToString("yyyy-MM-dd HH:mm:ss")).Append(',')
            .Append(Csv(muralActual.nombre)).Append(',')
            .Append(conGps ? Numero(l.latitude, 6) : "").Append(',')
            .Append(conGps ? Numero(l.longitude, 6) : "").Append(',')
            .Append(conGps ? Numero(l.horizontalAccuracy, 0) : "").Append(',')
            .Append(Csv(nombreArchivo)).Append(',')
            .AppendLine(Csv(campoNota.text.Trim()));
        File.AppendAllText(archivoRegistro, linea.ToString());
    }

    // ---------- Ver fotos ----------

    void AbrirGaleria()
    {
        DetenerCamara();
        panelFoto.SetActive(false);
        panelGaleria.SetActive(true);
        textoTituloGaleria.text = muralActual.nombre;
        DibujarGaleria();
    }

    void CerrarGaleria()
    {
        DetenerCargaMiniaturas();
        LiberarMiniaturas();
        foreach (Transform hijo in contenidoGaleria) Destroy(hijo.gameObject);
        panelGaleria.SetActive(false);
        panelFoto.SetActive(true);
        textoEstadoFoto.text = "";
        StartCoroutine(IniciarCamara());
    }

    void DibujarGaleria()
    {
        DetenerCargaMiniaturas();
        LiberarMiniaturas();
        foreach (Transform hijo in contenidoGaleria) Destroy(hijo.gameObject);

        // Tres columnas que ocupen el ancho disponible en cualquier pantalla.
        Canvas.ForceUpdateCanvases();
        float ancho = ((RectTransform)contenidoGaleria.parent).rect.width;
        float celda = Mathf.Floor((ancho - 20f - 2f * rejillaGaleria.spacing.x) / 3f);
        rejillaGaleria.cellSize = new Vector2(celda, celda);

        string[] fotos = FotosDe(muralActual.nombre);
        textoEstadoGaleria.text = fotos.Length == 0
            ? "Este mural todavía no tiene fotos."
            : fotos.Length + (fotos.Length == 1 ? " foto de este mural. Toca para verla en grande." : " fotos de este mural. Toca una para verla en grande.");

        List<KeyValuePair<string, RawImage>> pendientes = new List<KeyValuePair<string, RawImage>>();
        foreach (string foto in fotos)
        {
            string ruta = foto;
            GameObject go = new GameObject("Miniatura", typeof(RectTransform), typeof(RawImage), typeof(Button));
            go.transform.SetParent(contenidoGaleria, false);
            RawImage imagen = go.GetComponent<RawImage>();
            imagen.color = new Color(0.3f, 0.3f, 0.3f);
            Button boton = go.GetComponent<Button>();
            boton.targetGraphic = imagen;
            boton.onClick.AddListener(() => AbrirGrande(ruta));
            pendientes.Add(new KeyValuePair<string, RawImage>(ruta, imagen));
        }

        cargaMiniaturas = StartCoroutine(CargarMiniaturas(pendientes));
    }

    // Carga una por cuadro y en tamaño reducido para no saturar la memoria en celulares de gama baja.
    IEnumerator CargarMiniaturas(List<KeyValuePair<string, RawImage>> pendientes)
    {
        foreach (KeyValuePair<string, RawImage> p in pendientes)
        {
            yield return null;

            Texture2D original = LeerFoto(p.Key);
            if (original == null) continue;

            float escala = 320f / Mathf.Min(original.width, original.height);
            int w = Mathf.Max(1, Mathf.RoundToInt(original.width * escala));
            int h = Mathf.Max(1, Mathf.RoundToInt(original.height * escala));
            RenderTexture temporal = RenderTexture.GetTemporary(w, h);
            Graphics.Blit(original, temporal);
            RenderTexture anterior = RenderTexture.active;
            RenderTexture.active = temporal;
            Texture2D miniatura = new Texture2D(w, h, TextureFormat.RGB24, false);
            miniatura.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            miniatura.Apply();
            RenderTexture.active = anterior;
            RenderTexture.ReleaseTemporary(temporal);
            Destroy(original);

            miniaturas.Add(miniatura);
            p.Value.texture = miniatura;
            p.Value.color = Color.white;
            // Recorta al centro para llenar la celda cuadrada sin deformar la foto.
            float lado = Mathf.Min(w, h);
            p.Value.uvRect = new Rect((1f - lado / w) / 2f, (1f - lado / h) / 2f, lado / w, lado / h);
        }
        cargaMiniaturas = null;
    }

    static Texture2D LeerFoto(string ruta)
    {
        try
        {
            Texture2D textura = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (textura.LoadImage(File.ReadAllBytes(ruta))) return textura;
            Destroy(textura);
        }
        catch (IOException e)
        {
            Debug.LogError("RegistroCampo: " + e.Message);
        }
        return null;
    }

    void DetenerCargaMiniaturas()
    {
        if (cargaMiniaturas != null)
        {
            StopCoroutine(cargaMiniaturas);
            cargaMiniaturas = null;
        }
    }

    void LiberarMiniaturas()
    {
        foreach (Texture2D t in miniaturas) Destroy(t);
        miniaturas.Clear();
    }

    void AbrirGrande(string ruta)
    {
        LiberarGrande();
        texturaGrande = LeerFoto(ruta);
        if (texturaGrande == null)
        {
            textoEstadoGaleria.text = "No se pudo abrir la foto.";
            return;
        }

        rutaGrande = ruta;
        confirmandoBorrado = false;
        textoBotonBorrar.text = "Borrar";
        textoInfoGrande.text = Path.GetFileNameWithoutExtension(ruta).Replace('_', ' ');
        panelGrande.SetActive(true);

        Canvas.ForceUpdateCanvases();
        float escala = Mathf.Min(areaGrande.rect.width / texturaGrande.width, areaGrande.rect.height / texturaGrande.height);
        imagenGrande.texture = texturaGrande;
        imagenGrande.rectTransform.sizeDelta = new Vector2(texturaGrande.width * escala, texturaGrande.height * escala);
    }

    void CerrarGrande()
    {
        panelGrande.SetActive(false);
        LiberarGrande();
    }

    void LiberarGrande()
    {
        if (imagenGrande != null) imagenGrande.texture = null;
        if (texturaGrande != null)
        {
            Destroy(texturaGrande);
            texturaGrande = null;
        }
    }

    void BorrarFotoGrande()
    {
        if (!confirmandoBorrado)
        {
            confirmandoBorrado = true;
            textoBotonBorrar.text = "¿Seguro? Toca otra vez";
            return;
        }

        string mensaje;
        try
        {
            File.Delete(rutaGrande);
            QuitarDelRegistro(Path.GetFileName(rutaGrande));
            mensaje = "Foto borrada de la app. La copia de la galería del celular no se borra.";
        }
        catch (IOException e)
        {
            Debug.LogError("RegistroCampo: " + e.Message);
            mensaje = "No se pudo borrar la foto: " + e.Message;
        }

        CerrarGrande();
        DibujarGaleria();
        textoEstadoGaleria.text = mensaje;
    }

    void QuitarDelRegistro(string nombreArchivo)
    {
        if (!File.Exists(archivoRegistro)) return;
        string campo = Csv(nombreArchivo);
        File.WriteAllLines(archivoRegistro, File.ReadAllLines(archivoRegistro).Where(l => !l.Contains(campo)).ToArray());
    }

    static string Csv(string valor)
    {
        return "\"" + valor.Replace("\"", "\"\"") + "\"";
    }

    static string Numero(double valor, int decimales)
    {
        return valor.ToString("F" + decimales, CultureInfo.InvariantCulture);
    }

    static string NombreArchivoSeguro(string nombre)
    {
        StringBuilder limpio = new StringBuilder();
        foreach (char c in nombre)
        {
            bool invalido = Path.GetInvalidFileNameChars().Contains(c) || c == '*' || c == '?' || c == '[' || c == ']';
            limpio.Append(invalido ? '-' : c);
        }
        return limpio.ToString().Trim();
    }

    // ---------- Interfaz ----------

    void ConstruirInterfaz()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler escalador = gameObject.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1080, 1920);
        escalador.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        panelLista = CrearPanel("PanelLista");
        panelFoto = CrearPanel("PanelFoto");
        panelFoto.SetActive(false);
        panelGaleria = CrearPanel("PanelGaleria");
        panelGaleria.SetActive(false);
        panelGrande = CrearPanel("PanelGrande");
        panelGrande.SetActive(false);
        ConstruirPanelLista();
        ConstruirPanelFoto();
        ConstruirPanelGaleria();
        ConstruirPanelGrande();
    }

    GameObject CrearPanel(string nombre)
    {
        GameObject panel = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        panel.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);
        Colocar(panel.GetComponent<RectTransform>(), 0, 0, 1, 1);
        return panel;
    }

    void ConstruirPanelLista()
    {
        RectTransform panel = panelLista.GetComponent<RectTransform>();
        Colocar(CrearTexto(panel, "Registrar murales", 60, Color.white, FontStyle.Bold).rectTransform, 0.04f, 0.925f, 0.96f, 0.985f);
        textoEstadoLista = CrearTexto(panel, "", 34, new Color(0.85f, 0.85f, 0.85f), FontStyle.Normal);
        Colocar(textoEstadoLista.rectTransform, 0.04f, 0.86f, 0.96f, 0.925f);
        textoGps = CrearTexto(panel, "Buscando GPS...", 32, new Color(0.7f, 0.9f, 0.7f), FontStyle.Normal);
        Colocar(textoGps.rectTransform, 0.04f, 0.825f, 0.96f, 0.86f);

        // Lista con desplazamiento.
        GameObject vista = new GameObject("Vista", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        vista.transform.SetParent(panel, false);
        Colocar(vista.GetComponent<RectTransform>(), 0.04f, 0.24f, 0.96f, 0.815f);
        vista.GetComponent<Image>().color = new Color(1, 1, 1, 0.05f);
        vista.GetComponent<Mask>().showMaskGraphic = true;

        GameObject contenido = new GameObject("Contenido", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contenido.transform.SetParent(vista.transform, false);
        contenidoLista = contenido.GetComponent<RectTransform>();
        contenidoLista.anchorMin = new Vector2(0, 1);
        contenidoLista.anchorMax = new Vector2(1, 1);
        contenidoLista.pivot = new Vector2(0.5f, 1);
        contenidoLista.offsetMin = contenidoLista.offsetMax = Vector2.zero;
        VerticalLayoutGroup grupo = contenido.GetComponent<VerticalLayoutGroup>();
        grupo.spacing = 14;
        grupo.padding = new RectOffset(10, 10, 10, 10);
        grupo.childForceExpandHeight = false;
        grupo.childControlHeight = true;
        grupo.childControlWidth = true;
        contenido.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = vista.GetComponent<ScrollRect>();
        scroll.content = contenidoLista;
        scroll.viewport = vista.GetComponent<RectTransform>();
        scroll.horizontal = false;

        // Otro punto que no esté en la lista.
        campoOtroPunto = CrearCampo(panel, "Otro punto: escribe el nombre");
        Colocar(campoOtroPunto.GetComponent<RectTransform>(), 0.04f, 0.16f, 0.72f, 0.225f);
        Button agregar = CrearBoton(panel, "Fotos", new Color(0.2f, 0.45f, 0.8f), Color.white, AgregarOtroPunto);
        Colocar(agregar.GetComponent<RectTransform>(), 0.74f, 0.16f, 0.96f, 0.225f);

        Button copiar = CrearBoton(panel, "Copiar registro", new Color(0.3f, 0.3f, 0.3f), Color.white, CopiarRegistro);
        Colocar(copiar.GetComponent<RectTransform>(), 0.04f, 0.05f, 0.49f, 0.14f);
        Button cerrar = CrearBoton(panel, "Cerrar", new Color(0.3f, 0.3f, 0.3f), Color.white, () => Destroy(gameObject));
        Colocar(cerrar.GetComponent<RectTransform>(), 0.51f, 0.05f, 0.96f, 0.14f);
    }

    void ConstruirPanelFoto()
    {
        RectTransform panel = panelFoto.GetComponent<RectTransform>();
        textoTituloFoto = CrearTexto(panel, "", 46, Color.white, FontStyle.Bold);
        Colocar(textoTituloFoto.rectTransform, 0.04f, 0.91f, 0.96f, 0.985f);

        GameObject area = new GameObject("AreaCamara", typeof(RectTransform), typeof(Image), typeof(Mask));
        area.transform.SetParent(panel, false);
        areaCamara = area.GetComponent<RectTransform>();
        Colocar(areaCamara, 0.04f, 0.33f, 0.96f, 0.905f);
        area.GetComponent<Image>().color = Color.black;

        GameObject vista = new GameObject("VistaCamara", typeof(RectTransform), typeof(RawImage));
        vista.transform.SetParent(area.transform, false);
        vistaCamara = vista.GetComponent<RawImage>();
        RectTransform rt = vistaCamara.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // Reutiliza un texto de GPS propio de esta pantalla.
        Text gpsFoto = CrearTexto(panel, "Buscando GPS...", 32, new Color(0.7f, 0.9f, 0.7f), FontStyle.Normal);
        Colocar(gpsFoto.rectTransform, 0.04f, 0.29f, 0.96f, 0.325f);
        gpsFoto.gameObject.AddComponent<RegistroCampoEspejoTexto>().origen = textoGps;

        campoNota = CrearCampo(panel, "Nota (opcional): estado, autor, historia...");
        Colocar(campoNota.GetComponent<RectTransform>(), 0.04f, 0.21f, 0.96f, 0.285f);

        textoEstadoFoto = CrearTexto(panel, "", 34, new Color(1f, 0.9f, 0.6f), FontStyle.Normal);
        Colocar(textoEstadoFoto.rectTransform, 0.04f, 0.145f, 0.96f, 0.205f);

        Button tomar = CrearBoton(panel, "Tomar foto", new Color(0.2f, 0.6f, 0.3f), Color.white, TomarFoto);
        Colocar(tomar.GetComponent<RectTransform>(), 0.04f, 0.035f, 0.42f, 0.135f);
        Button ver = CrearBoton(panel, "Ver fotos", new Color(0.2f, 0.45f, 0.8f), Color.white, AbrirGaleria);
        Colocar(ver.GetComponent<RectTransform>(), 0.44f, 0.035f, 0.72f, 0.135f);
        Button volver = CrearBoton(panel, "Volver", new Color(0.3f, 0.3f, 0.3f), Color.white, VolverALista);
        Colocar(volver.GetComponent<RectTransform>(), 0.74f, 0.035f, 0.96f, 0.135f);
    }

    void ConstruirPanelGaleria()
    {
        RectTransform panel = panelGaleria.GetComponent<RectTransform>();
        textoTituloGaleria = CrearTexto(panel, "", 46, Color.white, FontStyle.Bold);
        Colocar(textoTituloGaleria.rectTransform, 0.04f, 0.91f, 0.96f, 0.985f);
        textoEstadoGaleria = CrearTexto(panel, "", 34, new Color(0.85f, 0.85f, 0.85f), FontStyle.Normal);
        Colocar(textoEstadoGaleria.rectTransform, 0.04f, 0.85f, 0.96f, 0.91f);

        GameObject vista = new GameObject("Vista", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        vista.transform.SetParent(panel, false);
        Colocar(vista.GetComponent<RectTransform>(), 0.04f, 0.15f, 0.96f, 0.84f);
        vista.GetComponent<Image>().color = new Color(1, 1, 1, 0.05f);

        GameObject contenido = new GameObject("Contenido", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        contenido.transform.SetParent(vista.transform, false);
        contenidoGaleria = contenido.GetComponent<RectTransform>();
        contenidoGaleria.anchorMin = new Vector2(0, 1);
        contenidoGaleria.anchorMax = new Vector2(1, 1);
        contenidoGaleria.pivot = new Vector2(0.5f, 1);
        contenidoGaleria.offsetMin = contenidoGaleria.offsetMax = Vector2.zero;
        rejillaGaleria = contenido.GetComponent<GridLayoutGroup>();
        rejillaGaleria.padding = new RectOffset(10, 10, 10, 10);
        rejillaGaleria.spacing = new Vector2(12, 12);
        rejillaGaleria.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        rejillaGaleria.constraintCount = 3;
        contenido.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = vista.GetComponent<ScrollRect>();
        scroll.content = contenidoGaleria;
        scroll.viewport = vista.GetComponent<RectTransform>();
        scroll.horizontal = false;

        Button volver = CrearBoton(panel, "Volver a la cámara", new Color(0.3f, 0.3f, 0.3f), Color.white, CerrarGaleria);
        Colocar(volver.GetComponent<RectTransform>(), 0.04f, 0.035f, 0.96f, 0.135f);
    }

    void ConstruirPanelGrande()
    {
        RectTransform panel = panelGrande.GetComponent<RectTransform>();
        panelGrande.GetComponent<Image>().color = Color.black;

        GameObject area = new GameObject("AreaGrande", typeof(RectTransform));
        area.transform.SetParent(panel, false);
        areaGrande = area.GetComponent<RectTransform>();
        Colocar(areaGrande, 0.02f, 0.2f, 0.98f, 0.985f);

        GameObject vista = new GameObject("ImagenGrande", typeof(RectTransform), typeof(RawImage));
        vista.transform.SetParent(area.transform, false);
        imagenGrande = vista.GetComponent<RawImage>();
        RectTransform rt = imagenGrande.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        textoInfoGrande = CrearTexto(panel, "", 30, new Color(0.85f, 0.85f, 0.85f), FontStyle.Normal);
        Colocar(textoInfoGrande.rectTransform, 0.04f, 0.145f, 0.96f, 0.2f);

        Button borrar = CrearBoton(panel, "Borrar", new Color(0.65f, 0.2f, 0.2f), Color.white, BorrarFotoGrande);
        Colocar(borrar.GetComponent<RectTransform>(), 0.04f, 0.035f, 0.49f, 0.135f);
        textoBotonBorrar = borrar.GetComponentInChildren<Text>();
        textoBotonBorrar.fontSize = 36;
        Button volver = CrearBoton(panel, "Volver", new Color(0.3f, 0.3f, 0.3f), Color.white, CerrarGrande);
        Colocar(volver.GetComponent<RectTransform>(), 0.51f, 0.035f, 0.96f, 0.135f);
    }

    static void Colocar(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    Text CrearTexto(Transform padre, string contenido, int tamano, Color color, FontStyle estilo)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(padre, false);
        Text texto = go.GetComponent<Text>();
        texto.font = fuente;
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.color = color;
        texto.fontStyle = estilo;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Truncate;
        texto.raycastTarget = false;
        return texto;
    }

    Button CrearBoton(Transform padre, string contenido, Color fondo, Color colorTexto, UnityEngine.Events.UnityAction accion)
    {
        GameObject go = new GameObject("Boton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);
        go.GetComponent<Image>().color = fondo;
        Text texto = CrearTexto(go.transform, contenido, 42, colorTexto, FontStyle.Bold);
        Colocar(texto.rectTransform, 0.02f, 0.02f, 0.98f, 0.98f);
        Button boton = go.GetComponent<Button>();
        boton.onClick.AddListener(accion);
        return boton;
    }

    InputField CrearCampo(Transform padre, string indicacion)
    {
        GameObject go = new GameObject("Campo", typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(padre, false);
        go.GetComponent<Image>().color = Color.white;

        Text pista = CrearTexto(go.transform, indicacion, 34, new Color(0.45f, 0.45f, 0.45f), FontStyle.Italic);
        pista.alignment = TextAnchor.MiddleLeft;
        Colocar(pista.rectTransform, 0.02f, 0f, 0.98f, 1f);

        Text texto = CrearTexto(go.transform, "", 36, Color.black, FontStyle.Normal);
        texto.alignment = TextAnchor.MiddleLeft;
        texto.supportRichText = false;
        Colocar(texto.rectTransform, 0.02f, 0f, 0.98f, 1f);

        InputField campo = go.GetComponent<InputField>();
        campo.textComponent = texto;
        campo.placeholder = pista;
        campo.targetGraphic = go.GetComponent<Image>();
        return campo;
    }
}

// Muestra en otra parte de la interfaz el mismo texto que un Text de origen.
public class RegistroCampoEspejoTexto : MonoBehaviour
{
    public Text origen;
    Text propio;

    void Awake() { propio = GetComponent<Text>(); }

    void Update()
    {
        if (origen != null && propio.text != origen.text) propio.text = origen.text;
    }
}
