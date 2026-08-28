
using UnityEngine;
using Vuforia;

public class Flash : MonoBehaviour
{
    public bool linterna = false;
    
    void Start()
    {
        Debug.Log("flash apagado");
        //CameraDevice.Instance.SetFlashTorchMode(false);
        VuforiaBehaviour.Instance.CameraDevice.SetFlash(false);
    }
    public void botonLinterna()
    {
        linterna = !linterna;
        Debug.Log("flash:" + linterna);
        VuforiaBehaviour.Instance.CameraDevice.SetFlash(linterna);
        //CameraDevice.Instance.SetFlashTorchMode(linterna);
    }
}