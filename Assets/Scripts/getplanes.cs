using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
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
   Dictionary<string, GameObject> planesDict = new Dictionary<string, GameObject>();
    private uint playerNetID;
    public GameObject StepChild;  //to get a position  and rotation
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
  public bool readImage = false;
   

    //tuner.EnableLocalEndpoint();
    void Start()
    {
      
        if (isLocalPlayer)
        {
           // GameObject newCanvas = Instantiate(canvas); 
            playerNetID = GetComponent<NetworkIdentity>().netId;
            m_AnchorManager = GetComponent<ARAnchorManager>();

            worldMap = GameObject.Find("WorldMap");
            while (worldMap == null)
                Debug.Log("coudnt find worldmap");
            planeManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
            if (planeManager != null)
            {
                Debug.Log("Plane manager found");
            }
            aRTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
            StepChild= GameObject.Find("StepChild");
            planeManager.planesChanged += OnPlanesChanged;
            Debug.Log("Subscribed to event");
            aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
            //CmdAskForPlanesFromServerOnStart(planesDict.Count);
            planeManager.enabled = false;


#if UNITY_EDITOR


            readImage = true;
            CmdAskForPlanesFromServerOnStart(this.gameObject, planesDict.Count);
            Debug.Log("asked for planes on start");
            planeManager.enabled = true;

#endif

        }
       

    }


    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        if (isLocalPlayer)
        {
            if (worldMap != null)
            {
                foreach (var TrackedImage in args.added)
                {
                    worldMap.transform.position = TrackedImage.transform.position;
                    worldMap.transform.rotation = TrackedImage.transform.rotation;

                    Debug.Log("Picture is seen and name: " + TrackedImage.name);
                    readImage = true;
                    CmdAskForPlanesFromServerOnStart(this.gameObject, planesDict.Count);
                    planeManager.enabled = true;
                   
                    
                }


               /*foreach (var TrackedImage in args.updated)
                {
                    if (TrackedImage == Image)
                    {
                        worldMap.transform.position = TrackedImage.transform.position;
                        worldMap.transform.rotation = TrackedImage.transform.rotation;

                        Debug.Log("Picture is seen and name: " + TrackedImage.name);
                    }
                }*/
            }
        }

    }



    public void PutWorldMapToOrigo()
    {
        //if (isLocalPlayer)
       // {
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
        //}
    }
    public void CallForThePlanes()
    {
        /*if (!isLocalPlayer)
            return;*/


        if (isLocalPlayer)
        {
            Debug.Log("In callforplanes localPlayer");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
            CmdAskForPlanesFromServerOnStart(this.gameObject ,planesDict.Count);
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
            
            if(PlanesCount<1)
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

                /* if (PlanesOnStart)
                 {
                     CmdAskForPlanesFromServerOnStart(connectionToClient);
                 }*/
                Debug.Log("In AddPlaneEvent as client");
                Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
                plane.boundaryChanged += UpdatePlane;
                Debug.Log("PlanesAdded and subscibed to event (probably)");

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

                string json = JsonConvert.SerializeObject(vertices);
                /* Debug.Log("In updatePlaneEvent as client");
                 Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                 Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/

                StepChild.transform.position = eventArgs.plane.transform.position;
                StepChild.transform.rotation = eventArgs.plane.transform.rotation;
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
            foreach (var entry in verticesDict)
            {
                if (entry.Value.playerNetID != playerNetID)
                {
                    if ((entry.Value.position - position).magnitude < PosDiff && Quaternion.Angle(entry.Value.rotation, rotation) < RotDiff)
                    {
                        
                            Debug.Log("I wouldnt add this ");
                            samePos = true;
                            break;
                        }
                }




                ///Let's check if it's appr. the same
                /////if yes, then drop the nwe one, becuse its more efficient probably
                ///if no, go on with the thingie
                /////cant use PositionsAreApproximatelyEqual(List<Vector3>, List<Vector3>, Single)
                ///because we don't have planes, only data's
                ///we cant have planes in here
                ///we could check the difference with different numbers in different directions
            }


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

                /*Debug.Log("In Add Cmd as Server");
                Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
                RpcAddPlaneToClient(json, position, rotation, id, boundarylength, playerNetID);
                 }
            }
            else
                samePos = false;
        
        
        
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

            foreach (var entry in verticesDict)
             {
                if (entry.Value.playerNetID != playerNetID)
                {
                    if ((entry.Value.position - position).magnitude < PosDiff && Quaternion.Angle(entry.Value.rotation, rotation) < RotDiff)
                    {
                       

                        //if (PositionApprSame(position, entry.Value.position, PosDiff))
                        //{
                        Debug.Log("I would leave out this one");
                        samePos = true;
                        break;
                        /*string Uverticesid = id.ToString() + playerNetID.ToString();
                        if (verticesDict.ContainsKey(Uverticesid))
                        {
                            verticesDict.Remove(Uverticesid);
                            RpcRemovePlaneFromClient(id, playerNetID);
                        }
                        else
                        {
                            Debug.Log("Tried to Remove a Map Info that didn't exist");
                        }*/

                    }
                }
            }
            ///Let's check if it's appr. the same
            /////if yes, then drop the nwe one, becuse its more efficient probably
            ///if no, go on with the thingie
            /////cant use PositionsAreApproximatelyEqual(List<Vector3>, List<Vector3>, Single)
            ///because we don't have planes, only data's
            ///we cant have planes in here
            ///we could check the difference with different numbers in different directions
             
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
                samePos = false;
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
        if (isLocalPlayer)
        {
            string idtoDict = id.ToString() + playerNetID.ToString();

            if (planesDict[idtoDict] != null)
            {
                planesDict[idtoDict].transform.parent = null;
                Destroy(planesDict[idtoDict]);
                if (planesDict[idtoDict] != null)
                    planesDict.Remove(idtoDict);
                else
                    Debug.Log("Nem volt mit eltavolitani");
            }
        }
    }

    public void UpdatePlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        //if (readImage)
        //{
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
                    planesDict[idtoDict].transform.localRotation = rotation;
                }
            }
            else
                Debug.Log("Tried to update a plane that didn't exist");
            //}
       // }
    }

    public void AddPlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
       // if (readImage)
       // {
            Debug.Log("im in add plane");
            // if (isLocalPlayer)
            // {
            /* Debug.Log("In Add as Clihet");
             Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
             Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
            ARAnchor anchor = null;
            string idtoDict = id.ToString() + playerNetID.ToString();


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
                newMeshF.transform.localRotation = rotation;
                anchor = newMeshF.GetComponent<ARAnchor>();
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
       // }
    }
    void Update()
    {


    }



}
