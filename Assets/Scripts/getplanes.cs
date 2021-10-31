using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using Mirror;
using Newtonsoft.Json;



public class getplanes : NetworkBehaviour
{
    public struct PlaneData// egy változó a plane-nek a pontjaira, és a plane id-jára
    {
        public Vector3 position;
        public string Jvertice;
        public Quaternion rotation;
        public int id;
        public int boundarylength;
        public uint playerNetID;


        public PlaneData(string Jvertice, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
        {
            this.position = position;
            this.rotation = rotation;
            this.Jvertice = Jvertice;
            this.id = id;
            this.boundarylength = boundarylength;
            this.playerNetID = playerNetID;

        }
    }


    ARAnchorManager m_AnchorManager;
    List<ARAnchor> m_Anchors = new List<ARAnchor>();
    static Dictionary<string, PlaneData> verticesDict = new Dictionary<string, PlaneData>();
    static Dictionary<string, Mesh> ServerMeshes = new Dictionary<string, Mesh>();
    static Dictionary<string, GameObject> planesDict = new Dictionary<string, GameObject>();
    private uint playerNetID;
    private GameObject StepChild;  //to get a position  and rotation
    public GameObject meshF;
    public ARPlaneManager planeManager;
    public Material mat;
    ARPlane planeNew;
    public Text debug;
    Unity.Collections.NativeArray<Vector2> vectors;
    PlayerObject playerobjscript;
    Vector3[] pontok = new Vector3[11];
    private GameObject origo;
    private ARTrackedImageManager aRTrackedImageManager;
    public GameObject worldMap;

    private ARTrackedImage Image;
    public bool watchTheimage = true;
    public bool samePos = false;
    public float PosDiff = 0.5f;
    public int RotDiff = 40;
    public static bool readImage = false;
    public float centerDist;
    public bool CheckNextMesh = false;
    public static float DiameterCheck = 0;
    //public float


    //tuner.EnableLocalEndpoint();
    void Start()
    {


        // GameObject newCanvas = Instantiate(canvas); 
        playerNetID = GetComponent<NetworkIdentity>().netId;
        m_AnchorManager = GetComponent<ARAnchorManager>();

        worldMap = GameObject.Find("WorldMap");
        while (worldMap == null)
            Debug.Log("coudnt find worldmap");
        if (worldMap != null)
            Debug.Log("Found worldmap");
        planeManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            Debug.Log("Plane manager found");
        }
        aRTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        if (aRTrackedImageManager != null)
            Debug.Log("found image tracker");
        StepChild = GameObject.Find("StepChild");
        planeManager.planesChanged += OnPlanesChanged;
        Debug.Log("Subscribed to event");
        aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
        //CmdAskForPlanesFromServerOnStart(planesDict.Count);
        planeManager.enabled = false;


#if UNITY_EDITOR
        readImage = true;
        if(!isServer)
        CmdAskForPlanesFromServerOnStart(this.gameObject, planesDict.Count);
        Debug.Log("asked for planes on start");
        planeManager.enabled = true;

#endif
    }


    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {

        if (worldMap != null)
        {
            foreach (var TrackedImage in args.added)
            {

                worldMap = GameObject.Find("WorldMap");
                /*while (worldMap == null)
                 Debug.Log("coudnt find worldmap");*/
                if (worldMap != null)
                    Debug.Log("Found worldmap");
                worldMap.transform.position = TrackedImage.transform.position;
                worldMap.transform.rotation = TrackedImage.transform.rotation;

                Debug.Log("Picture is seen and name: " + TrackedImage.name);
                readImage = true;
                CmdAskForPlanesFromServerOnStart(this.gameObject, planesDict.Count);
                planeManager.enabled = true;
            }


            foreach (var TrackedImage in args.updated)
            {
                worldMap.transform.position = TrackedImage.transform.position;
                worldMap.transform.rotation = TrackedImage.transform.rotation;

                Debug.Log("Picture is seen and name: " + TrackedImage.name);

            }
        }

    }



    public void PutWorldMapToOrigo()
    {
        Debug.Log("InPutworldMaptoOrigin as localclient");
        Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
        Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
        worldMap = GameObject.Find("WorldMap");
        if (worldMap != null)
        {
            worldMap.transform.position = GameObject.Find("Origo(Clone)").transform.position;
            worldMap.transform.rotation = GameObject.Find("Origo(Clone)").transform.rotation;
        }
        else
            Debug.Log("DIDNT FIND WORLDMAP");
    }
    public void CallForThePlanes()
    {
        if (isLocalPlayer)
        {
            Debug.Log("In callforplanes localPlayer");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
            //CmdAskForPlanesFromServerOnStart(this.gameObject ,planesDict.Count);
        }
    }




