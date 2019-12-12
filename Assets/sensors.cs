using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sensors : MonoBehaviour
{
    public int N;
    public int R;
    public int rr;
    public int T;
    private float scale = 1;
    private float PScale=1;
    private bool pause = false;
    private bool start = false;
    private Vector3 trans = new Vector3(0,0,0);
    public static List<GameObject> spheres = new List<GameObject>();
    private List<GameObject> cubes=new List<GameObject>();
    private List<GameObject> Lines = new List<GameObject>();
    private List<GameObject> ChildImages = new List<GameObject>();
    private GameObject parent;

    int test = 0;
    System.Random rnd = new System.Random();
    //int test = 0;
    GameObject cube;
    int src;
    int tgt;
    bool allReceived = true;
    GameObject pa;
    Transform Origin;

    Button btn_stop;
    Button btn_pause;
    Button btn_start;
    Button btn_subm;
    InputField FieldR;
    InputField BroadR;
    InputField SensorN;
    InputField SlotN;
    Slider PropSpeed;
    Slider SensorSize;
    Slider EdgeWidth;

    GameObject cam;

    Canvas cvs;

    private Vector3[] linePositions = new Vector3[3];

    private float lastDis;
    private Vector3 lastPos;
    private float lastRoty;
    private float lastRotx;
    private float lastRotz;

    public GameObject leftCtrl;
    public GameObject rightCtrl;


    [System.Serializable]
    public class SensorItem
    {
        public double x;
        public double y;
        public double z;
        public int id;
        public int action;
        public double radius;
        public double energy;
        public int load;
    }
    public static List<SensorItem> Sensors = new List<SensorItem>();

    public class PacketItem
    {
        public GameObject item;
        public int id;
        public Vector3 destination;
        public int dest_id;
    }
    public static List<PacketItem> Packets = new List<PacketItem>();
    List<GameObject> dustbin = new List<GameObject>();
    public class PrePacketItem
    {
        public int id;
        public int par_id;
    }
    List<PrePacketItem> PacketsPrepared = new List<PrePacketItem>();

    int[,] nc;
    int[,] edge;
    List<int> prepared=new List<int>();

    int Timer = 0;
    int flag = 1;
    int flag2 = 0;
    // Start is called before the first frame update
    void Start()
    {
        cvs = GameObject.Find("Canvas").GetComponent<Canvas>();
        //sphereCanvas.SetActive(false);

        parent = GameObject.Find("Canvas");

        pa = GameObject.Find("GameObject");
        Origin = pa.transform;
        Debug.Log(Origin.position.x);
        Debug.Log(Origin.position.y);
        Debug.Log(Origin.position.z);
        Debug.Log(Origin.rotation.x);
        Debug.Log(Origin.rotation.y);
        Debug.Log(Origin.rotation.z);
        Debug.Log(Origin.localScale.x);
        Debug.Log(Origin.localScale.y);
        Debug.Log(Origin.localScale.z);
        /*cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = pa.transform;
        cube.transform.position = spheres[src].transform.position;
        Color cubeColor = new Color();
        ColorUtility.TryParseHtmlString("#d00729", out cubeColor);
        cube.GetComponent<Renderer>().material.color = cubeColor;
        cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);*/

        ConstructNet();

        FieldR = GameObject.Find("Parameter of the Field").GetComponent<InputField>();
        FieldR.onEndEdit.AddListener(delegate { getR(FieldR); });

        BroadR = GameObject.Find("Parameter of the broadcast radius").GetComponent<InputField>();
        BroadR.onEndEdit.AddListener(delegate { get_rr(BroadR); });

        SensorN = GameObject.Find("The number of sensors").GetComponent<InputField>();
        SensorN.onEndEdit.AddListener(delegate { getN(SensorN); });

        SlotN = GameObject.Find("The number of slots").GetComponent<InputField>();
        SlotN.onEndEdit.AddListener(delegate { getT(SlotN); });

        PropSpeed = GameObject.Find("Speed adjust").GetComponent<Slider>();
        PropSpeed.onValueChanged.AddListener(delegate { getSpeed(PropSpeed); });

        SensorSize = GameObject.Find("sensor size adjust").GetComponent<Slider>();
        SensorSize.onValueChanged.AddListener(delegate { getSize(SensorSize); });

        EdgeWidth = GameObject.Find("edge width adjust").GetComponent<Slider>();
        EdgeWidth.onValueChanged.AddListener(delegate { getWidth(EdgeWidth); });

        btn_subm = GameObject.Find("Submit").GetComponent<Button>();
        btn_subm.onClick.AddListener(ConstructNet);

        btn_start=GameObject.Find("Start").GetComponent<Button>();
        btn_start.onClick.AddListener(BeginProp);

        btn_stop = GameObject.Find("Stop").GetComponent<Button>();
        btn_stop.onClick.AddListener(StopProp);

        btn_pause = GameObject.Find("Pause").GetComponent<Button>();
        btn_pause.onClick.AddListener(delegate { PauseProp(btn_pause); });
    }

    void ConstructNet()
    {
        pause = true;
        foreach (PacketItem p in Packets)
        {
            Destroy(p.item);
        }
        Packets.Clear();
        foreach (GameObject g in dustbin)
        {
            Destroy(g);
        }
        dustbin.Clear();
        PacketsPrepared.Clear();
        prepared.Clear();

        start = false;

        pa.transform.localScale = pa.transform.localScale / scale;
        pa.transform.position = Origin.transform.position;
        //pa.transform.position = pa.transform.position - trans;
        foreach (GameObject g in cubes)
        {
            Destroy(g);
        }
        cubes.Clear();

        Sensors.Clear();
        foreach (GameObject s in spheres)
            Destroy(s);
        spheres.Clear();
        foreach (GameObject l in Lines)
            Destroy(l);
        Lines.Clear();

        foreach (GameObject g in ChildImages)
        {
            Destroy(g);
        }
        ChildImages.Clear();

        PScale = 1;
        Sensors.Add(new SensorItem() { x = 0, y = pa.transform.position.y, z = 0, id = 0, action = 0, radius = rr, energy=1000, load = 0 });
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject cube;
        Color sphereColor = new Color();
        ColorUtility.TryParseHtmlString("#00ECFF", out sphereColor);
        sphere.GetComponent<Renderer>().material.color = sphereColor;
        sphere.transform.parent = pa.transform;
        sphere.transform.position = new Vector3(0, pa.transform.position.y, 0);
        //sphere.transform.localScale = pa.transform.localScale;
        spheres.Add(sphere);
        //create others


        for (int i = 0; i <= T - 1; i++)
        {
            GameObject NewObj = new GameObject();
            NewObj.transform.parent = parent.transform;
            NewObj.AddComponent<Image>();
            NewObj.GetComponent<RectTransform>().position = new Vector3(-2.8f + i * 0.25f, -0.95f, 5.05f);
            NewObj.GetComponent<RectTransform>().localScale = new Vector3(1.5f, 1.9f, 1.0f);
            NewObj.GetComponent<RectTransform>().sizeDelta = new Vector3(0.2f,0.5f);
            //ColorUtility.TryParseHtmlString("#FFFFFF", out sphereColor);
            NewObj.name = i.ToString();
            //NewObj.AddComponent<Renderer>();
            //NewObj.GetComponent<Renderer>().material.color = sphereColor;
            ChildImages.Add(NewObj);
        }

        nc = new int[N + 1, N + 1];
        edge = new int[N+1,N+1];
        int flag = 0;
        while (flag == 0)
        {
            Sensors.Clear();
            Sensors.Add(new SensorItem() { x = 0, y = pa.transform.position.y, z = 0, id = 0, action = 0, radius = rr, energy = 0.0, load = 0 });
            generateNodes(R, N, T);
            for (int i = 0; i <= N; i++)
                for (int j = 0; j <= N; j++)
                    nc[i, j] = 0;
            findNeighbors();
            if (connected(nc, N))
            {
                flag = 1;
            }

        }

        for (int i = 1; i < Sensors.Count; i++)
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphereColor = new Color();
            ColorUtility.TryParseHtmlString("#49FF00", out sphereColor);
            sphere.GetComponent<Renderer>().material.color = sphereColor;
            sphere.transform.parent = pa.transform;
            sphere.transform.position = new Vector3((float)Sensors[i].x, pa.transform.position.y, (float)Sensors[i].z);
            //sphere.transform.localScale = scale;
            spheres.Add(sphere);

            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (Sensors[i].energy>=R/4)
                ColorUtility.TryParseHtmlString("#5EB246", out sphereColor);
            if (Sensors[i].energy < R/4 && Sensors[i].energy >= R/8)
                ColorUtility.TryParseHtmlString("#EEB932", out sphereColor);
            if (Sensors[i].energy < R/8)
                ColorUtility.TryParseHtmlString("#C21443", out sphereColor);
            cube.GetComponent<Renderer>().material.color = sphereColor;
            cube.transform.parent = pa.transform;
            cube.transform.position = new Vector3((float)Sensors[i].x+0.6f, pa.transform.position.y+ (float)Sensors[i].energy/20.0f, (float)Sensors[i].z+0.6f);
            cube.transform.localScale = new Vector3(0.3f,(float)Sensors[i].energy/10.0f,0.3f);
            cubes.Add(cube);
        }
        //create edges
        for (int i = 0; i <= N - 1; i++)
        {
            for (int j = i + 1; j <= N; j++)
                if (nc[i, j] == 1)
                {
                    src = i;
                    tgt = j;
                    edge[i, j] = Lines.Count;
                    DrawConnection(spheres[i].transform.position, spheres[j].transform.position);
                }
        }
    }

    void getN(InputField N2)
    {
       N= toInt(N2.text);
        Debug.Log(toInt(N2.text));
    }

    void getR(InputField R2)
    {
        R = toInt(R2.text);
        Debug.Log(toInt(R2.text));
    }

    void get_rr(InputField rr2)
    {
        rr = toInt(rr2.text);
        Debug.Log(toInt(rr2.text));
    }

    void getT(InputField T2)
    {
        T = toInt(T2.text);
        Debug.Log(toInt(T2.text));
    }

    int toInt(string s)
    {
        int result = 0;
        for (int i=0; i<s.Length; i++)
        {
            result = result * 10 + s[i] - 48;
        }
        return result;
    }

    void getSpeed(Slider speed)
    {
        Time.timeScale = speed.value * 10+0.5f;
        //TimeScale = speed.value * 10;
    }

    void getSize(Slider size)
    {
        PScale = size.value * 10;
        foreach (GameObject s in spheres)
        {
            s.transform.localScale = new Vector3(size.value * 10, size.value * 10, size.value * 10);
            
            //s.transform.position = new Vector3(s.transform.position.x, pa.transform.position.y, s.transform.position.z);
        }
        foreach (PacketItem p in Packets)
        {
            p.item.transform.localScale= new Vector3(size.value * 10, size.value * 10, size.value * 10);
        }
        /*foreach (GameObject g in cubes)
        {
            g.transform.localScale = new Vector3(size.value * 10, 1, size.value * 10);
        }*/
    }

    void getWidth(Slider width)
    {
        foreach (GameObject l in Lines)
        {
            l.GetComponent<LineRenderer>().startWidth = width.value/10;
            l.GetComponent<LineRenderer>().endWidth = width.value/10;
        }
    }

    void StopProp()
    {
        Color color = new Color();
        pause = true;
        start = false;
        foreach (PacketItem p in Packets)
        {
            Destroy(p.item);
        }
        Packets.Clear();
        foreach (GameObject g in dustbin)
        {
            Destroy(g);
        }
        dustbin.Clear();
        PacketsPrepared.Clear();
        prepared.Clear();
        for (int i= 1; i < spheres.Count; i++)
        {
            ColorUtility.TryParseHtmlString("#49FF00", out color);
            spheres[i].GetComponent<Renderer>().material.color = color;
        }
    }

    void PauseProp(Button b)
    {
        if (!pause)
        {
            pause = true;
            b.GetComponentInChildren<Text>().text = "Resume";
        }
        else
        {
            pause = false;
            b.GetComponentInChildren<Text>().text = "Pause";
        }
    }

    void BeginProp()
    {
        pause = false;
        start = true;
        foreach (PacketItem p in Packets)
        {
            Destroy(p.item);
        }
        Packets.Clear();
        foreach (GameObject g in dustbin)
        {
            Destroy(g);
        }
        dustbin.Clear();
        PacketsPrepared.Clear();
        prepared.Clear();
        prepared.Add(1);
        for (int i = 1; i < Sensors.Count; i++)
            prepared.Add(0);
        checkReady(0);
        if (!allReceived || Packets.Count!=0)
        sending();
    }

    void checkReady(int id)
    {
        allReceived = true;
        for (int i = 0; i < prepared.Count; i++)
            if (prepared[i] == 0) allReceived = false;

        for (int i = 1; i < Sensors.Count; i++)
            if (nc[id,i] == 1 && prepared[i] == 0)
            {
                PrePacketItem PrePacket = new PrePacketItem();
                PrePacket.par_id = id;
                PrePacket.id = i;
                PacketsPrepared.Add(PrePacket);
                prepared[i] = 1;
            }
    }

    void sending()
    {
        int flag = 0;
        foreach (GameObject g in ChildImages)
            g.GetComponent<Image>().color = Color.white;
        foreach (GameObject g in ChildImages)
            if (g.name == Timer.ToString())
                g.GetComponent<Image>().color = Color.red;
            for (int i=0; i<PacketsPrepared.Count; i++)
        {
            if (Sensors[PacketsPrepared[i].id].action==Timer)
            {
                PacketItem newPacket = new PacketItem();
                newPacket.item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newPacket.item.transform.parent = pa.transform;
                newPacket.item.transform.position = spheres[PacketsPrepared[i].par_id].transform.position;
                Color cubeColor = new Color();
                ColorUtility.TryParseHtmlString("#d00729", out cubeColor);
                newPacket.item.GetComponent<Renderer>().material.color = cubeColor;
                newPacket.item.transform.localScale = new Vector3(PScale,PScale,PScale);
                newPacket.id = PacketsPrepared[i].par_id;
                newPacket.dest_id = PacketsPrepared[i].id;
                newPacket.destination = spheres[PacketsPrepared[i].id].transform.position;
                Packets.Add(newPacket);
                PacketsPrepared.Remove(PacketsPrepared[i]);
                flag = 1;
            }
        }
        if (flag==0 && PacketsPrepared.Count>0)
        {
            Timer = (Timer + 1) % T;
            sending();
        }
    }

    void DrawConnection(Vector3 start, Vector3 end)
    {
        Vector3 tmp = new Vector3(0, Vector3.Distance(start, end) / 10, 0);
        linePositions[0] = start;
        linePositions[1] = (start + end) / 2 + tmp;
        linePositions[2] = end;

        GameObject myLine = new GameObject();
        GameObject pa = GameObject.Find("GameObject");
        myLine.AddComponent<LineRenderer>();
        LineRenderer line = myLine.GetComponent<LineRenderer>();
        myLine.transform.parent = pa.transform;
        //line.transform.parent = pa.transform;
        line.useWorldSpace = false;
        //get smoothed values
        Vector3[] smoothedPoints = LineSmoother.SmoothLine(linePositions, 0.08f);

        //set line settings
        line.positionCount = smoothedPoints.Length;
        line.SetPositions(smoothedPoints);
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;

        Lines.Add(myLine);
    }

    void generateNodes(int R, int N, int T)
    {
        int rndx, rndz, dv2, rndt;
        int[,] tmpp = new int[R + 1, R + 1];
        Vector3 tmp1;
        
        for (int i = 0; i <= R; i++)
            for (int j = 0; j <= R; j++)
                tmpp[i, j] = 0;
        for (int i = 2; i <= N + 1; i++)
        {
            while (true)
            {
                rndx = rnd.Next(-(R / 2), R / 2 + 1);
                rndz = rnd.Next(-(R / 2), R / 2 + 1);
                rndt = rnd.Next(0, T);
                dv2 = rndx * rndx + rndz * rndz;
                if (tmpp[rndx + R / 2, rndz + R / 2] == 0 && dv2 < R * R)
                    break;
            }
            tmp1 = new Vector3(rndx, pa.transform.position.y, rndz);
            Sensors.Add(new SensorItem() { x = rndx, y = pa.transform.position.y, z = rndz, id = i-1, action = rndt, radius = rr, energy = Vector3.Distance(tmp1,spheres[0].transform.position), load = 0 });
            tmpp[rndx + R / 2, rndz + R / 2] = 1;
        }
    }

    bool connected(int[,] nc, int N)
    {
        List<int> stack = new List<int>();
        List<int> flag = new List<int>();
        int top = 1, tmp;
        for (int i = 0; i <= N; i++)
            stack.Add(0);
        for (int i = 0; i <= N; i++)
            flag.Add(0);
        flag[0] = 1;
        while (top >= 1)
        {
            tmp = stack[top - 1];
            top--;
            for (int i = 1; i <= N; i++)
                if (flag[i] == 0 && nc[tmp, i] == 1)
                {
                    top++;
                    stack[top - 1] = i;
                    flag[i] = 1;
                }
        }
        for (int i = 0; i <= N; i++)
        {
            if (flag[i] == 0) return false;
        }
        return true;
    }

    void findNeighbors()
    {
        for (int i = 0; i <= N - 1; i++)
        {
            for (int j = i + 1; j <= N; j++)
            {
                if ((Sensors[i].x - Sensors[j].x) * (Sensors[i].x - Sensors[j].x) + (Sensors[i].z - Sensors[j].z) * (Sensors[i].z - Sensors[j].z) <= rr * rr)
                {
                    nc[i, j] = 1;
                    nc[j, i] = 1;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //cam = GameObject.Find("PacketsPrepared");
        Color color = new Color();
        //cvs.transform.position = cvs.transform.position+ cvs.transform.forward*1.0f;
        //cvs.transform.rotation = cvs.transform.rotation;
        //cvs.transform.rotation = cam.transform.rotation;
        //cvs.transform.position = cam.transform.position + cam.transform.forward * 1.0f;
        if (!pause)
        {
            flag = 1;
            for (int i = 0; i < Packets.Count; i++)
            {
                if (Vector2.Dot(Packets[i].item.transform.position - Packets[i].destination, spheres[Packets[i].id].transform.position - Packets[i].destination) > 0)
                {
                    ColorUtility.TryParseHtmlString("#FFEA6D", out color);
                    spheres[Packets[i].dest_id].GetComponent<Renderer>().material.color = color;
                    Packets[i].item.transform.position += (Packets[i].destination - spheres[Packets[i].id].transform.position)/ 10 * Time.deltaTime;
                    flag = 0;
                }
                flag2 = 1;
            }
            if (flag == 1 && flag2 == 1)
            {
                Timer = (Timer + 1) % T;
                for (int i = 0; i < Packets.Count; i++)
                {
                    ColorUtility.TryParseHtmlString("#49FF00", out color);
                    spheres[Packets[i].dest_id].GetComponent<Renderer>().material.color = color;
                    checkReady(Packets[i].dest_id);
                    foreach (PrePacketItem p in PacketsPrepared)
                    {
                        if (p.id == Packets[i].dest_id) PacketsPrepared.Remove(p);
                    }
                }
                foreach (PacketItem p in Packets)
                {
                    Packets.Remove(p);
                    dustbin.Add(p.item);
                }
                if (!allReceived || PacketsPrepared.Count != 0)
                    sending();
                flag2 = 0;
            }
            if (allReceived && PacketsPrepared.Count==0)
            {
                start = false;
            }
        }


        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            cvs.enabled = false;
        }

        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            cvs.enabled = true;
        }

        if (!start)
        {
            UpdateObjectPosition();
            UpdateObjectRotation();
            UpdateObjectScale();

            lastPos = AverPos();
            lastDis = Dis();
            lastRoty = Roty();
            lastRotx = Rotx();
            lastRotz = Rotz();
        }
       
    }

    void UpdateObjectPosition()
    {
        GameObject pa = GameObject.Find("GameObject");
        if (Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.75 && Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.75)
        {
            Vector3 transVector = AverPos() - lastPos;
            trans = trans + transVector;
            pa.transform.position += transVector;
            Debug.Log("ok");
        }

    }

    void UpdateObjectRotation()
    {
        //OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && OVRInput.Get(OVRInput.RawButton.RIndexTrigger)
        if (Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.75 && Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.75)
        {
            float RotAmounty = (Roty() - lastRoty);
            float RotAmountz = (Rotz() - lastRotz);
            float RotAmountx = (Rotx() - lastRotx);
            //pa.transform.Rotate(RotAmountx, RotAmounty, RotAmountz);
            pa.transform.RotateAround(AverPos(), Vector3.up, RotAmounty);
            /*pa.transform.RotateAround(AverPos(), Vector3.right, RotAmountx);
            pa.transform.RotateAround(AverPos(), Vector3.forward, RotAmountz);*/
            Debug.Log("ok");
        }

    }

    void UpdateObjectScale()
    {
        if (Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.75 && Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.75)
        {
            float scaleFactor = 1 + (Dis() - lastDis);
            ScaleObjectAroundPoint(pa, AverPos(), scaleFactor);
            scale = scale * scaleFactor;
            Debug.Log("ok");
        }
    }

    float Dis()
    {
        return Vector3.Distance(leftCtrl.transform.position, rightCtrl.transform.position);
    }

    Vector3 AverPos()
    {
        return (leftCtrl.transform.position + rightCtrl.transform.position) / 2;
    }

    float Roty()
    {
        return Mathf.Atan2(leftCtrl.transform.position.z - rightCtrl.transform.position.z, leftCtrl.transform.position.x - rightCtrl.transform.position.x) * Mathf.Rad2Deg * -1.0f;
    }

    float Rotx()
    {
        return Mathf.Atan2(leftCtrl.transform.position.z - rightCtrl.transform.position.z, leftCtrl.transform.position.y - rightCtrl.transform.position.y) * Mathf.Rad2Deg * -0.1f;
    }

    float Rotz()
    {
        return Mathf.Atan2(leftCtrl.transform.position.x - rightCtrl.transform.position.x, leftCtrl.transform.position.y - rightCtrl.transform.position.y) * Mathf.Rad2Deg * -0.1f;
    }


    void ScaleObjectAroundPoint(GameObject objectToScale, Vector3 pivotPoint, float amountToScaleBy)
    {
        float minScale = 0.0001f;
        Vector3 a = objectToScale.transform.position;
        Vector3 endScale = objectToScale.transform.localScale * amountToScaleBy;

        if (endScale.x < minScale)
        {
            endScale = new Vector3(minScale, minScale, minScale);
        }

        Vector3 c = a - pivotPoint;
        Vector3 finalPosition = (c * amountToScaleBy) + pivotPoint;

        objectToScale.transform.localScale = endScale;
        objectToScale.transform.position = finalPosition;
    }
}
