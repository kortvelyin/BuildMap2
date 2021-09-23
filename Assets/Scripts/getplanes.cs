using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;


/*[System.Serializable]
public struct PlaneData// egy változó a plane-nek a pontjaira, és a plane id-jára
    {
    public int planeId;
    public string Jvertice;
    public NetworkInstanceId netId;
    
    /* public PlaneData(string Jvertice, int planeId, NetworkInstanceId netId)
    {
        this.planeId = planeId;
        this.netId = netId;
        this.Jvertice = Jvertice;
    }
    }*/


public class getplanes : NetworkBehaviour
{
    public struct PlaneData// egy változó a plane-nek a pontjaira, és a plane id-jára
    {
        public Vector3 position;
        public string Jvertice;
        public Quaternion rotation;
        public int id;
        public int boundarylength;
        public NetworkInstanceId playerNetID;


        public PlaneData(string Jvertice, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
        {
            this.position = position;
            this.rotation = rotation;
            this.Jvertice = Jvertice;
            this.id = id;
            this.boundarylength = boundarylength;
            this.playerNetID = playerNetID;

        }
    }
        //  public SyncListPlaneData onnewPlayer= new SyncListPlaneData();

        // public class SyncListPlaneData : SyncListStruct<PlaneData> { }

        ARAnchorManager m_AnchorManager;
    List<ARAnchor> m_Anchors = new List<ARAnchor>();
    Dictionary<string, PlaneData> verticesDict = new Dictionary<string, PlaneData>();
    Dictionary<string, GameObject> planesDict = new Dictionary<string, GameObject>();
    //public static HashSet<Player> ActivePlayers = new HashSet<Player>();
    [SyncVar]
    int PlayerId = 0;
  
    int ownID;
    private NetworkInstanceId playerNetID;
    //int playerid = ConnectedPlayer.playerId;
    //[SyncVar]
    //public int[] tria;
    public GameObject Cube;
    public GameObject meshF;
    public ARPlaneManager planeManager;
    //public ARMeshManager m_MeshManager;
    public MeshFilter meshFilterr;
    public Material mat;
    //float width = 1;
    //float height = 1;
    //ARPlaneManager m_ARPlaneManager;
    ARPlane planeNew;
    public Text debug;
    int idk;
    Vector3 smt;
    Quaternion identity;
    Transform any;
    //bool isit = true;
    Vector2[] smt2;
    List<Vector2> list;
    Unity.Collections.NativeArray<Vector2> vectors;
    PlayerObject playerobjscript;
    Vector3[] pontok = new Vector3[11];
    //float speed = 100.0f;
    // Start is called before the first frame update

    //PlayerObject playerobjscript;

    //tuner.EnableLocalEndpoint();
    void Start()
    {
        playerNetID = GetComponent<NetworkIdentity>().netId;
        m_AnchorManager = GetComponent<ARAnchorManager>();
       
        
        
        if (isLocalPlayer)
        {
            planeManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
            if (planeManager != null)
            {
                Debug.Log("Plane manager found");
            }


            planeManager.planesChanged += OnPlanesChanged;
            Debug.Log("Subscribed to event");

        }
       // CmdAskForPlanesFromServerOnStart(connectionToClient);

    }

   /* public void GetPlanesFromServer()
    {
        CmdAskForPlanesFromServerOnStart(connectionToClient);
    }

    [Command]
    void CmdAskForPlanesFromServerOnStart(NetworkConnection connectionToClient)
    {
        Debug.Log("asking plane informations from server");

        Debug.Log("CmdGetPLanesFrom... was called");
        Debug.Log(" vertices Count on Server when asking for the entries: " + verticesDict.Count);
        TargetChecking(connectionToClient, verticesDict.Count);
        foreach (var entry in verticesDict)
        {
            Debug.Log("There was something on the server");
            //TargetCreatePlanesFromServer(connectionToClient, entry.Value.Jvertice, entry.Value.position, entry.Value.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
        }

    }

    [TargetRpc]
    public void TargetCreatePlanesFromServer(NetworkConnection target, string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);

    }

    [TargetRpc]
    public void TargetChecking(NetworkConnection target, int idk)
    {
        Debug.Log("I'm back name  :" + target + " , and verticesnumber " + idk);
    }
   */

