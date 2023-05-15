using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// Tutorial link: https://www.youtube.com/watch?v=3yuBOB3VrCk
public class PlayerNetwork : NetworkBehaviour
{
    // The only limitation is you cannot use an object here, just primitive types (int, bool, float, struct)
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    private NetworkVariable<MyCustomData> myStruct = new NetworkVariable<MyCustomData>(
        new MyCustomData {
            _int = 1,
            _bool = true,
            _message = "First Message"
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private GameObject spawnObject;
    private GameObject newSpawn;
    
    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes _message;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref _message);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        ReadKeyE();
        ReadKeyF();
        ReadKeyR();
        ReadKeyT();
        ReadKeyI();
        ReadKeyK();
        MovePlayer();
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + " number is " + randomNumber.Value);
        };
        
        myStruct.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + " number is " + myStruct.Value._int + 
                      " bool is " + myStruct.Value._bool +
                      " message is " + myStruct.Value._message);
        };
    }

    private void MovePlayer()
    {
        var moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        const float moveSpeed = 3f;
        transform.position += moveDir * (moveSpeed * Time.deltaTime);
    }
    
    private void ReadKeyE()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            randomNumber.Value = Random.Range(0, 100);
        }
    }
    
    private void ReadKeyF()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            myStruct.Value = new MyCustomData
            {
                _int = Random.Range(0, 100),
                _bool = Random.Range(0,2) == 0,
                _message = "New message"
            };
        }
    }
    
    private void ReadKeyR()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestServerRpc("Test Server RPC 123");
        }
    }
    
    private void ReadKeyT()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestClientRpc("Test Client RPC 456");
        }
    }
    
    private void ReadKeyI()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            newSpawn = Instantiate(spawnObject);
            newSpawn.GetComponent<NetworkObject>().Spawn(true);
        }
    }
    
    private void ReadKeyK()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Destroy(newSpawn);
        }
    }

    [ServerRpc]
    private void TestServerRpc(string message)
    {
        Debug.Log("Test Server RPC" + OwnerClientId + " message: " + message);
    }
    
    [ClientRpc]
    private void TestClientRpc(string message)
    {
        Debug.Log("Test Client RPC" + OwnerClientId + " message: " + message);
    }
    
}