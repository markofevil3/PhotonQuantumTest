using UnityEngine;

namespace Quantum.Demo {
  public class UIMain : MonoBehaviour {
    public static QuantumLoadBalancingClient Client { get; set; }
    public static float FlipLayoutFactor = 1.34f;

    public GameObject LogoVertical;
    public GameObject LogoHorizontal;

    public enum PhotonEventCode : byte {
      GameStartCountDown = 110,
      StartGame = 111,
    }

    private void Update() {
      Client?.Service();

      LogoVertical.Toggle(Screen.width < Screen.height * FlipLayoutFactor);
      LogoHorizontal.Toggle(Screen.width >= Screen.height * FlipLayoutFactor);
    }
  }
}