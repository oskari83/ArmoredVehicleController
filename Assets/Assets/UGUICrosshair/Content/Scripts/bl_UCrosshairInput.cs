using UnityEngine;
using System.Collections;

public class bl_UCrosshairInput : MonoBehaviour
{

    private int currentCross;

    private void Update()
    {
        TestInput();
    }


    void TestInput()
    {
        if (Input.GetMouseButton(0)) {bl_UCrosshair.Instance.OnFire(); }
        bool aim = Input.GetMouseButton(1);
        bl_UCrosshair.Instance.OnAim(aim);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentCross = (currentCross + 1) % 25;
            bl_UCrosshair.Instance.Change(currentCross);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            bl_UCrosshair.Instance.SetColor(Color.red);
        }
        else if (Input.GetKeyUp(KeyCode.F)) { bl_UCrosshair.Instance.SetDefaultColors(); }
        if (Input.GetButtonDown("Fire3"))
        {
            bl_UCrosshair.Instance.OnHit();
        }
    }

    public void Rotate(bool rot)
    {
        bl_UCrosshair.Instance.Reset();
        bl_UCrosshair.Instance.RotateCrosshair = rot;
    }

    public void Follow(bool foll)
    {
        bl_UCrosshair.Instance.Reset();
        Cursor.visible = !foll;
        bl_UCrosshair.Instance.FollowMouse = foll;
    }
}