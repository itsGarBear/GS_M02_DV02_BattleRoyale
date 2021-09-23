using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;

    private int currAttackerID;

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int currHP;
    public int maxHP;
    public int kills;
    public bool dead;
    private bool flashingDMG;
    public MeshRenderer mr;

    [Header("Components")]
    public Rigidbody rb;
    public Player photonPlayer;
    public PlayerWeapon weapon;

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rb.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine || dead)
            return;


        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }

    [PunRPC]
    public void TakeDamage(int attackerID, int damage)
    {
        if (dead)
            return;

        currHP -= damage;
        currAttackerID = attackerID;

        photonView.RPC("DamageFlash", RpcTarget.Others);

        GameUI.instance.UpdateHealthBar();

        if (currHP <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDMG)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDMG = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDMG = false;
        }
    }

    [PunRPC]
    void Die()
    {
        currHP = 0;
        dead = true;
        GameManager.instance.alivePlayers--;

        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        if (photonView.IsMine)
        {
            if (currAttackerID != 0)
                GameManager.instance.GetPlayer(currAttackerID).photonView.RPC("AddKill", RpcTarget.All);

            GetComponentInChildren<CameraController>().SetAsSpectator();

            rb.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);

        }
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;
        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {

        currHP = Mathf.Clamp(currHP + amountToHeal, 0, maxHP);
        GameUI.instance.UpdateHealthBar();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rb.velocity.y;

        rb.velocity = dir;
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 1.5f))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

}
