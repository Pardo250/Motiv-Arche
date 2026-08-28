using System.Collections.Generic;

public class Contenido
{
    private string descripcion;
    private string contenido;
    private string tipo;
    private List<string> etiquetas;

    public Contenido(string descripcion,string contenido,string tipo,List<string> etiquetas)
    {
        this.descripcion = descripcion;
        this.contenido = contenido;
        this.tipo = tipo;
        this.etiquetas = etiquetas;
    }

    public string getDescripcion()
    {
        return descripcion;
    }

    public string getContenido()
    {
        return contenido;
    }

    public string getTipo()
    {
        return tipo;
    }

    public List<string> getEtiquetas()
    {
        return etiquetas;
    }

    public void setDescripcion(string descripcion)
    {
        this.descripcion = descripcion;
    }

    public void setContenido(string contenido)
    {
        this.contenido = contenido;
    }

    public void setTipo(string tipo)
    {
        this.tipo = tipo;
    }

    public void setEtiquetas(List<string> etiquetas)
    {
        this.etiquetas = etiquetas;
    }
}
