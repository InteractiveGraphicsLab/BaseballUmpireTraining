using UnityEngine;
using UnityEngine.UI;

public class BallInfoPreview : MonoBehaviour {
    [SerializeField] Text ballType;
    [SerializeField] Text ballSpeed;
    [SerializeField] Text ballSpinRate;
    [SerializeField] Image pitcherImage;
    [SerializeField] Text pitcherName;
    [SerializeField] Text pitcherTeam;
    [SerializeField] BallRotationPreview rotPreview;

    public void SetParameter(string btype, string bvelo, string pname, string pteam, Sprite pimg, Vector2 spinAxis, float bspinrate) {
        ballType.text = btype;
        ballSpeed.text = bvelo;
        ballSpinRate.text = (bspinrate * 60f).ToString("F2");

        pitcherImage.sprite = pimg;
        pitcherName.text = pname;
        pitcherTeam.text = pteam;

        rotPreview.Rerotate(spinAxis, bspinrate);
    }

    public void SetParameter(BallData.Type btype, float bvelo, string pname, string pteam, Sprite pimg, Vector2 spinAxis, float bspinrate) {
        SetParameter(btype.ToString(), bvelo.ToString("F2"), pname, pteam, pimg, spinAxis, bspinrate);
    }

    public void UpdateInfo(Pitcher p, Ball b) {
        SetParameter(b.balltype, b.velocity, p.name, "準備中", p.image, b.spinAxis, b.spinRate);
    }

    public void InitPreview() {
        SetParameter("-", "-", "-", "-", Pitcher.GetNoimage(), Vector2.zero, 0);
    }
}
