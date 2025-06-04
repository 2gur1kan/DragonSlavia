using UnityEngine;

public class Area : MonoBehaviour
{
    public float radius = 150f; // Küresel alan yarýçapý (sphere radius)

    public Vector3 GetWrappedPosition(Vector3 pos)
    {
        Vector3 center = transform.position;
        Vector3 dir = pos - center; // Merkezden uzaklýk vektörü (direction vector)

        if (dir.magnitude > radius)
        {
            // Kürenin dýþýna çýktýysa, ters yönden içeri al
            dir = dir.normalized * -radius;
            return center + dir;
        }

        return pos; // Ýçerideyse aynen geri döndür
    }

    // Küre içinde rastgele pozisyon döner
    public Vector3 GetRandomPositionInside()
    {
        // Unit sphere içinde rastgele nokta al
        Vector3 randomDirection = Random.insideUnitSphere;

        // Yarýçapýn yarýsý kadar mesafeye ölçekle
        float spawnRadius = radius * 0.5f;

        // Pozisyonu merkeze göre kaydýr
        Vector3 spawnPosition = transform.position + randomDirection * spawnRadius;

        return spawnPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
