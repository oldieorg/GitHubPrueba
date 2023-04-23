using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pruebaPullObject : MonoBehaviour
{
    public GameObject enemigo;
    private List<GameObject> listaEnemigos;
    public int cantidad;
    public Transform posEnemigo;
    // Start is called before the first frame update
    void Start()
    {
        listaEnemigos = new List<GameObject>();
        for (int i = 0; i < cantidad; i++)
        {
            listaEnemigos.Add(Instantiate(enemigo));
        }
        InvokeRepeating("CrearEnemigo", 10, 5);
    }

    // Update is called once per frame
   public void CrearEnemigos()
    {
        GameObject enemigosColocar = listaEnemigos[0];

        listaEnemigos.RemoveAt(0);
        enemigosColocar.transform.position = posEnemigo.position;
        enemigosColocar.SetActive(true);
        AnadirEnemigo(enemigo);
    }

    public void AnadirEnemigo(GameObject enemigo)
    {
        listaEnemigos.Add(enemigo);
    }
}
