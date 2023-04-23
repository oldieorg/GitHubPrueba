using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using System.Text;
using Unity.Networking.Transport;
using NetworkMessages;
using UnityEngine.UI;
using TMPro;

public class Server : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public ushort serverPort;
    static public bool juegoEmpezado = false;
    public NativeList<NetworkConnection> m_Connections;
    public GameObject[] jugadoresSimulados;
    public List<NetworkObject.NetworkPlayer> jugadores;
    public float velocidadMira;
    public float timer;
    public bool jugadoresConectados = false;

    public GameObject Tiempo;

    public float timeRemaining = 10;

    public GameObject miPoolEnemigo;
    //public bool timerIsRunning = false;
    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = serverPort;
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind to port" + serverPort);
        }
        else 
        {
            m_Driver.Listen();
        }
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        
    }

    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        //Limpiar las conexiones
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        //Acepta nuevas conexiones
        NetworkConnection c = m_Driver.Accept();
        while (c != default(NetworkConnection))
        {
            OnConnect(c);
            c = m_Driver.Accept();
        }

        //leer mensajes entrantes
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            Assert.IsTrue(m_Connections[i].IsCreated);
            NetworkEvent.Type cmd;
            cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            while(cmd != NetworkEvent.Type.Empty) 
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    OnData(stream, i);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
                cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            }
        }
        if (jugadoresConectados == true)
        {
            /*do
            {
                timer -= Time.deltaTime;
            } while (timer != 0);
            if (timer <= 0)
            {
                jugadoresSimulados[0].SetActive(true);
                jugadoresSimulados[1].SetActive(true);
            }
            if(timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (timer <= 0)
            {
                jugadoresSimulados[0].SetActive(true);
                jugadoresSimulados[1].SetActive(true);
            }*/
            if (timeRemaining > 0) 
            {
                timeRemaining -= Time.deltaTime;
                float seconds = Mathf.FloorToInt(timeRemaining % 60);
                Tiempo.GetComponent<Text>().text = seconds + "";
            }
            else if (timeRemaining < 0)
            {
                Server.juegoEmpezado = false;
                timeRemaining = 0;
                //timeRemaining -= Time.deltaTime;
                //float seconds = Mathf.FloorToInt(timeRemaining % 60);
                //Tiempo.GetComponent<Text>().text = seconds + "";
                //jugadoresConectados = false;
            }
        }
    }
    
    void OnConnect(NetworkConnection c) 
    {
        m_Connections.Add(c);
        Debug.Log("Accepted a connection");
        Debug.Log("Numero de conexiones: " + m_Connections.Length);

        HandShakeMsg m = new HandShakeMsg();
        m.player.id = c.InternalId.ToString();
        SendToClient(JsonUtility.ToJson(m), c);
    }

    private void FixedUpdate()
    {
        if(jugadores.Count == 2) 
        {
            EnviarPosNira();
        }
    }

    private void SendToClient(string message, NetworkConnection c)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(NetworkPipeline.Null, c, out writer);
        NativeArray<byte> bytes = new
            NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }
    
    private void OnData(DataStreamReader stream, int numJugador) 
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        NetworkHeader header = JsonUtility.FromJson<NetworkHeader>(recMsg);
        
        switch (header.command)
        {
            case Commands.HANDSHAKE:
                HandShakeMsg handShakeRecibido = JsonUtility.FromJson<HandShakeMsg>(recMsg);
                Debug.Log("Se ha conectado: " + handShakeRecibido.player.nombre);
                NetworkObject.NetworkPlayer nuevoJugador = new NetworkObject.NetworkPlayer();
                nuevoJugador.id = handShakeRecibido.player.id;
                nuevoJugador.nombre = handShakeRecibido.player.nombre;
                jugadores.Add(nuevoJugador);
                int numJugadores = jugadores.Count;
                if (numJugadores == 2)
                {
                    Debug.Log("2 jugadores conectados");
                    jugadoresConectados = true;
                    juegoEmpezado = true;
                    ReadyMsg readyMsg = new ReadyMsg();
                    readyMsg.playerList = jugadores;
                    for (int j = 0; j < numJugadores; j++)
                    {
                        //jugadoresSimulados[j].GetComponentInChildren<TextMeshProUGUI>().text = jugadores[j].nombre;
                        SendToClient(JsonUtility.ToJson(readyMsg), m_Connections[j]);
                    }
                }
                break;
            case Commands.PLAYERINPUT:
                PlayerInputMsg playerInputRecibido = JsonUtility.FromJson<PlayerInputMsg>(recMsg);
                int tamArray = jugadores.Count;
                for(int j = 0; j < tamArray; j++)
                {
                    SendToClient(JsonUtility.ToJson(playerInputRecibido), m_Connections[j]);
                }
                //mover miras
                if(playerInputRecibido.myInput == "ARRIBA_IZQUIERDA")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                }
                else if(playerInputRecibido.myInput == "ARRIBA_DERECHA")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                }
                else if(playerInputRecibido.myInput == "ABAJO_IZQUIERDA")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                }
                else if(playerInputRecibido.myInput == "ABAJO_DERECHA")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                }
                else
                {
                    if (playerInputRecibido.myInput == "ARRIBA")
                    {
                        int indiceJugador = -1;
                        int.TryParse(playerInputRecibido.id, out indiceJugador);
                        jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                    }
                    else if(playerInputRecibido.myInput == "ABAJO")
                    {
                        int indiceJugador = -1;
                        int.TryParse(playerInputRecibido.id, out indiceJugador);
                        jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                    }
                    if (playerInputRecibido.myInput == "IZQUIERDA")
                    {
                        int indiceJugador = -1;
                        int.TryParse(playerInputRecibido.id, out indiceJugador);
                        jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                    }
                    else if (playerInputRecibido.myInput == "DERECHA")
                    {
                        int indiceJugador = -1;
                        int.TryParse(playerInputRecibido.id, out indiceJugador);
                        jugadoresSimulados[indiceJugador].GetComponent<MoverMira>().MoverNira(playerInputRecibido.myInput);
                    }
                }
                /*
                if (playerInputRecibido.myInput == "ARRIBA")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].transform.Translate(Vector3.up * velocidadMira * Time.deltaTime);
                    int cantidadJugadores = jugadores.Count;
                    MoverMiraMsg moverMiraMsg = new MoverMiraMsg();
                    moverMiraMsg.jugador.id = playerInputRecibido.id;
                    moverMiraMsg.jugador.posJugador = jugadoresSimulados[indiceJugador].transform.position;
                    for (int j = 0; j < cantidadJugadores; j++)
                    {
                        SendToClient(JsonUtility.ToJson(moverMiraMsg), m_Connections[j]);
                    }
                }
                if (playerInputRecibido.myInput == "ABAJO")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].transform.Translate(Vector3.down * velocidadMira * Time.deltaTime);
                    int cantidadJugadores = jugadores.Count;
                    MoverMiraMsg moverMiraMsg = new MoverMiraMsg();
                    moverMiraMsg.jugador.id = playerInputRecibido.id;
                    moverMiraMsg.jugador.posJugador = jugadoresSimulados[indiceJugador].transform.position;
                    for (int j = 0; j < cantidadJugadores; j++)
                    {
                        SendToClient(JsonUtility.ToJson(moverMiraMsg), m_Connections[j]);
                    }
                }
                if (playerInputRecibido.myInput == "IZQUIERDA")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].transform.Translate(Vector3.left * velocidadMira * Time.deltaTime);
                    int cantidadJugadores = jugadores.Count;
                    MoverMiraMsg moverMiraMsg = new MoverMiraMsg();
                    moverMiraMsg.jugador.id = playerInputRecibido.id;
                    moverMiraMsg.jugador.posJugador = jugadoresSimulados[indiceJugador].transform.position;
                    for (int j = 0; j < cantidadJugadores; j++)
                    {
                        SendToClient(JsonUtility.ToJson(moverMiraMsg), m_Connections[j]);
                    }
                }
                if (playerInputRecibido.myInput == "DERECHA")
                {
                    int indiceJugador = -1;
                    int.TryParse(playerInputRecibido.id, out indiceJugador);
                    jugadoresSimulados[indiceJugador].transform.Translate(Vector3.right * velocidadMira * Time.deltaTime);
                    int cantidadJugadores = jugadores.Count;
                    MoverMiraMsg moverMiraMsg = new MoverMiraMsg();
                    moverMiraMsg.jugador.id = playerInputRecibido.id;
                    moverMiraMsg.jugador.posJugador = jugadoresSimulados[indiceJugador].transform.position;
                    for (int j = 0; j < cantidadJugadores; j++)
                    {
                        SendToClient(JsonUtility.ToJson(moverMiraMsg), m_Connections[j]);
                    }
                }*/
                if(playerInputRecibido.myInput == "DISPARO")
                {
                    Debug.Log("Disparo");
                }

                break;
            case Commands.TIMER:
                TimerMsg timerMsg = JsonUtility.FromJson<TimerMsg>(recMsg);
                float seconds = Mathf.FloorToInt(timeRemaining % 60);
                timerMsg.tiempo = seconds;
                int cantidad = jugadores.Count;
                for (int i = 0; i < cantidad; i++)
                {
                    SendToClient(JsonUtility.ToJson(timerMsg), m_Connections[i]);
                }
                break;
            case Commands.FINAL:
                FinalMsg finalMsg = JsonUtility.FromJson<FinalMsg>(recMsg);
                finalMsg.final = true;
                int finaltiempo = jugadores.Count;
                for (int i = 0; i < finaltiempo; i++)
                {
                    SendToClient(JsonUtility.ToJson(finalMsg), m_Connections[i]);
                }

                break;

            default:
                Debug.Log("Comando no reconocido");
                break;
        }
    }

    public void OnDestroy()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }
    
    public void EnviarPosNira() 
    {
        int cantidadJugadores = jugadores.Count;

        for(int j = 0; j < cantidadJugadores; j++) 
        {
            MoverMiraMsg moverMiraMsg = new MoverMiraMsg();
            moverMiraMsg.jugador.id = j + "";
            moverMiraMsg.jugador.posJugador = jugadoresSimulados[j].transform.position;
            SendToClient(JsonUtility.ToJson(moverMiraMsg), m_Connections[0]);
            SendToClient(JsonUtility.ToJson(moverMiraMsg), m_Connections[1]);
        }
    }

}
