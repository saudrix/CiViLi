using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

enum Section { carre, rectangle, I}

class Poutre : BuildingBloc
{
    #region Attributes
    private float _width;
    private float _height;
    private float I;
    private float _epaisseurSemelle;
    private float _epaisseurAme;

    private float _contrainteTraction;

    public Section _sectionType = new Section();
    #endregion

    #region Properties
    public float MomentFlexion { get; set; }
    // on utilise des proprietés pour calculer à la volé les charges critiques
    public float ContrainteTractionELS { get
        {
            CalculerContrainteTraction(ELS);
            return _contrainteTraction;
        }
        set { _contrainteTraction = value; }
    }
    public float ContrainteTractionELU
    {
        get
        {
            CalculerContrainteTraction(ELU);
            return _contrainteTraction;
        }
        set { _contrainteTraction = value; }
    }
    
    public float FlecheMax { get; set; }
    #endregion

    #region Constructors
    public Poutre(GameObject representedObject, string materialName) : base(representedObject, materialName){}
    
    // différent constructeur sont pour l'instant utilisé pour créer différentes poutre de différentes sections
    public Poutre(float width, GameObject representedObject, string materialName) : base(representedObject, materialName)
    {
        _width = width;
        _height = _width;
        _sectionType = Section.carre;
    }

    public Poutre(float width, float height, GameObject representedObject, string materialName) : base(representedObject, materialName)
    {
        _width = width;
        _height = height;
        _sectionType = Section.rectangle;
    }

    public Poutre(float largeurSemelle, float hauteurAme, float epaisseurSemelle, float epaisseurAme, GameObject representedObject, string materialName) : base(representedObject, materialName)
    {
        _width = largeurSemelle;
        _height = hauteurAme;
        _epaisseurSemelle = epaisseurSemelle;
        _epaisseurAme = epaisseurAme;
        _sectionType = Section.I;
    }
    #endregion
    
    #region Methods
    //calcul du moment quadratique
    void CalculerMomentQuadratique()
    {
        if (_sectionType == Section.carre)
            I = (float)Math.Pow(_width, 4) / 12;
        else if (_sectionType == Section.rectangle)
            I = (float)(_width * Math.Pow(_height, 3)) / 12;
        else if (_sectionType == Section.I)
        {
            float p1 = (_epaisseurAme * (float)Math.Pow((_height - 2 * _epaisseurSemelle), 3)) / 12;
            float p3 = (float)(_width*Math.Pow(_epaisseurSemelle,3))/12;
            float p4 = (_width * _epaisseurSemelle) * (float)Math.Pow((_height / 2 - _epaisseurSemelle / 2), 2);
            float p2 = p3 + p4;

            I = p1 + 2 * p2;
        }
    }

    // calcul du moment fléchissant
    void CalculerMomentFlexion(float charge)
    {
        Vector3 size = RepresentedObject.GetComponent<MeshFilter>().mesh.bounds.size;
        float length = Math.Max(size.x, size.z);
        // passage de charge en Nm2 en charge linéique en N/m
        float chargeLin = charge / length;
        // Calcul du moment de flexion 
        MomentFlexion = ((chargeLin * (float)Math.Pow(length, 2))) / 8;
    }

    // calcul de la contrainte en traction
    void CalculerContrainteTraction(float charge)
    {
        CalculerMomentQuadratique(); // calcul du moment quadratique
        CalculerMomentFlexion(charge); // calcule du moment en flexion
        _contrainteTraction = (MomentFlexion / (I * (float)Math.Pow(10,-12)) * ((_height * (float)Math.Pow(10,-3)) / 2)) * (float)Math.Pow(10,-6);
    }

    // calcul de la flèche maximale
    public void CalculerFlecheMax(float moduleYoung)
    {
        Vector3 size = RepresentedObject.GetComponent<MeshFilter>().mesh.bounds.size;
        float length = Math.Max(size.x, size.z);
        // passage de charge en Nm2 en charge linéique en N/m
        float charge = ELS / length;
        // passage du module de Young en N/m²
        moduleYoung = moduleYoung * (float)Math.Pow(10, 9);
        FlecheMax = ((5 * charge * (float)Math.Pow(length,4))/( 384 * moduleYoung *I));
    }
    #endregion
}
