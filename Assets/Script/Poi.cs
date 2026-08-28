using System.Collections.Generic;

public class Poi 
{
    private string nombre;
    private bool visualizacion;
    private bool edicion;
    private List<string> etiquetas;
    private List<string> imagenesAR;
    private double latitud;
    private double longitud;
    private List<Contenido> contenidos;
    private string email;

    public Poi(string nombre, bool visualizacion,bool edicion,List<string> etiquetas, List<string> imagenesAR, double latitud, double longitud,List<Contenido> contenidos,string email)
    {
        this.nombre = nombre;
        this.visualizacion = visualizacion;
        this.edicion = edicion;
        this.etiquetas = etiquetas;
        this.imagenesAR = imagenesAR;
        this.latitud = latitud;
        this.longitud = longitud;
        this.contenidos = contenidos;
        this.email = email;
    }

    public string getNombre()
    {
        return nombre;
    }

    public bool getVisualizacion()
    {
        return visualizacion;
    }

    public bool getEdicion()
    {
        return edicion;
    }

    public List<string> getEtiquetas()
    {
        return etiquetas;
    }

    public List<string> getImagenesAR()
    {
        return imagenesAR;
    }

    public double getLatitud()
    {
        return latitud;
    }

    public double getLongitud()
    {
        return longitud;
    }

    public List<Contenido> getContenidos()
    {
        return contenidos;
    }

    public string getEmail()
    {
        return email;
    }

    public void setNombre(string nombre)
    {
        this.nombre = nombre;
    }

    public void setVisualizacion(bool visualizacion)
    {
        this.visualizacion=visualizacion;
    }

    public void setEdicion(bool edicion)
    {
        this.edicion = edicion;
    }

    public void setEtiquetas(List<string> etiquetas)
    {
        this.etiquetas = etiquetas;
    }

    public void setImagenesAR(List<string> imagenesAR)
    {
        this.imagenesAR=imagenesAR;
    }

    public void setLatitud(double latitud)
    {
        this.latitud = latitud;
    }

    public void setLongitud(double longitud)
    {
        this.longitud = longitud;
    }

    public void setContenidos(List<Contenido> contenidos)
    {
        this.contenidos = contenidos;
    }

    public void setEmail(string email)
    {
        this.email = email;
    }
}
