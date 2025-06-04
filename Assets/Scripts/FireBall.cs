using System.Collections;
using UnityEngine;
using Mirror;

public class FireBall : NetworkBehaviour
{
    public float speed = 15f;
    private Rigidbody rb;

    private DragonController CurrentDragon;

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody>();

        StartCoroutine(DestroySelfAfterDelay(10f));
    }

    [Server]
    public void Launch(Vector3 position, Quaternion rotation, DragonController GG)
    {
        transform.position = position;
        transform.rotation = rotation;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }

        CurrentDragon = GG;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(CurrentDragon != null && CurrentDragon.name != other.name)
            {
                if(other.GetComponent<DragonController>().TakeDamage()) CurrentDragon.addScore(5);
                CurrentDragon.addScore();
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator DestroySelfAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkServer.Destroy(gameObject);
    }
}
