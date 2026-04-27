using UnityEngine;

public class PoolGameSelector : MonoBehaviour
{
    [System.Serializable]
    public class PoolTableEntry
    {
        public string tableName;
        public GameObject tableObject;         // The table already in the scene
        public GameObject mechanicsPrefab;     // The mechanics prefab tied to this table
        public Transform mechanicsSpawnPoint;  // Where to spawn the mechanics
    }

    [Header("Pool Table Entries")]
    public PoolTableEntry[] poolTables;

    [Header("Selection")]
    public bool randomize = true;
    public int selectedIndex = 0;

    private GameObject spawnedMechanics;

    private void Start()
    {
        SelectAndSetupTable();
    }

    public void SelectAndSetupTable()
    {
        if (poolTables == null || poolTables.Length == 0)
        {
            Debug.LogError("No pool table entries assigned.");
            return;
        }

        int index = randomize ? Random.Range(0, poolTables.Length) : selectedIndex;

        if (index < 0 || index >= poolTables.Length)
        {
            Debug.LogError("Selected index is out of range.");
            return;
        }

        PoolTableEntry chosenTable = poolTables[index];

        if (chosenTable.mechanicsPrefab == null)
        {
            Debug.LogError($"No mechanics prefab assigned for table: {chosenTable.tableName}");
            return;
        }

        Transform spawnPoint = chosenTable.mechanicsSpawnPoint;

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : chosenTable.tableObject.transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : chosenTable.tableObject.transform.rotation;

        spawnedMechanics = Instantiate(chosenTable.mechanicsPrefab, spawnPosition, spawnRotation);

        Debug.Log("Chosen Table: " + chosenTable.tableName);
        Debug.Log("Spawned Mechanics: " + chosenTable.mechanicsPrefab.name);
    }
}