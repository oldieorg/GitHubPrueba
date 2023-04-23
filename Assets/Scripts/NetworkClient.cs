using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections; 
using System.Text;
using Unity.Networking.Transport;
using UnityEngine.UI;
using NetworkMessages;
using TMPro;

public class NetworkClient : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIp;
    public ushort serverPort;
    //public TMP_InputField inputNombre;
    public TMP_InputField inputNombre;
    private bool empezar = false;
    public string idPlayer;
    //diferentes paneles que van ir variando con el inicio y fin de partida
    public GameObject panelJuego, panelFinal;
    public GameObject pantallaInicio;
    public GameObject jugador1, jugador2;

    //tiempo del servidor
    public GameObject timeRemaining;

    //private bool inicio = false;

    public GameObject[] jugadoresGameObject;
    
    // Start is called before the first frame update
     public void Conectar()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.Parse(serverIp, serverPort);
        m_Connection = m_Driver.Connect(endpoint);

        inputNombre.gameObject.SetActive(false);
        GameObject.Find("Button").SetActive(false);
        //GameObject.Find("Text").GetComponent<Text>().text = "Esperando";
        empezar = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (empezar)
        {
            m_Driver.ScheduleUpdate().Complete();
            if (!m_Connection.IsCreated)
            {
                Debug.Log("Something went wrong during connect");
                return;
            }
            DataStreamReader stream;
            NetworkEvent.Type cmd = m_Connection.PopEvent(m_Driver, out stream);
            while (cmd != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    OnConnect();
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    OnData(stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    //al desconectarse
                    //OnDisconnect();
                    m_Connection = default(NetworkConnection);
                }
                cmd = m_Connection.PopEvent(m_Driver, out stream);
            }
            if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.UpArrow))
            {
                //Arriba izquierda
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "ARRIBA_IZQUIERDA";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }
            else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.UpArrow))
            {
                //Arriba derecha
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "ARRIBA_DERECHA";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.DownArrow))
            {
                //abajo izquierda
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "ABAJO_IZQUIERDA";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            } 
            else if(Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.DownArrow))
            {
                //abajo derecha
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "ABAJO_DERECHA";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }
            else
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                    playerInputMsg.id = idPlayer;
                    playerInputMsg.myInput = "ARRIBA";
                    SendToServer(JsonUtility.ToJson(playerInputMsg));
                }
               else if (Input.GetKey(KeyCode.DownArrow))
                {
                    PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                    playerInputMsg.id = idPlayer;
                    playerInputMsg.myInput = "ABAJO";
                    SendToServer(JsonUtility.ToJson(playerInputMsg));
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                    playerInputMsg.id = idPlayer;
                    playerInputMsg.myInput = "IZQUIERDA";
                    SendToServer(JsonUtility.ToJson(playerInputMsg));
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                    playerInputMsg.id = idPlayer;
                    playerInputMsg.myInput = "DERECHA";
                    SendToServer(JsonUtility.ToJson(playerInputMsg));
                }
            }
            /*
            if(Input.GetKey(KeyCode.UpArrow))
            {
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "ARRIBA";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "ABAJO";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "IZQUIERDA";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "DERECHA";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }*/

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayerInputMsg playerInputMsg = new PlayerInputMsg();
                playerInputMsg.id = idPlayer;
                playerInputMsg.myInput = "DISPARO";
                SendToServer(JsonUtility.ToJson(playerInputMsg));
            }
            else if (timeRemaining.GetComponent<Text>().text == "0")
            {
                FinalMsg final = new FinalMsg();
                final.final = true;
                SendToServer(JsonUtility.ToJson(final));
                //Server.juegoEmpezado = false;
            }
            else if (empezar == true)
            {
                TimerMsg timerMsg = new TimerMsg();
                SendToServer(JsonUtility.ToJson(timerMsg));
            }
        }
    }
    
    private void OnData(DataStreamReader stream) 
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        NetworkHeader header = JsonUtility.FromJson<NetworkHeader>(recMsg);

        switch (header.command) 
        {
            case Commands.HANDSHAKE:
                HandShakeMsg handShakeRecibido =
                    JsonUtility.FromJson<HandShakeMsg>(recMsg);
                HandShakeMsg handShakeEnviar = new HandShakeMsg();
                idPlayer = handShakeRecibido.player.id;
                handShakeEnviar.player.nombre = inputNombre.text;
                SendToServer(JsonUtility.ToJson(handShakeEnviar));
                break;
            case Commands.READY:
                ReadyMsg readyMsg = JsonUtility.FromJson<ReadyMsg>(recMsg);
                pantallaInicio.SetActive(false);
                int numPlayers = readyMsg.playerList.Count;
                panelJuego.SetActive(true);
                /*for (int i = 0; i < numPlayers; i++)
                {
                    jugadoresGameObject[i].GetComponentInChildren<TextMeshProUGUI>().text = readyMsg.playerList[i].nombre;
                }*/
                break;
            case Commands.PLAYERINPUT:
                PlayerInputMsg playerInputMsg = JsonUtility.FromJson<PlayerInputMsg>(recMsg);
                break;
            case Commands.MOVER_MIRA:
                MoverMiraMsg moverMiraMsg = JsonUtility.FromJson<MoverMiraMsg>(recMsg);
                int idJugador;

                int.TryParse(moverMiraMsg.jugador.id, out idJugador);
                jugadoresGameObject[idJugador].transform.position = moverMiraMsg.jugador.posJugador;
                break;
            case Commands.DISPARO:
                DisparoMsg disparoMsg = JsonUtility.FromJson<DisparoMsg>(recMsg);
                break;

            case Commands.TIMER:
                TimerMsg timerMsg = JsonUtility.FromJson<TimerMsg>(recMsg);
                int calculartiempo = 0;

                int.TryParse(timerMsg.tiempo+"", out calculartiempo);
                timeRemaining.GetComponent<Text>().text = timerMsg.tiempo + "";

                break;

            case Commands.FINAL:
                ReadyMsg readyMsgfinal = JsonUtility.FromJson<ReadyMsg>(recMsg);
                FinalMsg finalMsg = JsonUtility.FromJson<FinalMsg>(recMsg);
                panelJuego.SetActive(false);
                panelFinal.SetActive(true);
                break;
            default:
                Debug.Log("Mensaje desconocido");
                break;
        }
    }

    private void SendToServer(string message) 
    {
        //var writer = m_Driver.BeginSend(m_Connection);
        DataStreamWriter writer;
        m_Driver.BeginSend(NetworkPipeline.Null, m_Connection, out writer);
        NativeArray<byte> bytes = new
            NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    private void OnConnect()
    {
        Debug.Log("Conectado correctamente");
    }

}