    void OnDisable()
    {
        if (planeManager == null)
            Debug.Log("too early2");
        else
        {
            planeManager.planesChanged -= OnPlanesChanged;
            Debug.Log("Unsubscribed to event");
        }

        /* foreach (ARPlane plane in planeManager.trackables)
         {
             CmdRemoveMapInfo(plane.GetInstanceID(), playerNetID);
         }*/
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
    {

        // Debug.Log("Planes Changed");
        if (!isLocalPlayer) return;
        // Debug.Log("We are local players");
        foreach (ARPlane plane in eventArgs.removed)
        {
            // plane.boundaryChanged -= UpdatePlane;
            Debug.Log("PlanesRemoved, unsubscribed form event(probably)");
            CmdRemoveMapInfo(plane.GetInstanceID(), playerNetID);
            // CmdRemovePlaneFromServer(plane.GetInstanceID(), playerNetID);

        }

        foreach (ARPlane plane in eventArgs.updated)
        {

           
            vectors = plane.boundary;

            Vector3[] vertices = new Vector3[plane.boundary.Length];
            int i;
            for (i = 0; i < plane.boundary.Length; i++)
            {
                vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
            }

            string json = JsonConvert.SerializeObject(vertices);
            CmdUpdateMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
        }

            foreach (ARPlane plane in eventArgs.added)
        {

            /* if (PlanesOnStart)
             {
                 CmdAskForPlanesFromServerOnStart(connectionToClient);
             }*/

            //plane.boundaryChanged += UpdatePlane;
            Debug.Log("PlanesAdded and subscibed to event (probably)");

            vectors = plane.boundary;

            Vector3[] vertices = new Vector3[plane.boundary.Length];
            int i;
            for (i = 0; i < plane.boundary.Length; i++)
            {
                vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
            }

            string json = JsonConvert.SerializeObject(vertices);

            // CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
            // CmdAddPlaneToServer(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
            CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
            //CmdCreatePlaneFromData(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
        }




    }

    [Command] //Serverre küldi a Plane adatokat
    public void CmdAddMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {


        string verticesid = id.ToString() + playerNetID.ToString();
        if (verticesDict.ContainsKey(verticesid))
        {
            Debug.Log("New Map Info existed already");
        }
        else
        {
            foreach (var entry in verticesDict)
            {
                Debug.Log("Position of the vertices until now: " + entry.Value.position);
            }
            Debug.Log("New Map Info added in cmd");
            PlaneData pData;

            pData.position = position;
            pData.rotation = rotation;
            pData.Jvertice = json;
            pData.id = id;
            pData.boundarylength = boundarylength;
            pData.playerNetID = playerNetID;

            verticesDict.Add(verticesid, pData);
            Debug.Log("VerticesDict number: " + verticesDict.Count);
            RpcAddPlaneToClient(json, position, rotation, id, boundarylength, playerNetID);
        }

    }

    [Command]
    public void CmdRemoveMapInfo(int id, NetworkInstanceId playerNetID)
    {

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

    [Command]
    public void CmdUpdateMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {

        foreach (var entry in verticesDict)
        {
            ///Let's check if it's appr. the same
            /////if yes, then drop the nwe one, becuse its more efficient probably
            ///if no, go on with the thingie
            /////cant use PositionsAreApproximatelyEqual(List<Vector3>, List<Vector3>, Single)
            ///because we don't have planes, only data's
            ///we cant have planes in here
            ///we could check the difference with different numbers in different directions
        }

        string verticesid = id.ToString() + playerNetID.ToString();
        if (verticesDict.ContainsKey(verticesid))
        {
             if (verticesDict[verticesid].Jvertice != json)
             {
            PlaneData pData;
            Debug.Log("Updating plane in cmd");
            pData.position = position;
            pData.rotation = rotation;
            pData.Jvertice = json;
            pData.id = id;
            pData.boundarylength = boundarylength;
            pData.playerNetID = playerNetID;

            verticesDict[verticesid] = pData;

            foreach (var entry in verticesDict)
            {
                Debug.Log("Theres a plane un update");
            }

            Debug.Log("number of entries: " + verticesDict.Count);
            RpcUpdatePlaneOnClient(json, position, rotation, id, boundarylength, playerNetID);
             }
             else
                 Debug.Log("Boundary didn't change");
        }
        else
        {
            Debug.Log("Tried to Update Map Info that didn't exist");
        }

    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcRemovePlaneFromClient(int id, NetworkInstanceId playerNetID)
    {
        RemovePlane(id, playerNetID);
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcUpdatePlaneOnClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        UpdatePlane(verticess, position, rotation, id, boundarylength, playerNetID);
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcAddPlaneToClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);
    }

    public void RemovePlane(int id, NetworkInstanceId playerNetID)
    {
        string idtoDict = id.ToString() + playerNetID.ToString();

        if (planesDict[idtoDict] != null)
        {
            DestroyImmediate(planesDict[idtoDict], true);
            planesDict.Remove(idtoDict);
        }
    }

    public void UpdatePlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
        // Debug.Log("Updating plane");
        string idtoDict = id.ToString() + playerNetID.ToString();
        Mesh mesh = new Mesh();
        if (planesDict.ContainsKey(idtoDict))
        {
            if (boundarylength > 2)
            {
                Debug.Log("updating plane on client");
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
                planesDict[idtoDict].transform.position = position;
                planesDict[idtoDict].transform.rotation = rotation;
            }
        }
        else
            Debug.Log("Tried to update a plane that didn't exist");

    }

    public void AddPlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
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
        planesDict.Add(idtoDict, newMeshF);
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
            newMeshF.transform.position = position;
            newMeshF.transform.rotation = rotation;
            anchor = newMeshF.GetComponent<ARAnchor>();
            if (anchor == null)
            {
                anchor = newMeshF.AddComponent<ARAnchor>();
                /*if (anchor != null)
                {
                    Debug.Log("Anchor added");
                }*/
            }
            m_Anchors.Add(anchor);
        }
    }
    void Update()
    {

        
    }

