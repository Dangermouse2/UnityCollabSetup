using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float lifetime = 0.6f;
    void Start() => Destroy(gameObject, lifetime);
}