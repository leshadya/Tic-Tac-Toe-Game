using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    IPAddress localAddr = IPAddress.Parse("192.168.1.202");
    public int port = 2222;
    public Button[] cellButtons;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private string mySymbol = "";
    [SerializeField] private UILineDrawer lineDrawer;
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private AudioSource mark;
    [SerializeField] private AudioSource win;


    void Start()
    {
        ConnectToServer();
        SetupButtons();
      
    }


    void ConnectToServer()
    {
        client = new TcpClient();
        client.Connect(localAddr, port);
        stream = client.GetStream();

        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();

    }

    void SetupButtons()
    {
        for (int i = 0; i < cellButtons.Length; i++) {

            int index = i;
            cellButtons[i].onClick.AddListener(() => OnCellClicked(index));
        }
    }

    void OnCellClicked(int index)
    {
        var text = cellButtons[index].GetComponentInChildren<TMP_Text>();
        if (text.text != "") return;

        byte[] data = Encoding.UTF8.GetBytes(index.ToString());
        stream.Write(data, 0, data.Length);

    }

    void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        while (true) { 

            int bytesCount = stream.Read(buffer, 0, buffer.Length);
            if (bytesCount == 0) break;

            string msg = Encoding.UTF8.GetString(buffer, 0, bytesCount);
            Debug.Log("Gelen mesaj: " + msg);

            if (msg == "YOUR_TURN")
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    turnText.text = "Your turn!";
                });
            }
            else if (msg == "WAIT")
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    turnText.text = "wait bro";
                });
            }
            if (msg == "YOU WIN")
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    lineDrawer.PlayConfetti();
                });
            }
            if (msg.StartsWith("WIN"))
            {
                string[] parts = msg.Split('|');
                string winner = parts[1];
                int index1 = int.Parse(parts[2]);
                int index2 = int.Parse(parts[3]);
                if (parts[4] == "YOU") {
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        lineDrawer.PlayConfetti();
                        win.Play();
                    });
                }
              
                ShowMessage(winner + " WINS!");
                DrawWinLine(index1, index2);

            }
            else if (msg.Contains('|'))
            {
                string[] parts = msg.Split('|');
                if (parts.Length == 2 && int.TryParse(parts[0], out int cell))
                {
                    string symbol = parts[1];
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        mark.Play();
                        UpdateCell(cell, symbol);
                    });
                    
                }
            }
            else if( msg == "DRAW")
            {
                ShowMessage("DRAW!");
            }
            else if (msg == "Reset")
            {
              ResetGame();
            }
            
        }
    }
    void DrawWinLine(int index1, int index2)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            RectTransform point1 = cellButtons[index1].GetComponentInChildren<RectTransform>();
            RectTransform point2 = cellButtons[index2].GetComponentInChildren<RectTransform>();
           
            lineDrawer.DrawLine(point1, point2);
            //lineDrawer.PlayConfetti();
            //Debug.Log("isWinner: "+ isWinner + "::: mySymbol: "+ mySymbol);
            //if (isWinner) { lineDrawer.PlayConfetti(); }
          
       });
    }
    void UpdateCell(int index, string symbol)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() => 
        {
            
            cellButtons[index].GetComponentInChildren<TMP_Text>().text = symbol;
            if (mySymbol == "")
                mySymbol = symbol == "X" ? "O" : "x";
          

        });
    }
    void ResetGame()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            ClearBoard();
            lineDrawer.ClearLine();
            lineDrawer.StopConfetti();
            winnerPanel.SetActive(false);
        });

    }
    void ShowMessage(string msg)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            turnText.text = "";
            winnerPanel.SetActive(true);
            winnerText.text = msg;
        });
    }
    
    public void SendRestartSignal()
    {
        byte[] data = Encoding.UTF8.GetBytes("R");
        stream.Write(data, 0, data.Length);
       
       
    }
    void ClearBoard()
    {
        foreach(var cell in cellButtons)
        {
            cell.GetComponentInChildren<TMP_Text>().text = "";
        }

    }
    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}