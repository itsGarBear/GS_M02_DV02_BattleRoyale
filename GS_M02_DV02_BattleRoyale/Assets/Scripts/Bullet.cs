using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerID;
    private bool isMine;

    public Rigidbody rig;

    public void Initialize(int dmg, int attackerID, bool isMine)
    {
        this.damage = dmg;
        this.attackerID = attackerID;
        this.isMine = isMine;

        Destroy(gameObject, 5.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);
            if (player.id != attackerID)
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerID, damage);
        }
        Destroy(gameObject);
    }
}
