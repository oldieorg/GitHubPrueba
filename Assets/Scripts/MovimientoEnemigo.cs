using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoEnemigo : MonoBehaviour
{
    private Transform miTransform;
    /*
    public int velocidad;
    public Vector3 _velocidad;
    */
    
    public Transform posicionInicial;
    public float timeRemaining = 10;
    public bool timerIsRunning = true;
    public GameObject server;
    //movimiento jonny
    public float velocidad;
    private float _velocidad;
    private float crono;
   

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        miTransform = transform;
        miTransform.position = GameObject.Find("posIzq").transform.position;
        _velocidad = velocidad;
        crono = 0;
        /*
        _velocidad.x = 1;
        _velocidad.y = 1;
        velocidad = 1;
        //posicion del pullobject
        //posicionInicial = GameObject.find("EnemigosPullObject").transform;
        */

    }

    // Update is called once per frame
    void Update()
    {
        crono += Time.deltaTime; 
        
        if(crono >= 5)
        {
           cambiar();
            crono = 0;
            _velocidad *= -1;
        }
        /*
        miTransform.Translate(_velocidad * velocidad * Time.deltaTime);
        int X = 0;
        int.TryParse(_velocidad.x + "", out X);
        int Y = 0;
        int.TryParse(_velocidad.y + "", out Y);
        */
        miTransform.Translate(Vector3.right * _velocidad * Time.deltaTime);
    }
    
    public void reiniciar()
    {
        this.gameObject.SetActive(false);
        miTransform.position = posicionInicial.position;
        //GameObject.Find("EnemigosPullObject").GetComponent<pullobject>().AnadirEnemigo(this.gameObject);
    }
    public void cambiar()
    {
        miTransform.localScale = new Vector3(miTransform.localScale.x * -1, 1, 1);
    }
}
