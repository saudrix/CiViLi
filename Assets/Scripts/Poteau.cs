using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Poteau : BuildingBloc
{
    #region Attributes
    private float _width;
    private float _height;
    private float I;
    private float A;
    private float _epaisseurSemelle;
    private float _epaisseurAme;
    
    public Section _sectionType = new Section();
    #endregion

    #region Properties
    public float LongueurFlambement { get; set; }
    public float RayonGiration { get { return (float)Math.Sqrt(I / A); } }
    public float CoeffElancement { get; set; }
    public float ElancementCritique { get; set; }
    public float ElancementReduit { get; set; }
    public float PHI { get; set; }
    public float CoeffReduction { get; set; }
    public float ChargeMax { get; set; }
    public float Alpha { get; set; }
    #endregion

    #region Constructors
    public Poteau(GameObject representedObject, string materialName) : base(representedObject, materialName) { }

    public Poteau(float width, GameObject representedObject, string materialName) : base(representedObject, materialName)
    {
        _width = width;
        _height = _width;
        _sectionType = Section.carre;
    }

    public Poteau(float width, float height, GameObject representedObject, string materialName) : base(representedObject, materialName)
    {
        _width = width;
        _height = height;
        _sectionType = Section.rectangle;
    }

    public Poteau(float largeurSemelle, float hauteurAme, float epaisseurSemelle, float epaisseurAme, GameObject representedObject, string materialName) : base(representedObject, materialName)
    {
        _width = largeurSemelle;
        _height = hauteurAme;
        _epaisseurSemelle = epaisseurSemelle;
        _epaisseurAme = epaisseurAme;
        _sectionType = Section.I;
    }
    #endregion

    #region Methods
    // calcul de I
    void CalculerMomentQuadratique()
    {
        if (_sectionType == Section.carre)
            I = (float)Math.Pow(_width, 4) / 12;
        else if (_sectionType == Section.rectangle)
            I = (float)(_width * Math.Pow(_height, 3)) / 12;
        else if (_sectionType == Section.I)
        {
            float p1 = (_epaisseurAme * (float)Math.Pow((_height - 2 * _epaisseurSemelle), 3)) / 12;
            float p3 = (float)(_width * Math.Pow(_epaisseurSemelle, 3)) / 12;
            float p4 = (_width * _epaisseurSemelle) * (float)Math.Pow((_height / 2 - _epaisseurSemelle / 2), 2);
            float p2 = p3 + p4;

            I = p1 + 2 * p2;
        }
    }

    // calcul de l'aire de la section en mm²
    void CalculerAireSection()
    {
        if (_sectionType == Section.carre || _sectionType == Section.rectangle) A = _width * _height;
        else if(_sectionType == Section.I)
        {
            A = _width * _epaisseurSemelle * 2 + (_height - 2 * _epaisseurSemelle) * _epaisseurAme;
        }
    }

    // calcul de la longeur de flambement
    void CalculerLongueurFlambement()
    {
        Vector3 size = RepresentedObject.GetComponent<MeshFilter>().mesh.bounds.size;
        float height = size.y;
        LongueurFlambement = (float)0.5 * height;
    }
    
    // calcul de l'élancement simple
    void CalculerElancementLambda()
    {
        CalculerMomentQuadratique(); // recuperation de I
        CalculerLongueurFlambement(); // calcul de la longueur de flambement
        CalculerAireSection(); // recuperation de A
        CoeffElancement = LongueurFlambement * (float)Math.Pow(10, 3) / RayonGiration;
    }

    // Compute Euler Formula
    void CalculerElancementCritique(float E, float Re)
    {
        // conversion de la limite élastique en GigaPascals
        Re = Re * (float)Math.Pow(10, -3);
        ElancementCritique = (float)Math.Sqrt(Math.PI * E / Re);
    }

    // calcul de l'élancement réduit
    void CalculerElancementReduit(float E, float Re)
    {
        //CalculerElancementLambda();
        //CalculerElancementCritique(E, Re);
        //ElancementReduit = CoeffElancement / ElancementCritique;

        CalculerLongueurFlambement(); // calcul de la longeur de flambement
        CalculerAireSection(); // récupération de A
        CalculerMomentQuadratique(); // récuperation de I
        ElancementReduit = (float)(LongueurFlambement / Math.PI) * (float)Math.Sqrt(A/I) * (float)Math.Sqrt(Re/(E*Math.Pow(10,3)));
    }

    // calcul du coefficient de reduction
    void CalculerCoeffReduction(float E, float Re)
    {
        CalculerElancementReduit(E, Re); // calcul de l'élancement réduit
        CalculerPHI(); // calcul de PHI

        CoeffReduction = 1 / (PHI + (float)Math.Sqrt(Math.Pow(PHI,2) - Math.Pow(ElancementReduit,2)));
    }
    
    void CalculerPHI()
    {
        // Alpha est le facteur d'imperfection du matériaux
        PHI =(float)( 1 + Alpha * (ElancementReduit - 0.2) + (float)Math.Pow(ElancementReduit,2)) / 2;
    }

    // Calcule la charge max en Newton
    public void CalculerChargeMax(float alpha, float E, float Re)
    {
        Alpha = alpha;
        CalculerCoeffReduction(E, Re);

        ChargeMax = CoeffReduction * (float)(A * Math.Pow(10,3)) * Re;
    }
    #endregion

}

