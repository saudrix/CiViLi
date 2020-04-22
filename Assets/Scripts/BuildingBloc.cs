using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class BuildingBloc
{
    #region Attributes
    private string materialName;
    private bool isSelected = false;
    private bool isStructural = true;

    // attributs statique permettant d'établir le lien entre les données et Unity
    public static Hashtable accessTable = new Hashtable();
    #endregion

    #region Properties
    // Properties relatives to display
    public string MaterialName
    {
        get{ return materialName; }
        set{
            materialName = value;
            SetMaterial(value);
        }
    }
    public GameObject RepresentedObject { get; set; }
    // Properties reliatives to calculation
    public List<BuildingBloc> On { get; set; }
    public List<BuildingBloc> Under { get; set; }

    public float VolumicMass { get; set; }
    public float Volume { get; set; }

    public float P { get; set; } // Poid propre
    public float G { get; set; } // Charges Globales
    public float Q { get; set; } // Charges d'exploitation

    public float ELS { get { return G + Q; } } // Etat limite standard
    public float ELU { get { return (float)(1.35 * G + 1.5 * Q); } } // Etat limite ultime

    #endregion

    #region Constructor
    public BuildingBloc(GameObject representedObject, string materialName)
    {
        RepresentedObject = representedObject;
        MaterialName = materialName;
        On = new List<BuildingBloc>();
        Under = new List<BuildingBloc>();
    }
    #endregion

    #region Methods
    // permet d'assigner le matéraux aux mesh encapsulé
    public void SetMaterial(string matName)
    {
        MeshRenderer renderer = RepresentedObject.GetComponent<MeshRenderer>();
        renderer.material = GetUnityMaterial(matName);
        float transparancyValue = isStructural ? 1f : 0.2f  ;
        renderer.material.SetFloat("transparency", transparancyValue);
    }
    
    // permet de charger un matéraux depuis Unity
    public Material GetUnityMaterial(string materialName)
    {
        return (Material)Resources.Load("Materials/" + materialName, typeof(Material));
    }
    
    public void ToggleStructural()
    {
        isStructural = !isStructural;
        SetMaterial("selected");
    }

    public void ToggleStructural(bool structural)
    {
        isStructural = structural;
        SetMaterial("MaterialName");
    }

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        if (isSelected) SetMaterial("selected"); // Si on selectionne on assigne un matériaux spécifique
        else SetMaterial(MaterialName); // en déselctionnant au retourne au matériaux choisi ou par défaut
    }

    // renvoie le volme signé d'un triangle
    float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var v321 = p3.x * p2.y * p1.z;
        var v231 = p2.x * p3.y * p1.z;
        var v312 = p3.x * p1.y * p2.z;
        var v132 = p1.x * p3.y * p2.z;
        var v213 = p2.x * p1.y * p3.z;
        var v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    // calcul le volume du mesh encapsuler
    public void SetVolume()
    {
        Mesh mesh = RepresentedObject.GetComponent<MeshFilter>().mesh;
        List<float> _signedVolume = new List<float>();
        int[] _triangles = mesh.triangles;
        Vector3[] _vertices = mesh.vertices;
        Debug.Log(_vertices.Length);
        for (int i = 0; i < _triangles.Length; i += 3)
        {
            _signedVolume.Add(SignedVolumeOfTriangle(_vertices[_triangles[i]], _vertices[_triangles[i + 1]], _vertices[_triangles[i + 2]]));
        }
        Volume = Math.Abs(_signedVolume.Sum());
        Debug.Log(Volume);
    }

    // calcul du poids propre
    public void ComputeSelfWeight()
    {
        SetVolume();
        const float g = 9.81f;
        P = g * Volume * VolumicMass;
    }

    // Calcule de la charge portée
    public void ComputeLiftedWeight()
    {
        float additionnalWeight = 0;
        foreach (BuildingBloc lifted in Under)
        {
            additionnalWeight += lifted.G / lifted.On.Count;
        }
        G = P + additionnalWeight;
    }
    #endregion
}
