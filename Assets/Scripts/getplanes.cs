using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.Networking;
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
    private GameObject origo;
    private ARTrackedImageManager aRTrackedImageManager;
    public GameObject worldMap;
    private Vector3 newPlanePos;
    private Quaternion newPlaneRot;
    // public GameObject canvas;
    //float speed = 100.0f;
    // Start is called before the first frame update

    //PlayerObject playerobjscript;

    //tuner.EnableLocalEndpoint();
    void Start()
    {

        if (isLocalPlayer)
        {
            //GameObject newCanvas = Instantiate(canvas); 
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

            planeManager.planesChanged += OnPlanesChanged;
            Debug.Log("Subscribed to event");
            aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
            // CmdAskForPlanesFromServerOnStart();
        }
        // CmdAskForPlanesFromServerOnStart(connectionToClient);

    }

    /// <summary>
    /// origo manuálisan és manuálisan rárakni a worldmapet
    /// nézni hogy bemegy-e alá
    /// nézni hogy megtalálja-e a kockát
    /// mekkora a kocka
    /// planes dictionary check if we can access it, then write addRpc for that scenario
    /// im,portant thing is to make it work
    /// check the vertice dict from two client 
    /// </summary>
    /// <param name="args"></param>

    /*public void GetPlanesFromServer()
     {
         if (!isLocalPlayer)
             return;
         CmdAskForPlanesFromServerOnStart(connectionToClient);
     }
    */

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
                }
            }
        }

    }



    public void PutWorldMapToOrigo()
    {
        if (isLocalPlayer)
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
            CmdAskForPlanesFromServerOnStart();
        }


        /*  if(isServer)
          {
              Debug.Log("In callforplanes as server");
              Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
              Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);

          }*/
        // CmdAskForPlanesFromServerOnStart();


        /* foreach (ARPlane plane in planeManager.trackables)
         {
             vectors = plane.boundary;

             Vector3[] vertices = new Vector3[plane.boundary.Length];
             int i;
             for (i = 0; i < plane.boundary.Length; i++)
             {
                 vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
             }

             string json = JsonConvert.SerializeObject(vertices);
             RpcAddPlaneToClient(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
         }*/

    }

    /* void OnPlayerConnected()
     {
         Debug.Log("OnPlayerConnected vertices Count on Server when asking for the entries: " + verticesDict.Count);
         // CmdAskForPlanesFromServerOnStart();
         foreach (ARPlane plane in planeManager.trackables)
         {
             vectors = plane.boundary;

             Vector3[] vertices = new Vector3[plane.boundary.Length];
             int i;
             for (i = 0; i < plane.boundary.Length; i++)
             {
                 vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
             }

             string json = JsonConvert.SerializeObject(vertices);
             RpcAddPlaneToClient(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
         }
     }*/


    /*  public  void OnServerConnect(NetworkConnection Conn)
      {
          if (Conn.hostId >= 0)
          {
              Debug.Log("New Player has joined");
          }
      }*/

    [Command]
    void CmdAskForPlanesFromServerOnStart()
    {

        if (isServer)
        {
            Debug.Log("In CmdAskforplanews as Server");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
        }
        /* Debug.Log("Writing out number of planes: " + planesDict.Count);

         foreach ( var entry in planesDict)
         {
             //RpcAddPlaneToClient(entry.Value.gameObject.GetComponent<MeshFilter>().mesh.vertices, entry.Value.gameObject.transform.position, entry.Value.gameObject.transform.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
         }
        */
        /*Debug.Log("asking plane informations from server");
        foreach (ARPlane plane in planeManager.trackables)
        {
            vectors = plane.boundary;

            Vector3[] vertices = new Vector3[plane.boundary.Length];
            int i;
            for (i = 0; i < plane.boundary.Length; i++)
            {
                vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
            }

            string json = JsonConvert.SerializeObject(vertices);
            RpcAddPlaneToClient(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
        }

        Debug.Log(" vertices Count on Server when asking for the entries: " + verticesDict.Count);
       // TargetChecking(connectionToClient, verticesDict.Count);
        foreach (var entry in verticesDict)
        {
            Debug.Log("There was something on the server");
           
            RpcAddPlaneToClient(entry.Value.Jvertice, entry.Value.position, entry.Value.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
            //TargetCreatePlanesFromServer(connectionToClient, entry.Value.Jvertice, entry.Value.position, entry.Value.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
        }*/

    }

    /*  [TargetRpc]
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
      }*/




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
             CmdRemoveMapInfo(plane.GetInstanceID(), playerNetID);
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
                /* if(worldMap==null)
                 {
                     worldMap = GameObject.Find("WorldMap");
                     Debug.Log("Trying to find worldmap");
                     newPlanePos = plane.transform.position - worldMap.transform.position;
                     newPlaneRot = Quaternion.Euler(plane.transform.rotation.eulerAngles - worldMap.transform.rotation.eulerAngles);
                 }
                 else
                 {
                     newPlanePos = plane.transform.position-worldMap.transform.position;
                     newPlaneRot = Quaternion.Euler(plane.transform.rotation.eulerAngles - worldMap.transform.rotation.eulerAngles);

                 }*/
                // CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                // CmdAddPlaneToServer(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                // CmdAddMapInfo(json, newPlanePos, newPlaneRot, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                //CmdCreatePlaneFromData(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
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
            Debug.Log("In updatePlaneEvent as client");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
            /*if (worldMap == null)
            {
                worldMap = GameObject.Find("WorldMap");
                Debug.Log("Trying to find worldmap");
                newPlanePos = eventArgs.plane.transform.position - worldMap.transform.position;
                newPlaneRot = Quaternion.Euler(eventArgs.plane.transform.rotation.eulerAngles - worldMap.transform.rotation.eulerAngles);
            }
            else
            {
                newPlanePos = eventArgs.plane.transform.position - worldMap.transform.position;
                newPlaneRot = Quaternion.Euler(eventArgs.plane.transform.rotation.eulerAngles - worldMap.transform.rotation.eulerAngles);

            }*/
            CmdUpdateMapInfo(json, eventArgs.plane.transform.position, eventArgs.plane.transform.rotation, eventArgs.plane.GetInstanceID(), eventArgs.plane.boundary.Length, playerNetID);
            // CmdUpdateMapInfo(json, newPlanePos, newPlaneRot, eventArgs.plane.GetInstanceID(), eventArgs.plane.boundary.Length, playerNetID);
        }
    }


    [Command] //Serverre küldi a Plane adatokat
    public void CmdAddMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {

        if (isServer)
        {
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

                Debug.Log("In Add Cmd as Server");
                Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
                RpcAddPlaneToClient(json, position, rotation, id, boundarylength, playerNetID);
            }
        }

    }

    [Command]
    public void CmdRemoveMapInfo(int id, NetworkInstanceId playerNetID)
    {
       // if (isServer)
        //{
            Debug.Log("In RemoveCmd as Server");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
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
       // }
    }

    [Command]
    public void CmdUpdateMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
       // if (isServer)
      //  {

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

                Debug.Log("In AddCmd as Server");
                Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
                RpcUpdatePlaneOnClient(json, position, rotation, id, boundarylength, playerNetID);
                /* }
                 else
                     Debug.Log("Boundary didn't change");*/
            }
            else
            {
                Debug.Log("Tried to Update Map Info that didn't exist");
            }
       // }

    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcRemovePlaneFromClient(int id, NetworkInstanceId playerNetID)
    {
       // if (isLocalPlayer)
       // {
            RemovePlane(id, playerNetID);
       // }
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcUpdatePlaneOnClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
       // if (isLocalPlayer)
       // {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
            UpdatePlane(verticess, position, rotation, id, boundarylength, playerNetID);
       // }
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcAddPlaneToClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
       // if (isLocalPlayer)
       // {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
            AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);
       // }
    }

    public void RemovePlane(int id, NetworkInstanceId playerNetID)
    {
        if (isLocalPlayer)
        {
            string idtoDict = id.ToString() + playerNetID.ToString();

            if (planesDict[idtoDict] != null)
            {
                DestroyImmediate(planesDict[idtoDict], true);
                planesDict.Remove(idtoDict);
            }
        }
    }

    public void UpdatePlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
        if (isLocalPlayer)
        {
            Debug.Log("In UpdatePlane as client");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
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
                    planesDict[idtoDict].transform.position = position;
                    planesDict[idtoDict].transform.rotation = rotation;
                }
            }
            else
                Debug.Log("Tried to update a plane that didn't exist");
        }

    }

    public void AddPlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, NetworkInstanceId playerNetID)
    {
        if (isLocalPlayer)
        {
            Debug.Log("In Add as Clihet");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
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
                Debug.Log("Parenting it, child of worldMap: "+worldMap.transform.GetChildCount().ToString());
            }
            else
            {
                worldMap = GameObject.Find("WorldMap");
                if (worldMap != null)
                {
                    newMeshF.transform.parent = worldMap.transform;
                    Debug.Log("did it");
                }
                Debug.Log("trying");
            }
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
    }
    void Update()
    {


    }



}
