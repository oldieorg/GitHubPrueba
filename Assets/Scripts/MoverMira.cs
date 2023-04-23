using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverMira : MonoBehaviour
{
    private string Mover = "";
    public GameObject server;
    public float velocidadMira;
    public int index;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Mover == "DERECHA")
        {
            transform.Translate(Vector3.right * velocidadMira * Time.deltaTime);
        }
        else if (Mover == "IZQUIERDA")
        {
            transform.Translate(Vector3.left * velocidadMira * Time.deltaTime);
        }
        
        if(Mover == "ARRIBA")
        {
            transform.Translate(Vector3.up * velocidadMira * Time.deltaTime);
        }
        else if (Mover == "ABAJO")
        {
            transform.Translate(Vector3.down * velocidadMira * Time.deltaTime);
        }
        else if (Mover == "ARRIBA_IZQUIERDA")
        {
            transform.Translate((Vector3.up + Vector3.left / 2) * velocidadMira * Time.deltaTime);
        }
        else if ( Mover == "ARRIBA_DERECHA")
        {
            transform.Translate((Vector3.up + Vector3.right / 2) * velocidadMira * Time.deltaTime);
        }
        else if ( Mover == "ABAJO_IZQUIERDA")
        {
            transform.Translate((Vector3.down + Vector3.left / 2) * velocidadMira * Time.deltaTime);
        }
        else if (Mover == "ABAJO_DERECHA")
        {
            transform.Translate((Vector3.down + Vector3.right / 2) * velocidadMira * Time.deltaTime);
        }
        Mover = "";
    }

    public void MoverNira(string Mov)
    {
        Mover = Mov;
    }
}
