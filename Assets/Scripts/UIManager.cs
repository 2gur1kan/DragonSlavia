using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private string hostIp = "192.168.1.100";

    [SerializeField] private GameObject JoinPanel;
    [SerializeField] private GameObject HitOrDead;

    [SerializeField] private TMP_InputField Input;
    [SerializeField] private TextMeshProUGUI Name;

    public string getName => Name.text;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        JoinPanel.SetActive(true);
    }

    private void Update()
    {
        if (NetworkClient.isConnected) Destroy(JoinPanel);
    }

    public void OnJoinOrHostClicked()
    {
        if (!NetworkClient.isConnected)
        {
            Name.text = Input.text;

            NetworkManager.singleton.networkAddress = hostIp;
            NetworkManager.singleton.StartClient();
            Debug.Log("Client bağlanmayı deniyor...");

            StartCoroutine(TryConnectThenHost());
        }
    }

    private IEnumerator TryConnectThenHost()
    {
        float timeout = 1f;
        float timer = 0f;

        while (!NetworkClient.isConnected && timer < timeout)
        {
            timer += .5f;
            yield return new WaitForSeconds(.5f);
        }

        if (!NetworkClient.isConnected)
        {
            NetworkManager.singleton.networkAddress = hostIp; // hostIp = 192.168.1.100 

            Debug.LogWarning("Client bağlantısı başarısız. Host başlatılıyor...");
            NetworkManager.singleton.StopClient();

            // Host modda IP ayarı gerekli değil
            NetworkManager.singleton.StartHost();

            hostIp = GetLocalIPAddress();
            Debug.Log("Host IP: " + hostIp);
        }

        Destroy(JoinPanel);
    }

    public void HitOrDeadPanel(bool isDead = false)
    {
        if (isDead)
        {
            HitOrDead.GetComponent<Image>().color = Color.black;

            Invoke("closeHitOrDeadPanel", 5f);
        }
        else
        {
            HitOrDead.GetComponent<Image>().color = Color.red;

            Invoke("closeHitOrDeadPanel", .2f);
        }

        HitOrDead.SetActive(true);
    }

    private void closeHitOrDeadPanel() => HitOrDead.SetActive(false);

    private string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        string fallbackIP = "127.0.0.1"; // İngilizce: fallback (geri dönüş) IP

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                string ipStr = ip.ToString();
                if (ipStr.StartsWith("192.168"))
                {
                    return ipStr; // Öncelikle Wi-Fi / Hotspot IP'yi döndür
                }
                else
                {
                    fallbackIP = ipStr; // Alternatif olarak 10.x.x.x veya diğer IPv4
                }
            }
        }

        return fallbackIP; // Eğer 192.168 bulunamazsa başka IPv4 döner
    }


    public void FoundHost(string ip)
    {
        Name.text = Input.text;
        hostIp = ip;
        Debug.Log("Otomatik bulunan host IP: " + hostIp);

        // Otomatik bağlan
        if (!NetworkClient.isConnected)
        {
            NetworkManager.singleton.networkAddress = hostIp;
            NetworkManager.singleton.StartClient();
            Debug.Log("Client otomatik bağlanmayı deniyor...");
        }
    }

    private void OnDestroy()
    {
        Name.text = Input.text;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
