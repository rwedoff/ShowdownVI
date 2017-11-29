using UnityEngine;
using UnityEngine.Networking;

public class BallSpawner : NetworkBehaviour
{

    public GameObject ballPrefab;
    public int numberOfEnemies;

    public override void OnStartServer()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            var spawnPosition = new Vector3(20, 3, -75);

            var spawnRotation = Quaternion.Euler(
                0.0f,
                Random.Range(0, 180),
                0.0f);

            var ball = Instantiate(ballPrefab, spawnPosition, spawnRotation);
            NetworkServer.Spawn(ball);
        }
    }
}