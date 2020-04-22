using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Classe statique de parsage de fichiers .obj
/// </summary>
public static class Parser
{
    #region Poperties
    private static int CurrentPointsCountOffset { get; set; }
    private static List<Vector3> CurrentPoints { get; set; }
    private static List<int> Faces { get; set; }
    private static int CurrentOffset { get; set; }

    // macimum et minimum 
    public static float MaxX { get; set; }
    public static float MinX { get; set; }
    public static float MaxY { get; set; }
    public static float MinY { get; set; }
    public static float MaxZ { get; set; }
    public static float MinZ { get; set; }
    #endregion

    #region Methods
    public static  List<GameObject> ParseFile(string filePath)
    {
        MaxX = float.MinValue;
        MinX = float.MaxValue;
        MaxY = float.MinValue;
        MinY = float.MaxValue;
        MaxZ = float.MinValue;
        MinZ = float.MaxValue;

        // Création de la liste de retour
        List<GameObject> result = new List<GameObject>();
        // Initialisation des propriétés statiques
        CurrentPoints = new List<Vector3>();
        Faces = new List<int>();
        CurrentOffset = 0;
        
        int objCount = 0;

        string currentObjectName = "";
        
        string[] dataLines = File.ReadAllLines(filePath); // On récupère les lignes du fichier de données

        // boucle de récupération des données
        foreach(string line in dataLines)
        {
            string[] data = line.Split(' ');
            switch (data[0])
            {
                // ajout d'un objet
                case "g":
                case "o":
                    if(objCount != 0)
                    {
                        result.Add(CreateGameObject(currentObjectName));
                        // Décalage de l'offset
                        CurrentOffset += CurrentPoints.Count;
                        // Remise a zero des listes décrivant l'objet en cours (changement d'objet)
                        CurrentPoints = new List<Vector3>();
                        Faces = new List<int>();
                    }
                    objCount++;
                    currentObjectName = data[1];
                    break;
                    // ajout d'un vertex
                case "v":
                    AddVertex(data);
                    break;
                    // ajout d'une face
                case "f":
                    AddFace(data);
                    break;
            }
        }

        result.Add(CreateGameObject(currentObjectName));
        return result;
    }

    static void AddVertex(string[] data)
    {
        // Conversion des chaines en flottants
        float x = Single.Parse(data[1], CultureInfo.InvariantCulture);
        float y = Single.Parse(data[2], CultureInfo.InvariantCulture);
        float z = Single.Parse(data[3], CultureInfo.InvariantCulture);

        // Recherche des maxs et mins
        if (x > MaxX) MaxX = x;
        if (x < MinX) MinX = x;
        if (y > MaxY) MaxY = y;
        if (y < MinY) MinY = y;
        if (z > MaxZ) MaxZ = z;
        if (z < MinZ) MinZ = z;

        // Creation du Vertex
        Vector3 vertex = new Vector3(x, y, z);

        CurrentPoints.Add(vertex);
    }

    // création des données d'une face
    static void AddFace(string[] data)
    {
        for(int i = 1; i < data.Length; i++)
        {
            string[] faceData = data[i].Split('/');
            Faces.Add(Convert.ToInt32(faceData[0]) - (CurrentOffset + 1));
        }
    }

    // méthode d'affichage générique de liste pour tests
    static void Display<T>(List<T> list)
    {
        string result = "";
        foreach(var obj in list)
        {
            result += (obj.ToString()) + " - ";
        }
    }

    // Création de donnée factices d'UV
    static Vector2[] CreateUVs()
    {
        Vector2[] result = new Vector2[CurrentPoints.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new Vector2(CurrentPoints[i].x, CurrentPoints[i].z);
        }
        return result;
    }

    // création d'un GameObjet pour Unity
    static GameObject CreateGameObject(string name)
    {
        Vector2[] uvs = CreateUVs();
        
        // On crée le mesh representé par le fichier
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.name = name;
        // On insère les données dans le mesh
        mesh.vertices = CurrentPoints.ToArray();
        mesh.uv = uvs;
        mesh.triangles = Faces.ToArray();
        mesh.RecalculateNormals();

        // On crée le gameObject et ses components nécessaires
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>(); // permet de donner un mesh à l'objet
        obj.AddComponent<MeshRenderer>(); // permet d'afficher le mesh qui constitue l'objet
        obj.AddComponent<MeshCollider>(); // permet de donner des pp de collision au mesh

        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        obj.name = name;

        return obj;
    }
    #endregion
}

