using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class Intro : MonoBehaviour
{
    [SerializeField]
    GameObject cameraPrefab;

    public CinemachineBrain brain;

    Vector3 mapCenter;

    GameObject cam1;
    GameObject cam2;
    GameObject cam3;

    GameObject canvas;

    [SerializeField]
    CinemachineSmoothPath purpleFlyThru;
    [SerializeField]
    CinemachineSmoothPath yellowFlyThru;

    CinemachineSmoothPath path1;
    [SerializeField]
    CinemachineSmoothPath path2;

    GameObject myBase;

    bool endIntro;

    public bool introDone;

    List<GameObject[]> transitions;

    Transform shipTransform;

    int myTeam;

    private void Start()
    {
        if (transform.root.GetChild(0).GetChild(0).GetComponent<PhotonView>().IsMine)
        {
            transitions = new List<GameObject[]>();
            canvas = GameObject.FindGameObjectWithTag("Canvas");
            canvas.SetActive(false);

            shipTransform = transform.root.GetChild(0).GetChild(0);

            mapCenter = GameObject.FindGameObjectWithTag("MapCenter").transform.position;

            myTeam = (int)shipTransform.GetComponent<PlayerController>().myTeam;

            GameObject[] bases = GameObject.FindGameObjectsWithTag("ResupplyBase");
            foreach (GameObject _base in bases)
            {
                if (myTeam == (int)_base.GetComponent<ReloadRegister>().myTeam)
                {
                    myBase = _base;
                }
            }

            if (myTeam == 0)
            {
                path1 = purpleFlyThru;
            }
            else
            {
                path1 = yellowFlyThru;
            }
            StartIntro();
        }
    }

    public void StartIntro()
    {
        Camera.main.cullingMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("TransparentFX")) |
                                  (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Water")) |
                                  (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("PostProcessing")) |
                                  (1 << LayerMask.NameToLayer("Island")) | (1 << LayerMask.NameToLayer("Projectile")) |
                                  (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("EnemyProjectile")) |
                                  (1 << LayerMask.NameToLayer("ResupplyBase"));
        Scene0();
        Invoke(nameof(Scene1), 1f);
    }

    void Scene0()
    {
        Vector3 pos;
        Vector3 rot;
        if (myTeam == 0)
        {
            pos = new Vector3(-20f, 700f, -322f);
            rot = new Vector3(90f, 100f, 0f);
        }
        else
        {
            pos = new Vector3(755f, 700f, -717f);
            rot = new Vector3(90f, -80f, 0f);
        }
        
        cam1 = Instantiate(cameraPrefab, pos, Quaternion.Euler(rot));
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 1;
    }

    //fly over
    void Scene1()
    {
        Vector3 pos;
        Vector3 rot;
        if (myTeam == 0)
        {
            pos = new Vector3(755f, 700f, -717f);
            rot = new Vector3(90f, 100f, 0f);
        }
        else
        {
            pos = new Vector3(-20f, 700f, -322f);
            rot = new Vector3(90f, -80f, 0f);
        }

        cam2 = Instantiate(cameraPrefab, pos, Quaternion.Euler(rot));
        cam2.GetComponent<CinemachineVirtualCamera>().Priority = 0;

        cam1.name = "FirstCam";
        cam2.name = "SecondCam";
        cam2.GetComponent<CinemachineVirtualCamera>().Priority = 1;
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        Invoke(nameof(Scene2), 3f);
    }

    //focus on your ship
    void Scene2()
    {
        Vector3 centerDir = (mapCenter - transform.position).normalized;
        cam1.transform.position = shipTransform.position + (centerDir * -20f);
        cam1.transform.LookAt(transform.root.GetChild(0).GetChild(0));

        cam1.name = "ThirdCam";
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 1;
        cam2.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        Invoke(nameof(Scene3), 2f);
    }

    //orbit
    void Scene3()
    {
        GameObject target = new GameObject();
        target.transform.position = shipTransform.position;
        cam2.transform.parent = target.transform;
        cam2.transform.position = cam1.transform.position;
        cam2.GetComponent<CinemachineVirtualCamera>().Priority = 1;
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        StartCoroutine(Orbit(target, cam2));
        Invoke(nameof(Scene4), 5f);
    }

    //missile turret
    void Scene4()
    {
        Destroy(cam1);
        Destroy(cam2);
        cam3 = Instantiate(cameraPrefab);
        cam3.GetComponent<CinemachineDollyCart>().m_Path = path2;
        cam3.GetComponent<CinemachineDollyCart>().m_Speed = 8f;
        cam3.GetComponent<CinemachineVirtualCamera>().Priority = 1;
        Invoke(nameof(Transition), 4f);
    }

    void Transition()
    {
        cam1 = Instantiate(cameraPrefab);
        cam1.GetComponent<CinemachineDollyCart>().m_Path = path1;
        cam1.GetComponent<CinemachineDollyCart>().m_Speed = 0f;

        cam3.name = "PathCam1";
        cam1.name = "PathCam2";
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 1;
        cam3.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        Invoke(nameof(Scene5), 3.9f);
    }

    //fly thru
    void Scene5()
    {
        Destroy(cam3);
        if (myTeam == 0)
        {
            cam1.GetComponent<CinemachineDollyCart>().m_Speed = 100f;
        }
        else
        {
            cam1.GetComponent<CinemachineDollyCart>().m_Speed = 90f;
        }
        Invoke(nameof(Scene6), 9f);
    }


    void Scene6()
    {
        cam2 = Instantiate(cameraPrefab);
        Vector3 pos = myBase.transform.position + 80f * (mapCenter - myBase.transform.position).normalized;
        pos.y -= 30f;
        cam2.transform.position = pos;
        cam2.transform.LookAt(myBase.transform);

        cam1.name = "SixthCam";
        cam2.name = "SeventhCam";
        cam2.GetComponent<CinemachineVirtualCamera>().Priority = 1;
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        Invoke(nameof(Scene7), 2f);
    }

    void Scene7()
    {
        cam1 = Instantiate(cameraPrefab, cam2.transform.position + new Vector3(0f, 60f, 0), cam2.transform.rotation);
        cam1.transform.LookAt(myBase.transform);
        cam2.name = "FourthCam";
        cam1.name = "FifthCam";
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 1;
        cam2.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        Invoke(nameof(Scene8), 4f);
    }

    void Scene8()
    {
        shipTransform.GetComponent<CameraController>().cameraObj.ForceCameraPosition(shipTransform.position + -30f*(mapCenter - transform.position).normalized, Quaternion.identity);
        shipTransform.GetComponent<CameraController>().cameraObj.Priority = 1;
        cam1.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        Invoke(nameof(FinishIntro), 2f);
    }

    IEnumerator Orbit(GameObject target, GameObject cam)
    {
        float time = 0f;
        while (time < 5f)
        {
            cam.transform.LookAt(target.transform);
            cam.transform.Translate(Vector3.right * Time.deltaTime * 20f);
            time += Time.deltaTime;
            yield return null;
        }
    }

    void FinishIntro()
    {
        Camera.main.cullingMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("TransparentFX")) |
                                  (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Water")) |
                                  (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("PostProcessing")) |
                                  (1 << LayerMask.NameToLayer("Island")) | (1 << LayerMask.NameToLayer("Projectile")) |
                                  (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("EnemyProjectile")) |
                                  (1 << LayerMask.NameToLayer("ResupplyBase")) | (1 << LayerMask.NameToLayer("MapBoundary"));
        introDone = true;
        canvas.SetActive(true);
    }
}