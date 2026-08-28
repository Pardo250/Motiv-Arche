using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RutaSeleccionada
{
    private int id;
    private float ordenRuta;
    private float lat;
    private float lng;
    private string nombrePunto;
    private bool incluirPuntoRuta;

    public RutaSeleccionada(int id, float ordenRuta, float lat, float lng, string nombrePunto, bool incluirPuntoRuta)
    {
        this.id = id;
        this.ordenRuta = ordenRuta;
        this.lat = lat;
        this.lng = lng;
        this.nombrePunto = nombrePunto;
        this.incluirPuntoRuta = incluirPuntoRuta;
    }

    public int GetId()
    {
        return this.id;
    }

    public float GetOrdenRuta()
    {
        return this.ordenRuta;
    }

    public float GetLat()
    {
        return this.lat;
    }

    public float GetLng()
    {
        return this.lng;
    }

    public string GetNombrePunto()
    {
        return this.nombrePunto;
    }

    public bool GetIncluirPuntoRuta()
    {
        return this.incluirPuntoRuta;
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    public void SetOrdenRuta(float ordenRuta)
    {
        this.ordenRuta = ordenRuta;
    }

    public void SetLat(float lat)
    {
        this.lat = lat;
    }

    public void SetLng(float lng)
    {
        this.lng = lng;
    }

    public void SetNombrePunto(string nombrePunto)
    {
        this.nombrePunto = nombrePunto;
    }

    public void SetIncluirPuntoRuta(bool incluirPuntoRuta)
    {
        this.incluirPuntoRuta = incluirPuntoRuta;
    }

}
