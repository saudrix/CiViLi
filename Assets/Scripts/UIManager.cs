using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIManager : MonoBehaviour
{
    private delegate void OnSelectionChangedHandler();
    private static event OnSelectionChangedHandler _onSelectionChanged;

    private delegate void OnSelectionModifiedHandler();
    private static event OnSelectionModifiedHandler _onSelectionModified;

    private static string connectionString;

    #region Properties
    // publics given by unity
    public GameObject mainCamera;
    public GameObject mainParent;
    public GameObject objectPanel;
    public GameObject updatePanel;
    public GameObject updateDisplayPort;
    public GameObject poutreInfoPanel;
    public GameObject selectedUpdatePanel;
    public GameObject multipleSelectionPanel;
    // used for display in update menu
    public int gridDisplayWidthCount;
    // customs
    List<BuildingBloc> currentSelection = new List<BuildingBloc>(); // permet de maintenir la séléction courante
    PoutreModel poutreModel;
    MaterialModel materialModel;
    // prefabs
    public GameObject poutrePic;
    public GameObject poutreSquarePic;
    public GameObject materialMiniature;
    public GameObject infoLabel;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // création des modèles
        poutreModel = new PoutreModel("URI=file:" + Application.dataPath + "/CiViLiDB.db");
        materialModel = new MaterialModel("URI=file:" + Application.dataPath + "/CiViLiDB.db");
        // On abonne les événement a leur délégués
        _onSelectionChanged += new OnSelectionChangedHandler(UpdateObjectPanel);
        _onSelectionModified += new OnSelectionModifiedHandler(UpdateObjectPanel);
        // On masque l'interface non pertinente
        objectPanel.SetActive(false);
        updatePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ManageInpupts();
    }
    
    void UpdateObjectPanel()
    {
        // Si rien n'est selectionné on cache le panneau d'info
        if (currentSelection.Count == 0) objectPanel.SetActive(false);
        // Si un seul objet est selectionné, on affiche ses infos
        else if( currentSelection.Count == 1)
        {
            objectPanel.SetActive(true);
            multipleSelectionPanel.SetActive(false);
            // On donne un titre au panel
            ((Text)objectPanel.transform.GetChild(0).GetComponent<Text>()).text = currentSelection[0].RepresentedObject.name;
            // Si l'objet est une poutre
            if(currentSelection[0] is Poutre)
            {
                Poutre selected = (Poutre)currentSelection[0];

                List<float> data = materialModel.GetAllDataByName(selected.MaterialName);

                float youngModuleOfSelection = data[0];
                poutreInfoPanel.SetActive(true);
                ((Text)poutreInfoPanel.transform.GetChild(0).GetComponent<Text>()).text = "Poid : " + selected.P + " N";
                ((Text)poutreInfoPanel.transform.GetChild(1).GetComponent<Text>()).text = "Poid porté : " + selected.G + " N";
                ((Text)poutreInfoPanel.transform.GetChild(2).GetComponent<Text>()).text = "Sigma ELS: " + selected.ContrainteTractionELS + " MPa";
                ((Text)poutreInfoPanel.transform.GetChild(2).GetComponent<Text>()).color = (selected.ContrainteTractionELS > data[1]) ? Color.red : Color.green;
                ((Text)poutreInfoPanel.transform.GetChild(3).GetComponent<Text>()).text = "Sigma ELU : " + selected.ContrainteTractionELU + " MPa";
                selected.CalculerFlecheMax(youngModuleOfSelection);
                float flechemm = selected.FlecheMax*(float)Math.Pow(10, 3);
                ((Text)poutreInfoPanel.transform.GetChild(4).GetComponent<Text>()).text = "Fleche Max : " + flechemm + " mm";
            }
            // Si l'objet est un poteau
            else if(currentSelection[0] is Poteau)
            {
                Poteau selected = (Poteau)currentSelection[0];

                List<float> data = materialModel.GetAllDataByName(selected.MaterialName);
                poutreInfoPanel.SetActive(true);
                selected.CalculerChargeMax(data[2], data[0], data[1]);
                ((Text)poutreInfoPanel.transform.GetChild(0).GetComponent<Text>()).text = "Poid : " + selected.P + " N";
                ((Text)poutreInfoPanel.transform.GetChild(1).GetComponent<Text>()).text = "Poid porté : " + (selected.G - selected.P) + " N";
                ((Text)poutreInfoPanel.transform.GetChild(2).GetComponent<Text>()).color = Color.black;
                ((Text)poutreInfoPanel.transform.GetChild(2).GetComponent<Text>()).text = "Labmda red: " + selected.ElancementReduit;
                ((Text)poutreInfoPanel.transform.GetChild(3).GetComponent<Text>()).text = "Coeff red : " + selected.CoeffReduction;
                ((Text)poutreInfoPanel.transform.GetChild(4).GetComponent<Text>()).text = "Charge Max : " + selected.ChargeMax + " N";
                ((Text)poutreInfoPanel.transform.GetChild(4).GetComponent<Text>()).color = (selected.G - selected.P > selected.ChargeMax) ? Color.red : Color.green;
            }
            // si l'a selection n'est pas encore renseignée
            else
            {
                poutreInfoPanel.SetActive(false);
            }
        }
        // si plusieurs objets sont selectionnées, on affiche leur noms
        else
        {
            objectPanel.SetActive(true);
            // On donne un titre au panel
            ((Text)objectPanel.transform.GetChild(0).GetComponent<Text>()).text = "Selection";
            // on rempli les titres
            poutreInfoPanel.SetActive(false);
            multipleSelectionPanel.SetActive(true);
            // clear the selection
            foreach (Transform child in multipleSelectionPanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            // on affiche les objets selectionnés
            int index = 0;
            int offset = 30;
            foreach(BuildingBloc selected in currentSelection)
            {
                GameObject label = Instantiate(infoLabel, new Vector3(100, -30 - offset * index, 0), Quaternion.identity);
                label.transform.SetParent(multipleSelectionPanel.transform, false);
                label.GetComponent<Text>().text = selected.RepresentedObject.name;
                index++;
            }
        }
    }

    void ManageInpupts()
    {
        float transSpeed = 15f;
        // Gestion du Scroll
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            mainParent.transform.Translate(Vector3.forward * transSpeed * Time.deltaTime, Space.World);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            mainParent.transform.Translate(Vector3.back * transSpeed * Time.deltaTime, Space.World);
        }
        // Gestion des touches
        float speed = 50f;
        if (Input.GetKey(KeyCode.Q))
        {
            mainParent.transform.Rotate(Vector3.up * speed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            mainParent.transform.Rotate(-Vector3.up * speed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            mainParent.transform.Rotate(Vector3.right * speed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            mainParent.transform.Rotate(-Vector3.right * speed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.G))
        {
            mainParent.transform.position = new Vector3(0, 0, 0);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.R))
        {
            mainParent.transform.rotation = Quaternion.identity;
        }
        // Gestions des clics
        if (Input.GetMouseButtonUp(0))
        {
            // Cast a ray to mouse coordinates
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                if (hit.transform != null) // If the ray exist
                {
                    BuildingBloc clickedBloc = (BuildingBloc)BuildingBloc.accessTable[hit.transform.gameObject];
                    // Manage selection
                    if (currentSelection.Contains(clickedBloc))
                    {
                        currentSelection.Remove(clickedBloc);
                        clickedBloc.ToggleSelection();
                    }
                    else if (Input.GetKey(KeyCode.LeftShift))
                    {
                        // selection multiple
                        currentSelection.Add(clickedBloc);
                        clickedBloc.ToggleSelection();
                    }
                    else
                    {
                        // selection simple
                        if (currentSelection.Count > 0)
                        {
                            foreach (BuildingBloc bloc in currentSelection) bloc.ToggleSelection();
                        }
                        currentSelection = new List<BuildingBloc>() { clickedBloc };
                        clickedBloc.ToggleSelection();
                    }
                    // On raise l'événement SelectionChangeg
                    _onSelectionChanged();
                }
            }
        }
    }

    public void OnStructuralToggle()
    {
        foreach(BuildingBloc selected in currentSelection)
        {
            selected.ToggleStructural();
        }
    }

    #region OnUpdateAction
    // Fonctions permettant d'assigner les objets back-end au gameObject
    void SetUptdateOnIPEPoutreClick(BaseEventData data)
    {
        // Find clicked miniature
        PointerEventData pointerData = data as PointerEventData;
        GameObject clicked = pointerData.pointerPress;
        // load related data
        List<float> poutreData = poutreModel.GetIPEByName(clicked.name);
        // replace selections
        for(int i = 0; i < currentSelection.Count; i++)
        {
            BuildingBloc undifined = currentSelection[i];
            GameObject represented = undifined.RepresentedObject;
            string matName = undifined.MaterialName;

            Poutre poutre = new Poutre(poutreData[1], poutreData[0], poutreData[2], poutreData[3], represented, matName);
            poutre.ToggleSelection();
            
            poutre.VolumicMass = undifined.VolumicMass;
            poutre.ComputeSelfWeight();

            BuildingBloc.accessTable[represented] = poutre;
            currentSelection[i] = poutre;
        }
        // on raise l'événement selectionModified
        _onSelectionModified();
    }

    void SetUptdateOnSquarePoutreClick(BaseEventData data)
    {
        // Find clicked miniature
        PointerEventData pointerData = data as PointerEventData;
        GameObject clicked = pointerData.pointerPress;
        // load related data
        List<float> poutreData = poutreModel.GetSquareByName(clicked.name);
        // replace selections
        for (int i = 0; i < currentSelection.Count; i++)
        {
            BuildingBloc undifined = currentSelection[i];
            GameObject represented = undifined.RepresentedObject;
            string matName = undifined.MaterialName;

            Poutre poutre = new Poutre(poutreData[0], represented, matName);
            poutre.ToggleSelection();

            poutre.VolumicMass = undifined.VolumicMass;
            poutre.ComputeSelfWeight();

            BuildingBloc.accessTable[represented] = poutre;
            currentSelection[i] = poutre;
        }
        _onSelectionModified();
    }

    void SetUpdateOnPoteauClick(BaseEventData data)
    {
        // Find clicked miniature
        PointerEventData pointerData = data as PointerEventData;
        GameObject clicked = pointerData.pointerPress;
        
        if(clicked.transform.childCount == 5)
        {
            // load related data
            List<float> poutreData = poutreModel.GetIPEByName(clicked.name);

            // replace selections
            for (int i = 0; i < currentSelection.Count; i++)
            {
                BuildingBloc undifined = currentSelection[i];
                GameObject represented = undifined.RepresentedObject;
                string matName = undifined.MaterialName;

                Poteau poteau = new Poteau(poutreData[1], poutreData[0], poutreData[2], poutreData[3], represented, matName);

                poteau.ToggleSelection();

                poteau.VolumicMass = undifined.VolumicMass;
                poteau.ComputeSelfWeight();

                BuildingBloc.accessTable[represented] = poteau;
                currentSelection[i] = poteau;
            }
        }
        else
        {
            // load related data
            List<float> poutreData = poutreModel.GetSquareByName(clicked.name);
            // replace selections
            for (int i = 0; i < currentSelection.Count; i++)
            {
                BuildingBloc undifined = currentSelection[i];
                GameObject represented = undifined.RepresentedObject;
                string matName = undifined.MaterialName;

                Poteau poteau = new Poteau(poutreData[0], represented, matName);
                poteau.ToggleSelection();

                poteau.VolumicMass = undifined.VolumicMass;
                poteau.ComputeSelfWeight();

                BuildingBloc.accessTable[represented] = poteau;
                currentSelection[i] = poteau;
            }
        }
        // on raise l'événement selectionModfied
        _onSelectionModified();
    }

    void SetMaterial(BaseEventData data)
    {
        // Find clicked miniature
        PointerEventData pointerData = data as PointerEventData;
        GameObject clicked = pointerData.pointerPress;
        // load related data
        float ro = materialModel.GetRoByName(clicked.name);
        // assign changes
        foreach(BuildingBloc bloc in currentSelection)
        {
            bloc.VolumicMass = ro;
            bloc.MaterialName = clicked.name;
            bloc.ComputeSelfWeight();
        }
        // on raise l'événement selectionMondified
        _onSelectionModified();
    }

    void SetUpdateOnMiscClick(BaseEventData data)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region TabPanelAction
    public void OnUpdatePanelChange(BaseEventData data)
    {
        // Find clicked object
        PointerEventData pointerData = data as PointerEventData;
        GameObject clicked = pointerData.pointerPress;
        // si on clique sur une nouvelle selection
        if(clicked != selectedUpdatePanel)
        {
            // Changes on apparence
            UnsetFocus(selectedUpdatePanel);
            selectedUpdatePanel = clicked;
            SetFocus(selectedUpdatePanel);

            // clear the selection
            foreach (Transform child in updateDisplayPort.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            // functionalities
            switch (clicked.name)
            {
                case "toggleStructuralMenu":
                    DisplayStructuralInfos();
                    break;
                case "toggleMaterialMenu":
                    DisplayMaterialInfos();
                    break;
                case "toggleMIscaleanousMenu":
                    //DisplayMiscInfos();
                    break;
            }
        }
    }

    void SetFocus(GameObject selectedButtonPanel)
    {
        selectedButtonPanel.GetComponent<Image>().color = updatePanel.GetComponent<Image>().color;
        ((Text)selectedButtonPanel.transform.GetChild(0).GetComponent<Text>()).color = Color.black;
    }

    void UnsetFocus(GameObject selectedButtonPanel)
    {
        selectedButtonPanel.GetComponent<Image>().color = new Color32(80, 80, 80, 255);
        ((Text)selectedButtonPanel.transform.GetChild(0).GetComponent<Text>()).color = Color.white;
    }

    #region TabPanelAffichage
    void DisplayStructuralInfos()
    {
        DisplayPoutresInfos();
        // DisplayPoteauInfos();
    }

    void DisplayPoutresInfos()
    {
        // recuperation et affichage des types de poutres
        List<string> poutresIPE = poutreModel.GetAllIPE();
        List<string> poutresSquare = poutreModel.GetAllsquare();

        // calcul des dimensions
        float winWidth = updateDisplayPort.GetComponent<RectTransform>().sizeDelta.x;
        float xsize = poutrePic.GetComponent<RectTransform>().sizeDelta.x;
        float ysize = poutrePic.GetComponent<RectTransform>().sizeDelta.y;
        float spaceSize = (winWidth - gridDisplayWidthCount * xsize) / (gridDisplayWidthCount + 1);

        int index = 0;
        // affichages des poutres à section en I
        foreach (string name in poutresIPE)
        {
            // calcul des positions
            int offset = 20;
            int offset1 = 180;

            int posx = index % gridDisplayWidthCount;
            int posy = index / gridDisplayWidthCount;

            float xCoord = (posx * xsize) + (posx + 1) * spaceSize;
            float yCoord = (posy * ysize) + (posy + 1) * spaceSize;

            // Instanciation du prefab
            GameObject go = Instantiate(poutrePic, new Vector3(xCoord + offset, -yCoord - offset, 0), Quaternion.identity);
            go.transform.SetParent(updateDisplayPort.transform, false);
            // Ajout des événement
            EventTrigger eventManager = go.GetComponent<EventTrigger>();
            eventManager.triggers[0].callback.AddListener((eventData) => { SetUptdateOnIPEPoutreClick(eventData); });

            // getting the actual data 
            List<float> data = poutreModel.GetIPEByName(name);
            // displaying the data
            go.transform.GetChild(0).GetComponent<Text>().text = "w: " + data[1].ToString();
            go.transform.GetChild(1).GetComponent<Text>().text = "h: " + data[0].ToString();
            go.transform.GetChild(2).GetComponent<Text>().text = "tw: " +  data[2].ToString();
            go.transform.GetChild(3).GetComponent<Text>().text = "tf: " + data[3].ToString();
            go.transform.GetChild(4).GetComponent<Text>().text = name;

            go.name = name;

            GameObject go1 = Instantiate(poutrePic, new Vector3(xCoord + offset, -yCoord - offset1, 0), Quaternion.identity);
            go1.transform.SetParent(updateDisplayPort.transform, false);
            // astuce, on dédouble les données et les prefabs des poutre seul la fonction appelé par l'évenement change
            EventTrigger eventManager1 = go1.GetComponent<EventTrigger>();
            eventManager1.triggers[0].callback.AddListener((eventData) => { SetUpdateOnPoteauClick(eventData); });
            
            // displaying the data
            go1.transform.GetChild(0).GetComponent<Text>().text = "w: " + data[1].ToString();
            go1.transform.GetChild(1).GetComponent<Text>().text = "h: " + data[0].ToString();
            go1.transform.GetChild(2).GetComponent<Text>().text = "tw: " + data[2].ToString();
            go1.transform.GetChild(3).GetComponent<Text>().text = "tf: " + data[3].ToString();
            go1.transform.GetChild(4).GetComponent<Text>().text = name;

            go1.name = name;

            index++;
        }
        // affichage
        index = 0;
        // affichage des miniatures pour les poutres carrées
        foreach (string name in poutresSquare)
        {
            int xoffset = 20;
            int yoffset = 100;
            int yoffset1 = 280;

            int posx = index % gridDisplayWidthCount;
            int posy = index / gridDisplayWidthCount;

            float xCoord = (posx * xsize) + (posx + 1) * spaceSize;
            float yCoord = (posy * ysize) + (posy + 1) * spaceSize;

            GameObject go = Instantiate(poutreSquarePic, new Vector3(xCoord + xoffset, -yCoord - yoffset, 0), Quaternion.identity);
            go.transform.SetParent(updateDisplayPort.transform, false);

            EventTrigger eventManager = go.GetComponent<EventTrigger>();
            eventManager.triggers[0].callback.AddListener((eventData) => { SetUptdateOnSquarePoutreClick(eventData); });

            // getting the actual data 
            List<float> data = poutreModel.GetSquareByName(name);
            // displaying the data
            go.transform.GetChild(0).GetComponent<Text>().text = "a: " + data[0].ToString();
            go.transform.GetChild(1).GetComponent<Text>().text = name;

            go.name = name;

            GameObject go1 = Instantiate(poutreSquarePic, new Vector3(xCoord + xoffset, -yCoord - yoffset1, 0), Quaternion.identity);
            go1.transform.SetParent(updateDisplayPort.transform, false);

            EventTrigger eventManager1 = go1.GetComponent<EventTrigger>();
            eventManager1.triggers[0].callback.AddListener((eventData) => { SetUpdateOnPoteauClick(eventData); });
            
            // displaying the data
            go1.transform.GetChild(0).GetComponent<Text>().text = "a: " + data[0].ToString();
            go1.transform.GetChild(1).GetComponent<Text>().text = name;

            go1.name = name;
            index++;
        }
    }

    void DisplayMaterialInfos()
    {
        
        // recuperation et affichage des types de matériaux
        List<string> materials = materialModel.GetAll();

        // calcul des positions
        float winWidth = updateDisplayPort.GetComponent<RectTransform>().sizeDelta.x;
        float xsize = materialMiniature.GetComponent<RectTransform>().sizeDelta.x;
        float ysize = materialMiniature.GetComponent<RectTransform>().sizeDelta.y;
        float spaceSize = (winWidth - gridDisplayWidthCount * xsize) / (gridDisplayWidthCount + 1);

        int index = 0;
        foreach (string name in materials)
        {
            int offset = 40;

            int posx = index % gridDisplayWidthCount;
            int posy = index / gridDisplayWidthCount;

            float xCoord = (posx * xsize) + (posx + 1) * spaceSize;
            float yCoord = (posy * ysize) + (posy + 1) * spaceSize;
            // Instanciation dynamique des miniatures
            GameObject go = Instantiate(materialMiniature, new Vector3(xCoord + offset, -yCoord - offset, 0), Quaternion.identity);
            go.transform.SetParent(updateDisplayPort.transform, false);

            // Remplissage de la miniature
            ((Text)go.transform.GetChild(0).GetComponent<Text>()).text = name;
            // création du Sprite à partir de l'image de texture
            ((Image)go.transform.GetChild(1).GetComponent<Image>()).sprite = Resources.Load<Sprite>("Textures/" + name + "Sprite");
            // Ajout de la fonction callback
            EventTrigger eventManager = go.GetComponent<EventTrigger>();
            eventManager.triggers[0].callback.AddListener((eventData) => { SetMaterial(eventData); });

            go.name = name;
            index++;
        }
    }

    void DisplayMiscInfos()
    {
        throw new NotImplementedException();
    }
    #endregion

    #endregion

    #region MainButtons
    public void LoadButtonOnClick()
    {
        // clear if already exists
        foreach (Transform child in mainParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        // clear current selection
        currentSelection = new List<BuildingBloc>();

        string _path = "C:/Users/simon/Desktop/2A - S4/CiViLi/BlenderTest.obj";
        List<GameObject> data   = Parser.ParseFile(_path); // Get a list of gameObjects out of an obj file
        // emmbedding game objetc in back end format
        TreatData(data);
        // set up view according to size parameters
        SetUpCamera();
    }
    
    public void UpdateButtonOnClick()
    {
        // on cache la structure
        mainParent.SetActive(false);
        // affichage du menu de selection
        updatePanel.SetActive(true);
        // par défaut on affiche le menu structurel
        SetFocus(selectedUpdatePanel);
        DisplayStructuralInfos();
    }
    
    public void OnUpdateConfirm()
    {
        // clear the selection
        foreach (Transform child in updateDisplayPort.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        UnsetFocus(selectedUpdatePanel);
        updatePanel.SetActive(false);
        mainParent.SetActive(true);
    }

    public void ComputeLoadButton()
    {
        mainParent.transform.localRotation = Quaternion.identity;

        // Set layered structure
        foreach (BuildingBloc obj1 in BuildingBloc.accessTable.Values)
        {
            obj1.RepresentedObject.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            foreach (BuildingBloc obj2 in BuildingBloc.accessTable.Values)
            {
                obj2.RepresentedObject.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                if (obj1 != obj2)
                {
                    // is one obj is on top of the other
                    if (IsOn(obj1.RepresentedObject, obj2.RepresentedObject))
                    {
                        obj1.On.Add(obj2);
                        obj2.Under.Add(obj1);
                    }
                }
            }
        }
        // Compute Lifted Weight
        List<BuildingBloc> orderedObjects = BuildingBloc.accessTable.Values.Cast<BuildingBloc>().ToList().OrderByDescending(o => o.RepresentedObject.GetComponent<Collider>().bounds.max.y).ToList();
        foreach (BuildingBloc data in orderedObjects)
        {  
            data.ComputeLiftedWeight();
        }
    }

    #endregion

    #region WeightDescent
    // return true if obj1 is on obj2 false otherwise
    bool IsOn(GameObject obj1, GameObject obj2)
    {
        Bounds obj1Bounds = obj1.GetComponent<Collider>().bounds;
        Bounds obj2Bounds = obj2.GetComponent<Collider>().bounds;

        Vector3 minObj1 = obj1Bounds.min;
        Vector3 maxObj1 = obj1Bounds.max;
        Vector3 minObj2 = obj2Bounds.min;
        Vector3 maxObj2 = obj2Bounds.max;

        // check if bottom and top are at the same height
        if (Math.Round(minObj1.y, 1) == Math.Round(maxObj2.y, 1))
        {
            // checking for ovelpas on two axis
            if (IsPlaneOverlap(minObj1, minObj2, maxObj1, maxObj2))
            {
                return true;
            }
        }
        return false;
    }

    bool IsPlaneOverlap(Vector3 minObj1, Vector3 minObj2, Vector3 maxObj1, Vector3 maxObj2)
    {
        // mins for object one on all axis
        float minX1 = minObj1.x;
        float minZ1 = minObj1.z;
        // mins for object two on all axis
        float minX2 = minObj2.x;
        float minZ2 = minObj2.z;
        // maxs for object one on all axis
        float maxX1 = maxObj1.x;
        float maxZ1 = maxObj1.z;
        // maxs for object two on all axis
        float maxX2 = maxObj2.x;
        float maxZ2 = maxObj2.z;

        // check for overlaps on a single axis
        if (IsAxisOverlap(minX1, minX2, maxX1, maxX2)
            && IsAxisOverlap(minZ1, minZ2, maxZ1, maxZ2)
            ) return true;  // X & Z
        return false;
    }

    bool IsAxisOverlap(float min1, float min2, float max1, float max2)
    {
        if (max1 >= min2 || min1 <= max2) return true;
        else return false;
    }
    #endregion

    #region Initialisation
    void SetUpCamera()
    {
        // Recherche de l'axe principal et des tailles
        float mainAxesMaxLength = Math.Max(Math.Abs(Parser.MaxX-Parser.MinX), Math.Abs(Parser.MaxZ-Parser.MinZ));
        float secondaryAxisMaxLength = Math.Min(Math.Abs(Parser.MaxX-Parser.MinX), Math.Abs(Parser.MaxZ-Parser.MinZ));
        float fov = Camera.main.fieldOfView;
        float totalHeight = Math.Abs(Parser.MaxY - Parser.MinY);
        //Calcul de l'offset
        float widthNeededZ = (float)(Math.Tan(fov/2) * mainAxesMaxLength /2f );
        // Calcul des position
        float centerX = Parser.MaxX - secondaryAxisMaxLength / 2;
        float posZ = mainParent.transform.position.z - Math.Abs(widthNeededZ);
        float posY = Parser.MinY + totalHeight / 2f;

        Camera.main.transform.position = new Vector3(centerX, posY, posZ);
    }

    // fonction permettant d'associer les données front et back et de remplir la table de hachage
    void TreatData(List<GameObject> data)
    {
        foreach(GameObject obj in data)
        {
            obj.transform.parent = mainParent.transform;
            BuildingBloc bloc = new BuildingBloc(obj, "default");
            BuildingBloc.accessTable.Add(obj, bloc);
        }
    }
    #endregion
}
