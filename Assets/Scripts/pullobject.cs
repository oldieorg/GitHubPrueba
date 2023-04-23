using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pullobject : MonoBehaviour
{

    public GameObject enemigo;
    public int cantidad;
    private List<GameObject> listaEnemigos;
    public Transform posEnemigo;
    private Vector3[] direcciones;
    private Transform miTransform;
    public GameObject server;
    private int id = 0;
    // Start is called before the first frame update
    void Start()
    {
        miTransform = this.transform;

        listaEnemigos = new List<GameObject>();
        for (int i = 0; i < cantidad; i++)
        {
            listaEnemigos.Add(Instantiate(enemigo));
        }
        InvokeRepeating("CrearEnemigo", 5, 5);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CrearEnemigo()
    {
        if (Server.juegoEmpezado == true)
        {
            Debug.Log("enemigo");
            GameObject enemigoElegido = listaEnemigos[0];
            listaEnemigos.RemoveAt(0);
            //server.GetComponent<Server>().EnviarPosEnemigos(enemigoElegido.transform.position, id + "");

            enemigoElegido.transform.position = posEnemigo.position;
            enemigoElegido.GetComponent<Rigidbody2D>().gravityScale = 0;

            enemigoElegido.SetActive(true);
            id++;
            AnadirEnemigo(enemigo);
        }
    }

    public void AnadirEnemigo(GameObject enemigo)
    {
        listaEnemigos.Add(enemigo);
    }
}
