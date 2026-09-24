using UnityEngine;

public class QuickLevelGenerator : MonoBehaviour
{
    [Header("Dimensiones del Nivel")]
    public float width = 18f;
    public float height = 10f;

    [Header("Opciones")]
    public bool generarObstaculos = true;

    void Awake()
    {
        GenerateBoxArena();
    }

    void GenerateBoxArena()
    {
        GameObject arenaParent = new GameObject("ArenaMurosExpress");

        // 1. Crear los 4 muros perimetrales
        CreateWindowWall(arenaParent, new Vector3(0, height / 2f, 0), new Vector3(width, 1f, 1f), "Muro_Arriba");
        CreateWindowWall(arenaParent, new Vector3(0, -height / 2f, 0), new Vector3(width, 1f, 1f), "Muro_Abajo");
        CreateWindowWall(arenaParent, new Vector3(-width / 2f, 0, 0), new Vector3(1f, height, 1f), "Muro_Izquierda");
        CreateWindowWall(arenaParent, new Vector3(width / 2f, 0, 0), new Vector3(1f, height, 1f), "Muro_Derecha");

        // 2. Generar plataformas internas opcionales
        if (generarObstaculos)
        {
            CreateWindowWall(arenaParent, new Vector3(0, -2f, 0), new Vector3(6f, 0.5f, 1f), "Plataforma_Centro");
            CreateWindowWall(arenaParent, new Vector3(-5f, 1f, 0), new Vector3(3f, 0.5f, 1f), "Plataforma_Izquierda");
            CreateWindowWall(arenaParent, new Vector3(5f, 1f, 0), new Vector3(3f, 0.5f, 1f), "Plataforma_Derecha");
        }
    }

    void CreateWindowWall(GameObject parent, Vector3 pos, Vector3 scale, string nombreObjeto)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = nombreObjeto;
        wall.transform.SetParent(parent.transform);
        wall.transform.position = pos;
        wall.transform.localScale = scale;

        // AQUÍ ESTÁ LA MAGIA: Destruye el 3D AL INSTANTE para que no joda al 2D. No borra nada más.
        BoxCollider col3D = wall.GetComponent<BoxCollider>();
        if (col3D != null)
        {
            DestroyImmediate(col3D);
        }

        // Le añadimos el collider 2D limpio
        wall.AddComponent<BoxCollider2D>();

        // Solo le damos color para que se vea, la malla queda intacta
        Renderer rend = wall.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = new Color(0.15f, 0.15f, 0.15f);
        }
    }
}