    /// A függvény ami kezeli a kilensen a mesh létrehozást
    public void CreatePlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {

        ARAnchor anchor = null;
        string idtoDict = id.ToString() + playerNetID.ToString();
        
        Mesh mesh = new Mesh();
        if (planesDict.ContainsKey(idtoDict))
        {
            if (planesDict[idtoDict].GetComponent<MeshFilter>().mesh.vertices != vertices)
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
                planesDict[idtoDict].GetComponent<MeshFilter>().mesh = mesh;
                planesDict[idtoDict].GetComponent<MeshRenderer>().material = mat;
                Destroy(planesDict[idtoDict].GetComponent<MeshCollider>());
                planesDict[idtoDict].AddComponent<MeshCollider>();
                //if (planesDict[idtoDict].GetComponent<MeshCollider>()==false)
                  //planesDict[idtoDict].AddComponent<MeshCollider>();
                
                //planesDict[idtoDict].AddComponent<Rigidbody>().isKinematic = true;
                planesDict[idtoDict].transform.position = position;
                planesDict[idtoDict].transform.rotation = rotation;

                //DestroyImmediate(planesDict[idtoDict], true);
               // planesDict.Remove(idtoDict);*/
            }
            //DestroyImmediate(planesDict[idtoDict], true);
           // planesDict.Remove(idtoDict);
        }
        else
        {
            Debug.Log("New mesh");
            GameObject newMeshF = Instantiate(meshF);
            planesDict.Add(idtoDict, newMeshF);
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
            //newMeshF.GetComponent<MeshCollider>().
            newMeshF.AddComponent<Rigidbody>().isKinematic = true;
            newMeshF.transform.position = position;
            newMeshF.transform.rotation = rotation;
            anchor = newMeshF.GetComponent<ARAnchor>();
            if (anchor == null)
            {
                anchor = newMeshF.AddComponent<ARAnchor>();
                if (anchor != null)
                {
                    //Debug.Log("anchor worked");
                    //worked++;
                    // anchorThingie.text = worked.ToString();
                }
            }
            m_Anchors.Add(anchor);
        }
        //debug.text = "in create plane" + vertices[0].ToString() + vertices[1].ToString() + vertices[2].ToString() + "position" + position.ToString() + "rotation" + rotation.ToString();
    }

  

    /// <summary>
    /// Előállítja a pontok tömb alapján a mesh-t
    /// </summary>
    public void MakeMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[11];
        for (int z = 0; z < 5; z++)
        {
            // debug.text = "I'm working1";
            GameObject newMeshF = Instantiate(meshF);

            int i;
            for (i = 0; i < 11; i++)
                {
                    vertices[i] = new Vector3(pontok[i].x, pontok[i].y, z);
                }
            mesh.vertices = vertices;

            int[] tria = new int[3 * 10];
            for (int c = 0; c < 9; c++)
            {
                tria[3 * c] = 0;
                tria[3 * c + 1] = c + 1;
                tria[3 * c + 2] = c + 2;
            }
            tria[(3 * 9)] = 0;
            tria[(3 * 9) + 1] = 10;
            tria[(3 * 9) + 2] = 1;
            mesh.triangles = tria;
            mesh.RecalculateNormals();
            NetworkServer.Spawn(newMeshF);
            newMeshF.GetComponent<MeshFilter>().mesh = mesh;
            newMeshF.GetComponent<MeshRenderer>().material = mat;

        }
    }

    //kintről kapja az adatokat, nem benne van
    public void MakeMeshextra(Vector3[] verticess, int[] triaa)
    {



        Mesh mesh = new Mesh();
        GameObject newMeshFF = Instantiate(meshF);

        mesh.vertices = verticess;
        mesh.triangles = triaa;
        mesh.RecalculateNormals();
        //NetworkServer.Spawn(newMeshF);
        newMeshFF.GetComponent<MeshFilter>().mesh = mesh;
        newMeshFF.GetComponent<MeshRenderer>().material = mat;


    }



    /*[Command]
       void CmdsrvrmeshToPlane(float[] boundaryX, float[] boundaryY, Vector3 position, Quaternion rotation, int id, int boundarylength)
      {
          Vector3[] vertices = new Vector3[boundarylength];
          int i;
          for (i = 0; i < boundarylength; i++)
          {
              vertices[i] = new Vector3(boundaryX[i], 0, boundaryY[i]);
          }

          Rpcmeshextra(vertices, position, rotation, id, boundarylength);
      }
      */


    [Command] //Serverre küldi a Plane adatokat
    public void CmdCreatePlaneFromData(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
        PlaneData pData;
       
        pData.position = position;
        pData.rotation = rotation;
        pData.Jvertice = json;
        pData.id = id;
        pData.boundarylength = boundarylength;
        pData.playerNetID = playerNetID;
        string verticesid = id.ToString() + playerNetID.ToString();
        if (verticesDict.ContainsKey(verticesid))
        {
            if (verticesDict[verticesid].Jvertice != json)
            {
                verticesDict[verticesid] = pData;
                //Debug.Log("chasnged");
            }

        }
        else
        {
            Debug.Log("new");
            verticesDict.Add(verticesid, pData);
        }
        
        /*PlaneData pd;
        pd.Jvertice = json;
        pd.netId = playerNetID;
        pd.planeId = id;

        onnewPlayer.Add(pd);*/



        //Vector3[] verticess = new Vector3[boundarylength];
        //verticess = meshprefab.GetComponent<MeshFilter>().mesh.vertices;
        Rpcmeshextra(json, position, rotation, id, boundarylength, playerNetID);
        //int[] tria = new int[3 * (boundarylength - 2)];
        //tria= meshprefab.GetComponent<MeshFilter>().mesh.triangles;
        //debug.text = "I work"+json;
        // RpcmeshSomelvnevnlv(verticess, tria, position, rotation, id, boundarylength);
        //MakeMesh();
    }
   

    [ClientRpc]//Plane adatból állítja elő a mesh-t
    void Rpcmeshextra(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {

        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        //Debug.Log(name);
        // Debug.Log(id);
        //isit = false;
        //debug.text = "Rpc2"+vertices[0].ToString()+ vertices[2].ToString();
        // MakeMesh();
        CreatePlane(verticess, position, rotation, id, boundarylength, playerNetID);
        //MakeMeshextra(vertices,tria);
    }

    [ClientRpc]//Mit csinál az RPC hívással
    public void Rpcmesh()
    {
        //once = false;
        debug.text = "Rpc";
        MakeMesh();
    }
    [Command]//Még nincs playerID
    public void Cmdplayerplus()
    {
        PlayerId++;
    }

    [Command]//Még nincs playerID
    public void CmdTryingToCallCommand()
    {
        RpcWritesmt();
        //Rpcmesh();
    }
    [ClientRpc]
    public void RpcWritesmt()
    {
        debug.text = "i can call an Rpc and therefore a command too in getplanes";
    }

   
}