    [Command]
    public void CmdAskForPlanesFromServerOnStart(GameObject target, int PlanesCount)
    {

        if (isServer)
        {
            Debug.Log("In CmdAskforplanews as Server");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);

            if (PlanesCount < 1)
                foreach (var entry in verticesDict)
                {
                    NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
                    // RpcAddPlaneToClient(entry.Value.Jvertice, entry.Value.position, entry.Value.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
                    TargetCreatePlanesFromServer(opponentIdentity.connectionToClient, entry.Value.Jvertice, entry.Value.position, entry.Value.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
                }
        }
    }

    [TargetRpc]
    public void TargetCreatePlanesFromServer(NetworkConnection target, string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        Debug.Log("sorry");
        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);

    }

    [TargetRpc]
    public void TargetChecking(NetworkConnection target, int idk)
    {
        Debug.Log("I'm back name  :" + target + " , and verticesnumber " + idk);
    }



    void OnDisable()
    {

        if (!isLocalPlayer)
            return;
        if (planeManager == null)
            Debug.Log("too early2");
        else
        {
            planeManager.planesChanged -= OnPlanesChanged;
            Debug.Log("Unsubscribed to event");
        }

        aRTrackedImageManager.trackedImagesChanged += OnImageChanged;

        foreach (ARPlane plane in planeManager.trackables)
        {
            // CmdRemoveMapInfo(plane.GetInstanceID(), playerNetID);
            RemovePlane(plane.GetInstanceID(), playerNetID);
        }
    }




    void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
    {

        // Debug.Log("Planes Changed");
        if (isLocalPlayer)
        {

            // Debug.Log("We are local players");
            foreach (ARPlane plane in eventArgs.removed)
            {
                plane.boundaryChanged -= UpdatePlane;
                Debug.Log("PlanesRemoved, unsubscribed form event(probably)");
                CmdRemoveMapInfo(plane.GetInstanceID(), playerNetID);
                // CmdRemovePlaneFromServer(plane.GetInstanceID(), playerNetID);

            }



            foreach (ARPlane plane in eventArgs.added)
            {
                Debug.Log("In AddPlaneEvent as client");
                Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
                plane.boundaryChanged += UpdatePlane;
                Debug.Log("PlanesAdded and subscibed to event (probably)");

                StepChild = GameObject.Find("WorldMap").gameObject.transform.GetChild(0).gameObject;
                Debug.Log("Found Stepchild in add, name: " + StepChild.name);
                vectors = plane.boundary;

                Vector3[] vertices = new Vector3[plane.boundary.Length];
                int i;
                for (i = 0; i < plane.boundary.Length; i++)
                {
                    vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
                }

                string json = JsonConvert.SerializeObject(vertices);


                StepChild.transform.position = plane.transform.position;
                StepChild.transform.rotation = plane.transform.rotation;
                Debug.Log("rotation: " + StepChild.transform.rotation);
                Debug.Log("localrotation: " + StepChild.transform.localRotation);
                //StepChild.transform.parent = worldMap.transform;
                // CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                // CmdAddPlaneToServer(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                CmdAddMapInfo(json, StepChild.transform.localPosition, StepChild.transform.localRotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                //CmdAskForPlanesFromServerOnStart(planesDict.Count);
                // StepChild.transform.parent = null;
                //// CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                //CmdCreatePlaneFromData(json, plane.transform.position, Quaternion.Euler(newPlaneRot), plane.GetInstanceID(), plane.boundary.Length, playerNetID);
            }


        }

    }

    void UpdatePlane(ARPlaneBoundaryChangedEventArgs eventArgs)
    {

        if (isLocalPlayer)
        {

            // Debug.Log("Update was called by boundaryChanged");
            //Debug.Log("PlanesUpdated");
            vectors = eventArgs.plane.boundary;

            Vector3[] vertices = new Vector3[eventArgs.plane.boundary.Length];
            int i;
            for (i = 0; i < eventArgs.plane.boundary.Length; i++)
            {
                vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
            }
            StepChild = GameObject.Find("WorldMap").gameObject.transform.GetChild(0).gameObject;
            Debug.Log("Found Stepchild in update, name: " + StepChild.name);
            string json = JsonConvert.SerializeObject(vertices);
            /* Debug.Log("In updatePlaneEvent as client");
             Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
             Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/

            StepChild.transform.position = eventArgs.plane.transform.position;
            StepChild.transform.rotation = eventArgs.plane.transform.rotation;
            Debug.Log("rotation: " + StepChild.transform.rotation);
            Debug.Log("localrotation: " + StepChild.transform.localRotation);
            // StepChild.transform.parent = worldMap.transform;
            // CmdUpdateMapInfo(json, eventArgs.plane.transform.position, eventArgs.plane.transform.rotation, eventArgs.plane.GetInstanceID(), eventArgs.plane.boundary.Length, playerNetID);
            CmdUpdateMapInfo(json, StepChild.transform.localPosition, StepChild.transform.localRotation, eventArgs.plane.GetInstanceID(), eventArgs.plane.boundary.Length, playerNetID);
            // CmdAskForPlanesFromServerOnStart();
            // StepChild.transform.parent = null;

        }
    }


    [Command] //Serverre küldi a Plane adatokat
    public void CmdAddMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {

        if (isServer)
        {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
            string thrown = "good";
            foreach (var entry in verticesDict)
            {
                /* if (entry.Value.playerNetID != playerNetID && Mathf.Abs((entry.Value.position - position).magnitude) <= DiameterCheck && Mathf.Abs(entry.Value.rotation.eulerAngles.z - rotation.eulerAngles.z) < RotDiff && Mathf.Abs(entry.Value.position.z - position.z) < PosDiff)
                 {*/
                if (entry.Value.playerNetID != playerNetID && Mathf.Abs((entry.Value.position - position).magnitude) <= DiameterCheck && Mathf.Abs(Quaternion.Angle(rotation, entry.Value.rotation)) < RotDiff && Mathf.Abs(entry.Value.position.y - position.y) < PosDiff)
                {
                    int goodDist = 0;
                    CheckNextMesh = false;


                    var entryVertices = JsonConvert.DeserializeObject<List<Vector3>>(entry.Value.Jvertice);
                    Vector3[] entryVerticess = entryVertices.ToArray();
                    /* Vector2[] entryPolygon = new Vector2[entryVerticess.Length];
                     Vector2[] newPolygon = new Vector2[verticess.Length];
                     for (int i=0; i<entryVerticess.Length; i++)
                     {
                         entryPolygon[i] = new Vector2(entryVerticess[i].x + entry.Value.position.x, entryVerticess[i].z + entry.Value.position.z);
                     }
                     for (int i = 0; i < verticess.Length; i++)
                     {
                         newPolygon[i] = new Vector2(verticess[i].x + position.x, verticess[i].z + position.z);
                     }

                     Vector2 worldPoint = new Vector2(position.x, position.z);
                     if(entryPolygon.Length>2 && newPolygon.Length>2)
                    if (IsPointInPolygon4(entryPolygon, worldPoint, newPolygon))
                     {*/
                    if (IsPointInPolygon(entryVerticess, entry.Value.position, position, verticess))
                    {
                        /*Debug.Log("Yes, Center is in BigMesh");
                        foreach (var point in verticess)
                        {
                            worldPoint = new Vector2(position.x + point.x, position.z + point.z);
                            if (IsPointInPolygon4(Polygon, worldPoint))
                            {
                                Debug.Log("Yes, Points are also in mesh");
                                thrown = "yes";
                                samePos = true;
                               // samePos = false;
                               //RpcRemovePlaneFromClient(id, playerNetID);
                               //RemovePlane(id, playerNetID);
                            }
                            else
                            {
                                Debug.Log("No");
                                thrown = "No";
                                samePos = false;
                            }
                        }*/
                        RpcRemovePlaneFromClient(id, playerNetID);
                        RemovePlane(id, playerNetID);
                        Debug.Log("Yes, Points are also in mesh");
                        samePos = true;
                    }
                    else
                    {
                        Debug.Log("No");
                        thrown = "No";
                        samePos = false;
                    }
                    /*if (isInside(Polygon, entryVerticess.Length, worldPoint))
                    {
                        Debug.Log("Yes, center is inside");
                        foreach( var point in verticess)
                        {
                            worldPoint = new Vector2(position.x + point.x, position.z + point.z);
                            if (isInside(Polygon, entryVerticess.Length, worldPoint))
                            {
                                Debug.Log("Yes, points are inside");
                                thrown = "yes";
                                samePos = true;
                            }
                            else
                            {
                                Debug.Log("No, points are not inside");
                                thrown = "No";
                                samePos = false;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No, center is not inside");
                        thrown = "No";
                        samePos = false;
                    }*/

                    ///check if pos.x && .y  is inside the points+pos.x & .y
                    ///if not, go to the next 
                    ///if inside, check if all th points are inside, maybe with a bit of a posdiff



                }
            }

            foreach(var point in vertices)
            {
                if (Mathf.Abs((point - position).magnitude) > DiameterCheck)
                    DiameterCheck = (point - position).magnitude;
            }
            Debug.Log("Diameter Check: " + DiameterCheck);

            if (!samePos)
            {
                Debug.Log("I was kept and added");
                string verticesid = id.ToString() + playerNetID.ToString();
                if (verticesDict.ContainsKey(verticesid))
                {
                    Debug.Log("New Map Info existed already");
                }

                else
                {
                    /* foreach (var entry in verticesDict)
                     {
                         Debug.Log("Position of the vertices until now: " + entry.Value.position);
                     }*/
                    // Debug.Log("New Map Info added in cmd");
                    PlaneData pData;

                    pData.position = position;
                    pData.rotation = rotation;
                    pData.Jvertice = json;
                    pData.id = id;
                    pData.boundarylength = boundarylength;
                    pData.playerNetID = playerNetID;

                    verticesDict.Add(verticesid, pData);


                    /*if (ServerMeshes.ContainsKey(verticesid))
                    {
                        Debug.Log("New Mesh existed already");
                    }
                    else
                    {

                        Mesh Servermesh = new Mesh();
                        if (boundarylength > 2)
                        {

                            int[] tria = new int[3 * (boundarylength - 2)];
                            for (int c = 0; c < boundarylength - 2; c++)
                            {
                                tria[3 * c] = 0;
                                tria[3 * c + 1] = c + 1;
                                tria[3 * c + 2] = c + 2;
                            }

                            Servermesh.vertices = verticess;
                            Servermesh.triangles = tria;
                            Servermesh.RecalculateNormals();
                            ServerMeshes.Add(verticesid, Servermesh);
                        }
                    }*/
                    /*Debug.Log("In Add Cmd as Server");
                    Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                    Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
                    RpcAddPlaneToClient(json, position, rotation, id, boundarylength, playerNetID);
                    AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);
                }
            }
            else
            {
                samePos = false;
                Debug.Log("we dont add this");
            }

        }
    

    }

    [Command]
    public void CmdRemoveMapInfo(int id, uint playerNetID)
    {
        if (isServer)
        {
            /*Debug.Log("In RemoveCmd as Server");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
            string verticesid = id.ToString() + playerNetID.ToString();
            if (verticesDict.ContainsKey(verticesid))
            {
                verticesDict.Remove(verticesid);
                //RemovePlane(id, playerNetID);
                RpcRemovePlaneFromClient(id, playerNetID);
            }
            else
            {
                Debug.Log("Tried to Remove a Map Info that didn't exist");
            }
       }
    }

    public bool PositionApprSame(Vector3 Pos, Vector3 entryPos, float difference)
    {
        if (Mathf.Abs(Pos.x) - difference < Mathf.Abs(entryPos.x) || Mathf.Abs(entryPos.x) < Mathf.Abs(Pos.x) + difference)
        {
            if (Mathf.Abs(Pos.y) - difference < Mathf.Abs(entryPos.y) || Mathf.Abs(entryPos.y) < Mathf.Abs(Pos.y) + difference)
            {
                if (Mathf.Abs(Pos.z) - difference < Mathf.Abs(entryPos.z) || Mathf.Abs(entryPos.z) < Mathf.Abs(Pos.z) + difference)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;


    }

    public bool RotationApprSame(Quaternion Rot, Quaternion entryRot, float difference)
    {
        return true;
    }

    [Command]
    public void CmdUpdateMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        if (isServer)
        {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
            string thrown = "good";
            foreach (var entry in verticesDict)
            {
                /* if (entry.Value.playerNetID != playerNetID && Mathf.Abs((entry.Value.position - position).magnitude) <= DiameterCheck && Mathf.Abs(entry.Value.rotation.eulerAngles.z - rotation.eulerAngles.z) < RotDiff && Mathf.Abs(entry.Value.position.z - position.z) < PosDiff)
                {*/
                if (entry.Value.playerNetID != playerNetID && Mathf.Abs(( entry.Value.position - position).magnitude) <= DiameterCheck && Mathf.Abs(Quaternion.Angle(rotation, entry.Value.rotation)) < RotDiff && Mathf.Abs(entry.Value.position.y - position.y) < PosDiff)
                {
                    int goodDist = 0;
                    CheckNextMesh = false;


                    var entryVertices = JsonConvert.DeserializeObject<List<Vector3>>(entry.Value.Jvertice);
                    Vector3[] entryVerticess = entryVertices.ToArray();
                    /* Vector2[] entryPolygon = new Vector2[entryVerticess.Length];
                     Vector2[] newPolygon = new Vector2[verticess.Length];
                     for (int i=0; i<entryVerticess.Length; i++)
                     {
                         entryPolygon[i] = new Vector2(entryVerticess[i].x + entry.Value.position.x, entryVerticess[i].z + entry.Value.position.z);
                     }
                     for (int i = 0; i < verticess.Length; i++)
                     {
                         newPolygon[i] = new Vector2(verticess[i].x + position.x, verticess[i].z + position.z);
                     }

                     Vector2 worldPoint = new Vector2(position.x, position.z);
                     if(entryPolygon.Length>2 && newPolygon.Length>2)
                    if (IsPointInPolygon4(entryPolygon, worldPoint, newPolygon))
                     {*/
                    if (IsPointInPolygon(entryVerticess, entry.Value.position, position, verticess))
                    {
                        /*Debug.Log("Yes, Center is in BigMesh");
                        foreach (var point in verticess)
                        {
                            worldPoint = new Vector2(position.x + point.x, position.z + point.z);
                            if (IsPointInPolygon4(Polygon, worldPoint))
                            {
                                Debug.Log("Yes, Points are also in mesh");
                                thrown = "yes";
                                samePos = true;
                               // samePos = false;
                               //RpcRemovePlaneFromClient(id, playerNetID);
                               //RemovePlane(id, playerNetID);
                            }
                            else
                            {
                                Debug.Log("No");
                                thrown = "No";
                                samePos = false;
                            }
                        }*/
                        RpcRemovePlaneFromClient(id, playerNetID);
                        RemovePlane(id, playerNetID);
                        Debug.Log("Yes, Points are also in mesh");
                        samePos = true;
                    }
                    else
                    {
                        Debug.Log("No");
                        thrown = "No";
                        samePos = false;
                    }
                   

                   /* if (isInside(Polygon, entryVerticess.Length, worldPoint))
                     {
                         Debug.Log("Yes, Center is in BigMesh");
                         foreach (var point in verticess)
                         {
                              worldPoint = new Vector2(position.x + point.x, position.z + point.z);
                             if (isInside(Polygon, entryVerticess.Length, worldPoint))
                             {
                                 Debug.Log("Yes, Points are also in mesh");
                                 thrown = "yes";
                                 samePos = true;
                                 RpcRemovePlaneFromClient(id, playerNetID);
                                 RemovePlane(id, playerNetID);
                             }
                             else
                             {
                                 Debug.Log("No");
                                 thrown = "No";
                                 samePos = false;
                             }
                         }
                     }
                     else
                     {
                         Debug.Log("No");
                         thrown = "No";
                         samePos = false;
                     }*/
                    ///check if pos.x && .y  is inside the points+pos.x & .y
                    ///if not, go to the next 
                    ///if inside, check if all th points are inside, maybe with a bit of a posdiff



                }
            }
            if (!samePos)
            {
                Debug.Log("i was kept and updated");
                string verticesid = id.ToString() + playerNetID.ToString();
                if (verticesDict.ContainsKey(verticesid))
                {
                    /* if (verticesDict[verticesid].Jvertice != json)
                     {*/
                    PlaneData pData;
                    // Debug.Log("Updating plane in cmd");
                    pData.position = position;
                    pData.rotation = rotation;
                    pData.Jvertice = json;
                    pData.id = id;
                    pData.boundarylength = boundarylength;
                    pData.playerNetID = playerNetID;

                    verticesDict[verticesid] = pData;

                    /*foreach (var entry in verticesDict)
                    {
                        Debug.Log("Theres a plane un update");
                    }*/

                    /* Debug.Log("In AddCmd as Server");
                     Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                     Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
                    
                   UpdatePlane(verticess, position, rotation, id, boundarylength, playerNetID);
                    RpcUpdatePlaneOnClient(json, position, rotation, id, boundarylength, playerNetID);
                    /* }
                     else
                         Debug.Log("Boundary didn't change");*/
                }
                else
                {
                    Debug.Log("Tried to Update Map Info that didn't exist");
                    /* verticesid = id.ToString() + playerNetID.ToString();

                     PlaneData pData;

                     pData.position = position;
                     pData.rotation = rotation;
                     pData.Jvertice = json;
                     pData.id = id;
                     pData.boundarylength = boundarylength;
                     pData.playerNetID = playerNetID;

                     verticesDict.Add(verticesid, pData);


                     RpcAddPlaneToClient(json, position, rotation, id, boundarylength, playerNetID);*/
                }

            }
            else
            {
                samePos = false;
                Debug.Log("we dont update this");
            }
        }

    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcRemovePlaneFromClient(int id, uint playerNetID)
    {
       // if (isLocalPlayer)
       // {
            RemovePlane(id, playerNetID);
       // }
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcUpdatePlaneOnClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
       // if (isLocalPlayer)
       // {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
            UpdatePlane(verticess, position, rotation, id, boundarylength, playerNetID);
       // }
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcAddPlaneToClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
       // if (isLocalPlayer)
       // {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
            AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);
       // }
    }

    public void RemovePlane(int id, uint playerNetID)
    {
        
            string idtoDict = id.ToString() + playerNetID.ToString();

            if (planesDict.ContainsKey(idtoDict))
            {
                planesDict[idtoDict].transform.parent = null;
                Destroy(planesDict[idtoDict].gameObject);
                if (planesDict[idtoDict] != null)
                    planesDict.Remove(idtoDict);
                else
                    Debug.Log("Nem volt mit eltavolitani");
            }
        
    }

    public void UpdatePlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
         if(readImage)
        {
            Debug.Log("im in update");
            //if (isLocalPlayer)
            //{
            /*Debug.Log("In UpdatePlane as client");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
            // Debug.Log("Updating plane");
            string idtoDict = id.ToString() + playerNetID.ToString();
            Mesh mesh = new Mesh();
            if (planesDict.ContainsKey(idtoDict))
            {
                if (boundarylength > 2)
                {
                    /// Debug.Log("updating plane on client");
                    int[] tria = new int[3 * (boundarylength - 2)];
                    for (int c = 0; c < boundarylength - 2; c++)
                    {
                        tria[3 * c] = 0;
                        tria[3 * c + 1] = c + 1;
                        tria[3 * c + 2] = c + 2;
                    }
                    mesh.vertices = vertices;
                    mesh.triangles = tria;
                    mesh.RecalculateNormals();
                    planesDict[idtoDict].GetComponent<MeshFilter>().mesh = mesh;
                    planesDict[idtoDict].GetComponent<MeshRenderer>().material = mat;
                    Destroy(planesDict[idtoDict].GetComponent<MeshCollider>());
                    planesDict[idtoDict].AddComponent<MeshCollider>();
                    planesDict[idtoDict].transform.localPosition = position;
                    //planesDict[idtoDict].transform.localPosition = new Vector3(position.x, position.y, 0f);
                    planesDict[idtoDict].transform.localRotation = rotation;
                   // planesDict[idtoDict].name = thrown;
                    // NetworkIdentity netId = planesDict[idtoDict].GetComponent<NetworkIdentity>();

                    /*if (isServer)
                    {
                        NetworkServer.UnSpawn(planesDict[idtoDict].gameObject);
                       // NetworkServer.Spawn(planesDict[idtoDict]);
                    }*/
                }
            }
            else
                Debug.Log("Tried to update a plane that didn't exist");
            //}
        }
    }

    public void AddPlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        if (readImage)
        {
            Debug.Log("im in add plane");
            // if (isLocalPlayer)
            // {
            /* Debug.Log("In Add as Clihet");
             Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
             Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
            ARAnchor anchor = null;
            string idtoDict = id.ToString() + playerNetID.ToString();
        worldMap = GameObject.Find("WorldMap");
        while (worldMap == null)
            Debug.Log("coudnt find worldmap");
        if (worldMap != null)
            Debug.Log("Found worldmap");

        Mesh mesh = new Mesh();
            if (planesDict.ContainsKey(idtoDict))
            {
                Debug.Log("It tried to add a plane that already existed");
                return;
            }

            Debug.Log("Adding Plane on client");
            GameObject newMeshF = Instantiate(meshF);

            if (worldMap != null)
            {
                newMeshF.transform.parent = worldMap.transform;
                Debug.Log("Parenting it, child of worldMap: " + worldMap.transform.childCount.ToString() + " planesDict count: " + planesDict.Count);
            }
            else
            {
                
                while (worldMap != null)
                {
                worldMap = GameObject.Find("WorldMap");
                newMeshF.transform.parent = worldMap.transform;
                    Debug.Log("trying to locate worldmap");
                }
               
            }
        newMeshF.transform.parent = worldMap.transform;
        Debug.Log("Parenting it, child of worldMap: " + worldMap.transform.childCount.ToString() + " planesDict count: " + planesDict.Count);
        if (boundarylength > 2)
            {
                int[] tria = new int[3 * (boundarylength - 2)];
                for (int c = 0; c < boundarylength - 2; c++)
                {
                    tria[3 * c] = 0;
                    tria[3 * c + 1] = c + 1;
                    tria[3 * c + 2] = c + 2;
                }
                mesh.vertices = vertices;
                mesh.triangles = tria;
                mesh.RecalculateNormals();
                newMeshF.GetComponent<MeshFilter>().mesh = mesh;
                newMeshF.GetComponent<MeshRenderer>().material = mat;
                newMeshF.AddComponent<MeshCollider>();
                newMeshF.AddComponent<Rigidbody>().isKinematic = true;
                newMeshF.transform.localPosition = position;
                //planesDict[idtoDict].transform.localPosition = new Vector3(position.x, position.y, 0f);
                newMeshF.transform.localRotation = rotation;
                anchor = newMeshF.GetComponent<ARAnchor>();
                //planesDict[idtoDict].name = thrown;
                /*if(isServer)
                NetworkServer.Spawn(newMeshF);*/
                planesDict.Add(idtoDict, newMeshF);
            if (anchor == null)
                {
                    anchor = newMeshF.AddComponent<ARAnchor>();
                    /*if (anchor != null)
                    {
                        Debug.Log("Anchor added");
                    }*/
                }
                m_Anchors.Add(anchor);
                // }
            }
        }
    }
    void Update()
    {


    }

    // Given three collinear points p, q, r,
    // the function checks if point q lies
    // on line segment 'pr'
    static bool onSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) &&
            q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) &&
            q.y >= Mathf.Min(p.y, r.y))
        {
            return true;
        }
        return false;
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    static int orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float val = (q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y);

        if (val == 0)
        {
            return 0; // collinear
        }
        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    // The function that returns true if
    // line segment 'p1q1' and 'p2q2' intersect.
    static bool doIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
       // Debug.Log("p1: " + p1 + "q1: " + q1 + "p2: " + p2 + "q2: " + q2);
        // Find the four orientations needed for
        // general and special cases
        int o1 = orientation(p1, q1, p2);
        Debug.Log("orientation of o1: " + o1);

        int o2 = orientation(p1, q1, q2);
        Debug.Log("orientation of o2: " + o2);
        int o3 = orientation(p2, q2, p1);
        Debug.Log("orientation of o3: " + o3);
        int o4 = orientation(p2, q2, q1);
        Debug.Log("orientation of o4: " + o4);

        // General case
        if (o1 != o2 && o3 != o4)
        {
            return true;
            Debug.Log("General Case");
        }

        // Special Cases
        // p1, q1 and p2 are collinear and
        // p2 lies on segment p1q1
        if (o1 == 0 && onSegment(p1, p2, q1))
        {
            return true;
            Debug.Log("Special Case1");
        }

        // p1, q1 and p2 are collinear and
        // q2 lies on segment p1q1
        if (o2 == 0 && onSegment(p1, q2, q1))
        {
            return true;
            Debug.Log("Special Case2");
        }

        // p2, q2 and p1 are collinear and
        // p1 lies on segment p2q2
        if (o3 == 0 && onSegment(p2, p1, q2))
        {
            return true;
            Debug.Log("Special Case3");
        }

        // p2, q2 and q1 are collinear and
        // q1 lies on segment p2q2
        if (o4 == 0 && onSegment(p2, q1, q2))
        {
            return true;
            Debug.Log("Special Case4");
        }

        // Doesn't fall in any of the above cases
        Debug.Log("None of the above cases, they do not intersect");
        return false;
    }
    // Returns true if the point p lies
    // inside the polygon[] with n vertices
    static bool isInside(Vector2[] polygon, int n, Vector2 p)
    {
        // There must be at least 3 vertices in polygon[]
        if (n < 3)
        {
            return false;
            Debug.Log("N was smaller than 3");
        }

        // Create a point for line segment from p to infinite
        Vector2 extreme = new Vector2(Mathf.Infinity, p.y);

        // Count intersections of the above line
        // with sides of polygon
        int count = 0, i = 0;
        do
        {
            Debug.Log("in do");
            int next = (i + 1) % n;

            // Check if the line segment from 'p' to
            // 'extreme' intersects with the line
            // segment from 'polygon[i]' to 'polygon[next]'
            if (doIntersect(polygon[i], polygon[next], p, extreme))
            {

                //Debug.Log("polygon[i]: "+ polygon[i]+"polygon[next]: "+ polygon[next]+"p: "+ p+ "extreme: "+ extreme);
                // If the point 'p' is collinear with line
                // segment 'i-next', then check if it lies
                // on segment. If it lies, return true, otherwise false
                if (orientation(polygon[i], p, polygon[next]) == 0)
                {
                    Debug.Log("orientation is zero");
                    Debug.Log("on segment: " + onSegment(polygon[i], p, polygon[next]));
                    return onSegment(polygon[i], p,
                                    polygon[next]);
                }
                count++;
            }
            i = next;
        } while (i != 0);

        // Return true if count is odd, false otherwise
        return (count % 2 == 1); // Same as (count%2 == 1)
    }


    public static bool IsPointInPolygon4(Vector2[] entryPolygon, Vector2 centerPoint, Vector2[] newPolygon)
    {
        float minX = entryPolygon[0].x;
        float maxX = entryPolygon[0].x;
        float minY = entryPolygon[0].y;
        float maxY = entryPolygon[0].y;
        bool result = false;
        int j = entryPolygon.Length- 1;
        for (int i = 0; i < entryPolygon.Length; i++)
        {
            Vector2 q = entryPolygon[i];
            minX = Mathf.Min(q.x, minX);
            maxX = Mathf.Max(q.x, maxX);
            minY = Mathf.Min(q.y, minY);
            maxY = Mathf.Max(q.y, maxY);
            if (entryPolygon[i].y < centerPoint.y && entryPolygon[j].y >= centerPoint.y || entryPolygon[j].y < centerPoint.y && entryPolygon[i].y >= centerPoint.y)
            {
                if (entryPolygon[i].x + (centerPoint.y - entryPolygon[i].y) / (entryPolygon[j].y - entryPolygon[i].y) * (entryPolygon[j].x- entryPolygon[i].x) < centerPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        if(!result)
        return result;
        else
        {
           Debug.Log("center was inside");
            for (int i = 0; i < newPolygon.Length; i++)
            {
                if (newPolygon[i].x < minX || newPolygon[i].x > maxX || newPolygon[i].y < minY || newPolygon[i].y > maxY)
                {
                    return false;
                    Debug.Log("Point wasnt inside");
                }
            }
            Debug.Log("Point was inside");
            return true;
        }
    }

    public static bool IsPointInPolygon(Vector3[] entryPolygon, Vector3 entryCenter, Vector3 centerPoint, Vector3[] newPolygon)
    {
        float minX = entryPolygon[0].x+ entryCenter.x;
        float maxX = entryPolygon[0].x + entryCenter.x;
        float minY = entryPolygon[0].z + entryCenter.z;
        float maxY = entryPolygon[0].z + entryCenter.z;
        bool result = false;
        int j = entryPolygon.Length - 1;
        for (int i = 0; i < entryPolygon.Length; i++)
        {
            Vector3 q = entryPolygon[i]+ entryCenter;
            minX = Mathf.Min(q.x, minX);
            maxX = Mathf.Max(q.x, maxX);
            minY = Mathf.Min(q.z, minY);
            maxY = Mathf.Max(q.z, maxY);
            if (entryCenter.z+entryPolygon[i].z < centerPoint.z && entryCenter.z+entryPolygon[j].z >= centerPoint.z || entryCenter.z+entryPolygon[j].z < centerPoint.z && entryCenter.z + entryPolygon[i].z >= centerPoint.z)
            {
                if (entryCenter.x + entryPolygon[i].x + (centerPoint.z - entryCenter.z + entryPolygon[i].z) / ((entryCenter.z + entryPolygon[j].z) - (entryCenter.z + entryPolygon[i].z)) * ((entryCenter.x + entryPolygon[j].x) - (entryCenter.x + entryPolygon[i].x)) < centerPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        if (!result)
            return result;
        else
        {
            Debug.Log("center was inside");
            for (int i = 0; i < newPolygon.Length; i++)
            {
                if (centerPoint.x+newPolygon[i].x < minX || centerPoint.x + newPolygon[i].x > maxX || centerPoint.z + newPolygon[i].z < minY || centerPoint.z + newPolygon[i].z > maxY)
                {
                    Debug.Log("Point wasnt inside");
                    return false;
                    
                }
            }
            Debug.Log("Point was inside");
            return true;
        }
    }
}
