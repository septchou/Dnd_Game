using Photon.Pun;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameSceneController : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // Each player has a unique ActorNumber in Photon

            if (CharacterManager.Instance != null)
            {
                Transform spawnPoint = GetSpawnPoint(playerIndex);
                CharacterManager.Instance.SpawnPlayerCharacter(spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.LogError("CharacterManager instance not found!");
            }
        }
    }

    public Transform GetSpawnPoint(int index)
    {
        if (index >= 0 && index < spawnPoints.Length)
        {
            return spawnPoints[index];
        }
        else
        {
            Debug.LogWarning("Spawn point index out of range, returning default spawn point.");
            return spawnPoints[0]; // Default to the first spawn point
        }
    }
}
