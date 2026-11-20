using UnityEngine;

// Agregar a cualquier objeto que el invitado no deba ver (p. ej. CrearPoiOption y GestionarPoiOption en MenuScene).
public class OcultarParaInvitado : MonoBehaviour
{
    void Awake()
    {
        if (Main.esInvitado)
        {
            gameObject.SetActive(false);
        }
    }
}
