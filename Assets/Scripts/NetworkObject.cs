using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkObject
{
    [System.Serializable]
    public class NetworkObject
    {
        public string id;
    }

    [System.Serializable]
    public class NetworkPlayer : NetworkObject
    {
        public Vector3 posJugador;
        public string nombre;
    }

}
namespace NetworkMessages
{
    public enum Commands 
    {
        HANDSHAKE,
        READY,
        PLAYERINPUT,
        MOVER_MIRA,
        DISPARO,
        SPAWN,
        TIMER,
        FINAL,
    }

    [System.Serializable]
    public class NetworkHeader 
    {
        public Commands command;
    }

    [System.Serializable]
    public class HandShakeMsg : NetworkHeader 
    {
        public NetworkObject.NetworkPlayer player;
        public HandShakeMsg()
        {
            command = Commands.HANDSHAKE;
            player = new NetworkObject.NetworkPlayer();
        }
    }

    [System.Serializable]
    public class ReadyMsg : NetworkHeader
    {
        public List<NetworkObject.NetworkPlayer> playerList;
        public ReadyMsg()
        {
            command = Commands.READY;
            playerList = new List<NetworkObject.NetworkPlayer>();
        }
    }

    [System.Serializable]
    public class PlayerInputMsg : NetworkHeader
    {
        public string id;
        public string myInput;

        public PlayerInputMsg()
        {
            command = Commands.PLAYERINPUT;
            myInput = "";
            id = "";
        }
    }

    [System.Serializable]
    public class MoverMiraMsg : NetworkHeader
    {
        public NetworkObject.NetworkPlayer jugador;
        public MoverMiraMsg()
        {
            command = Commands.MOVER_MIRA;
            jugador = new NetworkObject.NetworkPlayer();
        }
    }

    [System.Serializable]
    public class DisparoMsg : NetworkHeader
    {
        public NetworkObject.NetworkPlayer jugador;
        public DisparoMsg()
        {
            command = Commands.DISPARO;
            jugador = new NetworkObject.NetworkPlayer();
        }
    }

    [System.Serializable]
    public class SpawnMsg : NetworkHeader
    {
        public GameObject jugador1;
        public GameObject jugador2;
        
        public SpawnMsg()
        {
            command = Commands.SPAWN;
            jugador1 = new GameObject();
            jugador2 = new GameObject();
        }
    }
    
    //controlar el timer final
    
    [System.Serializable]
    public class TimerMsg : NetworkHeader
    {
        public float tiempo;
        public TimerMsg()
        {
            command = Commands.TIMER;
            tiempo = 0;
        }

    }

    // para el final del juego
    [System.Serializable]
    public class FinalMsg : NetworkHeader
    {
        public bool final;
        public FinalMsg()
        {
            command = Commands.FINAL;
            final = false;
        }

    }


}
    



