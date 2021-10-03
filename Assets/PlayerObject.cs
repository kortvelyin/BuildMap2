using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
//using UnityEngine.UI;

public class PlayerObject : NetworkBehaviour
{
   // public Button moveBtn;
    public GameObject PlayerUnitPrefab;
    //public GameObject myPlayerUnit;
    ObjectSpawner spawner;
   
    public GameObject spawned;
    
    // Start is called before the first frame update
    void Start()
    {

        if (isLocalPlayer == false)
            return;


        CmdSpawnMyUnit();
       
        spawner = FindObjectOfType<ObjectSpawner>();


    }
   
   
   
    [Command]
    public void CmdSpawnMyUnit()
    {
       GameObject obj= Instantiate(PlayerUnitPrefab);
        //NetworkServer.Spawn(obj);
        //myPlayerUnit = obj;
        NetworkServer.SpawnWithClientAuthority(obj, connectionToClient);
    }
    [System.NonSerialized]
    public bool once = false;

    // Update is called once per frame
    void Update()
    {
        
        if (isLocalPlayer == false)
        {
            return;
        }


       if (once)
        {
            NetworkServer.Spawn(spawned);
            once = false;
        }

        /* if (Input.GetKeyDown(KeyCode.Space))
         {
             PlayerUnitPrefab.transform.Translate(0, 1, 0);
         }

         moveBtn = GetComponent<Button>();
         //moveBtn.GetComponent<NetworkIdentity>().AssignClientAuthority(PlayerUnitPrefab.GetComponent<NetworkIdentity>().connectionToClient);
         moveBtn.onClick.AddListener(RpcMoveObject);*/

    }


    /*[ClientRpc]
    void RpcMoveObject()
    {
        PlayerUnitPrefab.transform.Translate(0, 1, 0);
    }*/
   /* [Command]
    void CmdMove()
    {
        this.transform.Translate(1, 1, 2);
    }

    public void Move()
    {
        CmdMove();
    }*/


}
