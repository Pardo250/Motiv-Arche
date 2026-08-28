using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public bool mapaActivo = true;
    public GameObject Minimap;
    public GameObject minimask;
    public GameObject minirenderer;
    public Button buttonBuscar;
    public GameObject poiNameSelected;
    public GameObject previewButton;
    public GameObject nextButton;
    public GameObject routeOptions;


    public void botonMapa()
    {
        mapaActivo = !mapaActivo;
        if (mapaActivo)
        {
            
            //buttonBuscar.gameObject.SetActive(true);
            //poiNameSelected.gameObject.SetActive(false);
            //previewButton.gameObject.SetActive(false);
            //nextButton.gameObject.SetActive(false);
            
            //poiNameSelected.GetComponent<TextMeshPro>().color = new Color32(0, 0, 0, 255);
            //previewButton.GetComponent<RawImage>().color = new Color32(0, 0, 0, 255);
            //nextButton.GetComponent<RawImage>().color = new Color32(0, 0, 0, 255);
            routeOptions.GetComponent<RawImage>().color = new Color32(79, 79, 79, 255);
            Minimap.GetComponent<RawImage>().color = new Color32(255, 255, 255, 255);
            minimask.GetComponent<RawImage>().color = new Color32(255, 255, 255, 255);
            minirenderer.GetComponent<RawImage>().color = new Color32(255, 255, 255, 255);
        }
        else
        {
            /*
            buttonBuscar.gameObject.SetActive(false);
            poiNameSelected.gameObject.SetActive(true); 
            previewButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
            */

            

            if (ContenidoGPS.posicion == 0)
            {
                previewButton.gameObject.SetActive(false);
            }else if (ContenidoGPS.posicion == (ContenidoGPS.totalElementosRuta) || ContenidoGPS.totalElementosRuta==0)
            {
                nextButton.gameObject.SetActive(false);
            }

            if(ContenidoGPS.posicion==0 && ContenidoGPS.totalElementosRuta == 0)
            {
                poiNameSelected.gameObject.SetActive(false);
            }

            //poiNameSelected.GetComponent<TextMeshProUGUI>().color = new Color32(255,255, 255, 255);
            //previewButton.GetComponent<RawImage>().color = new Color32(255, 255, 255, 255);
            //nextButton.GetComponent<RawImage>().color = new Color32(255, 255, 255, 255);
            routeOptions.GetComponent<RawImage>().color = new Color32(79, 79, 79, 255);
            Minimap.GetComponent<RawImage>().color = new Color32(255, 255, 255, 0);
            minimask.GetComponent<RawImage>().color = new Color32(255, 255, 255, 0);
            minirenderer.GetComponent<RawImage>().color = new Color32(255, 255, 255, 0);
        }
        
    }
}
