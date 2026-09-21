using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using static CloudContentManager;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

// Escanea codigos QR con la camara del dispositivo y activa el contenido del
// punto de interes cuyo nombre este codificado en el QR, reutilizando el
// mismo flujo de contenidos que ya usa la activacion por GPS
// (ContenidoGPS.obtenerContenidosGPS).
public class QrScanner : MonoBehaviour
{
    public ContenidoGPS contenidoGPS;

    public GameObject panelEscanerQR;
    public RawImage previewCamara;
    public TextMeshProUGUI textoEstado;

    [Tooltip("La mayoria de dispositivos entregan la imagen de la camara invertida verticalmente respecto a lo que espera el lector de QR. Si el escaner no detecta codigos validos, prueba desactivando esta opcion.")]
    public bool invertirFilas = true;

    [Tooltip("Segundos entre cada intento de lectura del cuadro de la camara.")]
    public float intervaloEscaneo = 0.35f;

    WebCamTexture webCamTexture;
    IBarcodeReader lectorQR;
    Coroutine escaneoRutina;
    bool procesando = false;

    void Awake()
    {
        lectorQR = new BarcodeReader
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
            }
        };
    }

    public void AbrirEscanerQR()
    {
        if (panelEscanerQR != null) panelEscanerQR.SetActive(true);
        procesando = false;
        StartCoroutine(IniciarCamara());
    }

    public void CerrarEscanerQR()
    {
        DetenerCamara();
        if (panelEscanerQR != null) panelEscanerQR.SetActive(false);
    }

    IEnumerator IniciarCamara()
    {
        if (textoEstado != null) textoEstado.text = "Solicitando permiso de camara...";

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            float espera = 0f;
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && espera < 15f)
            {
                espera += Time.deltaTime;
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                if (textoEstado != null) textoEstado.text = "Se necesita permiso de camara para escanear el codigo QR.";
                yield break;
            }
        }
#endif

        if (WebCamTexture.devices.Length == 0)
        {
            if (textoEstado != null) textoEstado.text = "No se encontro ninguna camara en el dispositivo.";
            yield break;
        }

        webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 640, 480);
        webCamTexture.Play();

        if (previewCamara != null) previewCamara.texture = webCamTexture;

        if (textoEstado != null) textoEstado.text = "Apunta la camara al codigo QR del punto de interes.";

        escaneoRutina = StartCoroutine(EscanearLoop());
    }

    IEnumerator EscanearLoop()
    {
        WaitForSeconds espera = new WaitForSeconds(intervaloEscaneo);
        while (true)
        {
            yield return espera;

            if (webCamTexture == null || !webCamTexture.isPlaying || procesando)
            {
                continue;
            }

            IntentarDecodificar();
        }
    }

    void IntentarDecodificar()
    {
        int ancho = webCamTexture.width;
        int alto = webCamTexture.height;
        Color32[] pixeles = webCamTexture.GetPixels32();
        byte[] rawRGBA = new byte[pixeles.Length * 4];

        for (int y = 0; y < alto; y++)
        {
            int filaOrigen = invertirFilas ? (alto - 1 - y) : y;
            int inicioOrigen = filaOrigen * ancho;
            int inicioDestino = y * ancho * 4;

            for (int x = 0; x < ancho; x++)
            {
                Color32 p = pixeles[inicioOrigen + x];
                int o = inicioDestino + x * 4;
                rawRGBA[o] = p.r;
                rawRGBA[o + 1] = p.g;
                rawRGBA[o + 2] = p.b;
                rawRGBA[o + 3] = p.a;
            }
        }

        Result resultado = lectorQR.Decode(rawRGBA, ancho, alto, RGBLuminanceSource.BitmapFormat.RGBA32);

        if (resultado != null && !string.IsNullOrEmpty(resultado.Text))
        {
            procesando = true;
            ProcesarCodigoQR(resultado.Text.Trim());
        }
    }

    void ProcesarCodigoQR(string poiName)
    {
        bool encontrado = false;
        foreach (GPSContenidos poi in Main.contenidosHotspot)
        {
            if (poi.name != null && poi.name.Trim().Equals(poiName))
            {
                encontrado = true;
                break;
            }
        }

        if (!encontrado)
        {
            if (textoEstado != null) textoEstado.text = "El codigo QR no corresponde a ningun punto de interes conocido (\"" + poiName + "\"). Intenta de nuevo.";
            procesando = false;
            return;
        }

        if (textoEstado != null) textoEstado.text = "Punto de interes encontrado: " + poiName;

        DetenerCamara();
        if (panelEscanerQR != null) panelEscanerQR.SetActive(false);

        if (contenidoGPS != null)
        {
            contenidoGPS.obtenerContenidosGPS(poiName);
        }
        else
        {
            Debug.LogError("QrScanner: falta asignar la referencia a ContenidoGPS en el inspector.");
        }
    }

    void DetenerCamara()
    {
        if (escaneoRutina != null)
        {
            StopCoroutine(escaneoRutina);
            escaneoRutina = null;
        }

        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }

        procesando = false;
    }

    void OnDisable()
    {
        DetenerCamara();
    }
}
