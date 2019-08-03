using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameHandler : MonoBehaviour
{
    Scene newScene;
    GameObject gameManager;
    Camera mainCamera;

    GameObject eventHandler;

    GameObject boat;
    float horizontalSpeed = 0.1f;
    float jumpSpeed = 0.35f;
    bool isInAir = true;
    Vector3 velocity;

    public Material superMaterial;
    GameObject fadeImg;
    TextMeshProUGUI scoreText;
    Vector3 startPos;
    Quaternion targetRot;

    GameObject water;
    WaterLine theWaterLine;
    public List<GameObject> rocks = new List<GameObject>();
    float score;
    float difficulty = 1.0f;
    float diffTimer;
    float startDiffTimer = 3.0f;

    string[] shaderText = new string[46]
    {
        "Shader \"Custom/SuperShader\"",
        "{",
        "Properties",
        "{",
        "_MainTex (\"Texture\", 2D) = \"white\" {}",
        "_Color(\"Color\", Color) = (1, 1, 1, 1)",
        "}",
        "SubShader",
        "{",
        "Tags { \"RenderType\"=\"Opaque\" }",
        "LOD 100",
        "Pass",
        "{",
        "CGPROGRAM",
        "#pragma vertex vert",
        "#pragma fragment frag",
        "#include \"UnityCG.cginc\"",
        "struct appdata",
        "{",
        "float4 vertex : POSITION;",
        "float2 uv : TEXCOORD0;",
        "};",
        "struct v2f",
        "{",
        "float2 uv : TEXCOORD0;",
        "float4 vertex : SV_POSITION;",
        "};",
        "sampler2D _MainTex;",
        "fixed4 _Color;",
        "float4 _MainTex_ST;",
        "v2f vert(appdata v)",
        "{",
        "v2f o;",
        "o.vertex = UnityObjectToClipPos(v.vertex);",
        "o.uv = TRANSFORM_TEX(v.uv, _MainTex);",
        "return o;",
        "}",
        "fixed4 frag (v2f i) : SV_Target",
        "{",
        "fixed4 col = tex2D(_MainTex, i.uv) * _Color;",
        "return col;",
        "}",
        "ENDCG",
        "}",
        "}",
        "}"
    };

    public void StartGame()
    {
        diffTimer = startDiffTimer;
        GameObject mainCameraObj = new GameObject();
        mainCameraObj.transform.name = "MainCamera";
        mainCameraObj.AddComponent<Camera>();
        mainCamera = mainCameraObj.GetComponent<Camera>();
        mainCamera.orthographic = true;
        mainCamera.transform.position = new Vector3(0.0f, 4.0f, -10.0f);
        mainCamera.transform.Rotate(15.0f, 0.0f, 0.0f);

        CreateShader();

        CreateMaterial();

        //player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boat = new GameObject();
        boat.transform.name = "Boat";
        CreateBoat(boat);

        GameObject boatMast = new GameObject();
        boatMast.transform.name = "BoatMast";
        CreateQuad(boatMast);
        boatMast.transform.localScale = new Vector3(0.3f, 2.0f, 1.0f);
        boatMast.transform.Translate(0.4f, -2.0f, 0.0f);
        boatMast.transform.SetParent(boat.transform);
        GiveColor(boatMast, new Color(0.25f, 0.1f, 0.1f, 1.0f));

        boat.transform.Rotate(0.0f, 0.0f, 180.0f);
        Vector3 tempPos = mainCamera.ViewportToWorldPoint(new Vector2(0.2f, 0.13f));
        tempPos.z = 0.5f;
        boat.transform.position = tempPos;
        boat.transform.position -= Vector3.up * 1.5f;
        startPos = boat.transform.position;
        targetRot = boat.transform.rotation;
        //bottomLeftOfScreen = Camera.main.ViewportToWorldPoint(new Vector2(0f, 0f)); 
        GiveColor(boat, new Color(0.25f, 0.1f, 0.1f, 1.0f));

        gameManager = new GameObject();
        gameManager.transform.name = "OneScript";
        gameManager.AddComponent<OneScript>();

        water = new GameObject();
        water.transform.name = "Water";
        water.transform.position -= Vector3.up * 2.0f;
        water.AddComponent<WaterLine>();
        theWaterLine = water.GetComponent<WaterLine>();

        // UI
        GameObject mainCanvas = new GameObject();
        mainCanvas.transform.name = "MainCanvas";
        mainCanvas.AddComponent<Canvas>();
        mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        fadeImg = new GameObject();
        fadeImg.transform.name = "FadeImg";
        fadeImg.transform.SetParent(mainCanvas.transform);
        fadeImg.AddComponent<Image>();
        fadeImg.GetComponent<Image>().color = Color.black;
        StartCoroutine(Fade());
        SetAndStretchToParentSize(fadeImg.GetComponent<RectTransform>(), fadeImg.transform.parent.GetComponent<RectTransform>());
        GameObject scoreObj = new GameObject();
        scoreObj.AddComponent<TextMeshProUGUI>();
        scoreObj.transform.name = "ScoreText";
        scoreObj.transform.SetParent(mainCanvas.transform);
        scoreObj.GetComponent<RectTransform>().anchorMin = new Vector2(1.0f, 1.0f);
        scoreObj.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 1.0f);
        scoreObj.GetComponent<RectTransform>().pivot = new Vector2(1.0f, 1.0f);
        scoreObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //scoreObj.GetComponent<RectTransform>().position = new Vector3(0.0f, -10.0f, 0.0f);
        scoreText = scoreObj.GetComponent<TextMeshProUGUI>();

        StartCoroutine(RockSpawning());
    }

    IEnumerator RockSpawning()
    {
        while(true)
        {
            SpawnRock();
            yield return new WaitForSeconds(3.0f * difficulty);
        }
    }

    void SpawnRock()
    {
        // Rock
        GameObject rock = new GameObject();
        rocks.Add(rock);
        rock.AddComponent<EdgeCollider2D>();
        rock.AddComponent<DestoyRock>();
        rock.transform.name = "Rock";
        CreateRock(rock);
        GiveColor(rock, Color.gray);
        Vector3 tempVec = mainCamera.ViewportToWorldPoint(new Vector2(1.2f, -0.13f));
        tempVec.z = 0.2f;
        rock.transform.position = tempVec;
        Destroy(rock, 10.0f);
    }

    void Update()
    {
        score += Time.deltaTime;
        scoreText.text = score.ToString("F2");
        diffTimer -= Time.deltaTime;
        if(diffTimer < 0.0f)
        {
            diffTimer = startDiffTimer;
            difficulty -= 0.05f;
        }

        foreach(GameObject obj in rocks)
        {
            obj.transform.Translate(Vector3.right * -0.14f);
        }
        boat.transform.rotation = Quaternion.RotateTowards(boat.transform.rotation, targetRot, 25.0f * Time.deltaTime);
        Vector3 tempVec = boat.transform.position;
        tempVec.x = Mathf.Lerp(tempVec.x, startPos.x, 2.0f * Time.deltaTime);
        boat.transform.position = Vector3.Lerp(tempVec, startPos, 2.0f * Time.deltaTime);
        BoatMovement();
    }

    IEnumerator Fade()
    {
        float lerp = 0.0f;
        while(lerp < 1.0f)
        {
            fadeImg.GetComponent<Image>().color = Color.Lerp(Color.black, Color.clear, lerp);
            lerp += Time.deltaTime;
            yield return null;
        }
    }

    void BoatMovement()
    {
        velocity.y -= 0.01f;
        if(boat.transform.position.y <= startPos.y)
        {
            boat.transform.position = new Vector3(boat.transform.position.x, startPos.y, boat.transform.position.z);
            velocity.y = 0;
            isInAir = false;
        }

        if(Input.GetButtonDown("Jump") && !isInAir)
        {
            theWaterLine.parts[19].gameObject.transform.Translate(0.0f, 2.0f, 0.0f);
            boat.transform.Rotate(0.0f, 0.0f, 20.0f);
            velocity.y += jumpSpeed;
            isInAir = true;
        }

        velocity.z = 0;
        boat.transform.Translate(0.0f, -velocity.y, 0.0f); 
    }

    // https://answers.unity.com/questions/1007886/how-to-set-the-new-unity-ui-rect-transform-anchor.html
      public void SetAndStretchToParentSize(RectTransform _mRect, RectTransform _parent)
    {
        _mRect.anchoredPosition = _parent.position;
        _mRect.anchorMin = new Vector2(0, 0);
        _mRect.anchorMax = new Vector2(1, 1);
        _mRect.pivot = new Vector2(0.5f, 0.5f);
        _mRect.sizeDelta = _parent.rect.size;
        _mRect.transform.SetParent(_parent);
    }

    void AddTri(Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(1, 0, 0));
        vertices.Add(new Vector3(1, 1, 0));

        mesh.vertices = vertices.ToArray();


        List<int> indices = new List<int>();

        indices.Add(0);
        indices.Add(2);
        indices.Add(1);

        mesh.triangles = indices.ToArray();


        List<Vector3> normals = new List<Vector3>();

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        mesh.normals = normals.ToArray();


        List<Vector2> uvs = new List<Vector2>();

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));

        mesh.uv = uvs.ToArray();
    }

    void CreateTri(GameObject obj)
    {
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        obj.GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(1, 0, 0));
        vertices.Add(new Vector3(1, 1, 0));

        mesh.vertices = vertices.ToArray();


        List<int> indices = new List<int>();

        indices.Add(0);
        indices.Add(2);
        indices.Add(1);

        mesh.triangles = indices.ToArray();


        List<Vector3> normals = new List<Vector3>();

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        mesh.normals = normals.ToArray();


        List<Vector2> uvs = new List<Vector2>();

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));

        mesh.uv = uvs.ToArray();
        obj.GetComponent<MeshRenderer>().material = superMaterial;
    }

    void CreateQuad(GameObject obj)
    {
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        obj.GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(1, 0, 0));
        vertices.Add(new Vector3(0, 1, 0));
        vertices.Add(new Vector3(1, 1, 0));

        mesh.vertices = vertices.ToArray();


        List<int> indices = new List<int>();

        indices.Add(0);
        indices.Add(2);
        indices.Add(1);

        indices.Add(2);
        indices.Add(3);
        indices.Add(1);

        mesh.triangles = indices.ToArray();


        List<Vector3> normals = new List<Vector3>();

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        mesh.normals = normals.ToArray();


        List<Vector2> uvs = new List<Vector2>();

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        mesh.uv = uvs.ToArray();
        obj.GetComponent<MeshRenderer>().material = superMaterial;
    }

    void CreateBoat(GameObject obj)
    {
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        obj.GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(1, 0, 0));
        vertices.Add(new Vector3(0, 1, 0));
        vertices.Add(new Vector3(1, 1, 0));

        vertices.Add(new Vector3(1, 0, 0));
        vertices.Add(new Vector3(1, 1, 0));
        vertices.Add(new Vector3(2, 0, 0));

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(-1, 0, 0));
        vertices.Add(new Vector3(0, 1, 0));


        mesh.vertices = vertices.ToArray();


        List<int> indices = new List<int>();

        indices.Add(0);
        indices.Add(2);
        indices.Add(1);

        indices.Add(2);
        indices.Add(3);
        indices.Add(1);

        indices.Add(4);
        indices.Add(5);
        indices.Add(6);

        indices.Add(7);
        indices.Add(8);
        indices.Add(9);

        mesh.triangles = indices.ToArray();


        List<Vector3> normals = new List<Vector3>();

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        mesh.normals = normals.ToArray();

        obj.GetComponent<MeshRenderer>().material = superMaterial;
    }

    void CreateRock(GameObject obj)
    {
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        obj.GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(0.7f, 0.2f, 0));
        vertices.Add(new Vector3(1.0f, 1.0f, 0));

        vertices.Add(new Vector3(0.7f, 0.2f, 0));
        vertices.Add(new Vector3(2.0f, 1.8f, 0));
        vertices.Add(new Vector3(1.0f, 1.0f, 0));

        mesh.vertices = vertices.ToArray();


        List<int> indices = new List<int>();

        indices.Add(0);
        indices.Add(2);
        indices.Add(1);

        indices.Add(3);
        indices.Add(5);
        indices.Add(4);

        mesh.triangles = indices.ToArray();


        List<Vector3> normals = new List<Vector3>();

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        mesh.normals = normals.ToArray();

        obj.GetComponent<MeshRenderer>().material = superMaterial;
    }

    void GiveColor(GameObject objToColor, Color newCol)
    {
        objToColor.GetComponent<MeshRenderer>().material.SetColor("_Color", newCol);
    }

    void CreateShader()
    {
        using(TextWriter tw = File.CreateText("Assets/SuperShader.shader"))
        {
            for(int i = 0; i < shaderText.Length; i++)
            {
                tw.WriteLine(shaderText[i]);
            }
        }
        // StreamWriter writer = new StreamWriter("Assets/superShader.shader", true);
        // for(int i = 0; i < shaderText.Length; i++)
        // {
        //     writer.Write(shaderText[i]);
        // }
    }

    void CreateMaterial()
    {
        superMaterial = new Material(Shader.Find("Custom/SuperShader"));
    }
}

public class DestoyRock : MonoBehaviour
{
    void OnDestroy()
    {
        FindObjectOfType<GameHandler>().rocks.Remove(gameObject);
    }
}

public class OneScript : MonoBehaviour
{
    static Scene newScene;
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        newScene = SceneManager.CreateScene("OneScriptScene");
        SceneManager.LoadScene("OneScriptScene", LoadSceneMode.Single);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("OneScriptScene"));
        
        GameObject go = new GameObject();
        go.transform.name = "GameHandler";
        go.AddComponent<GameHandler>().StartGame();
    }
}

#region water
// https://dmayance.com/water-line-2d-unity/

public struct WaterLinePart
{
  public float height;
  public float velocity;
  public GameObject gameObject;
  public Mesh mesh;
  public Vector2 boundsMin;
  public Vector2 boundsMax;
}

public class WaterLine : MonoBehaviour
{
  public float velocityDamping = 0.4f; // Proportional velocity damping, must be less than or equal to 1.
  public float timeScale = 25f;

  public int Width = 50;
  public float Height = 10f;
  public Material material;
  public Color color = Color.blue;

  public WaterLinePart[] parts;

  private int size;
  private float currentHeight;

#if UNITY_EDITOR
  private bool cleanRequested;
#endif

  void Start()
  {

    material = FindObjectOfType<GameHandler>().superMaterial;

#if UNITY_EDITOR
    // Remove what we see from the editor
    Clear();
#endif

    Initialize();
  }

  private void Initialize()
  {
    size = Width;
    currentHeight = Height;

    material.color = color;

    parts = new WaterLinePart[size];

    // we'll use spheres to represent each vertex for demonstration purposes
    for (int i = 0; i < size; i++)
    {
      // Create a game object
      GameObject go = new GameObject("WavePart");
      go.transform.parent = this.transform;
      go.transform.localPosition = new Vector3(i - (size / 2), 0, 0);

      parts[i].gameObject = go;
    }

    // Create the meshes
    for (int i = 0; i < size; i++)
    {
      GameObject go = parts[i].gameObject;

      // Except for the last point
      if (i < size - 1)
      {
        Mesh mesh = new Mesh();
        mesh.MarkDynamic();
        parts[i].mesh = mesh;

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        // Define vertices for the mesh (the points of the model)
        UpdateMeshVertices(i);

        // Define triangles and normals
        InitializeTrianglesAndNormalsForMesh(i);

        go.GetComponent<MeshFilter>().mesh = mesh;
        go.GetComponent<MeshRenderer>().material = material;
      }
    }

    // Small wave
    Splash(size / 2, 10);
  }

#if UNITY_EDITOR
  /// <summary>
  /// SUPER VIOLENT METHOD FOR EDITOR MODE
  /// </summary>
  private void Clear()
  {
    for (int i = 0; i < size; i++)
    {
      DestroyImmediate(parts[i].mesh);
      DestroyImmediate(parts[i].gameObject);
    }

    parts = null;
  }
#endif

  private void UpdateMeshVertices(int i)
  {
    Mesh mesh = parts[i].mesh;
    if (mesh == null) return;

    Transform current = parts[i].gameObject.transform;

    Transform next = current;
    if (i < parts.Length - 1)
    {
      next = parts[i + 1].gameObject.transform;
    }

    Vector3 left = Vector3.zero;
    Vector3 right = next.localPosition - current.localPosition;

    // Get all parts of the mesh (it's just 2 planes, one on top and one on the front face)
    Vector3 topLeftFront = new Vector3(left.x, left.y, 0);
    Vector3 topRightFront = new Vector3(right.x, right.y, 0);
    Vector3 topLeftBack = new Vector3(left.x, left.y, 1);
    Vector3 topRightBack = new Vector3(right.x, right.y, 1);
    Vector3 bottomLeftFront = new Vector3(left.x, left.y + (0 - Height), 0);
    Vector3 bottomRightFront = new Vector3(right.x, right.y + (0 - Height), 0);

    mesh.vertices = new Vector3[] { topLeftFront, topRightFront, topLeftBack, topRightBack, bottomLeftFront, bottomRightFront };

    parts[i].boundsMin = topLeftFront + current.position;
    parts[i].boundsMax = bottomRightFront + current.position;
  }

  private void InitializeTrianglesAndNormalsForMesh(int i)
  {
    Mesh mesh = parts[i].mesh;
    if (mesh == null) return;

    // Normals
    var uvs = new Vector2[mesh.vertices.Length];
    for (int i2 = 0; i2 < uvs.Length; i2++)
    {
      uvs[i2] = new Vector2(mesh.vertices[i2].x, mesh.vertices[i2].z);
    }
    mesh.uv = uvs;

    // Triangles
    mesh.triangles = new int[] { 5, 4, 0, 0, 1, 5, 0, 2, 3, 3, 1, 0 };

    // For shader
    mesh.RecalculateNormals();
  }

  void Update()
  {
#if UNITY_EDITOR
    // Size has been updated?
    if (Width != size || Height != currentHeight)
    {
      cleanRequested = true;
    }

    // Recalculate everything!
    // This should be for the editor only!
    if (cleanRequested)
    {
      cleanRequested = false;
      Debug.Log("Reinitializing water. Make sure we are in editor mode!");
      Clear();
      Initialize();
    }

    color = material.color;
#endif

    // Water tension is simulated by a simple linear convolution over the height field.
    for (int i = 1; i < size - 1; i++)
    {
#if UNITY_EDITOR
      // Objects deleted from editor
      if (parts[i].gameObject == null)
      {
        cleanRequested = true;
        return;
      }
#endif
      int j = i - 1;
      int k = i + 1;
      parts[i].height = (parts[i].gameObject.transform.localPosition.y + parts[j].gameObject.transform.localPosition.y + parts[k].gameObject.transform.localPosition.y) / 3.0f;
    }

    // Velocity and height are updated...
    for (int i = 0; i < size; i++)
    {
      // update velocity and height
      parts[i].velocity = (parts[i].velocity + (parts[i].height - parts[i].gameObject.transform.localPosition.y)) * velocityDamping;

      float timeFactor = Time.deltaTime * timeScale;
      if (timeFactor > 1f) timeFactor = 1f;

      parts[i].height += parts[i].velocity * timeFactor;

      // Update the dot position
      Vector3 newPosition = new Vector3(
          parts[i].gameObject.transform.localPosition.x,
          parts[i].height,
          parts[i].gameObject.transform.localPosition.z);
      parts[i].gameObject.transform.localPosition = newPosition;
    }

    // Update meshes
    for (int i = 0; i < size; i++)
    {
      UpdateMeshVertices(i);
    }
  }

  #region Interaction

  /// <summary>
  /// Make waves from a point
  /// </summary>
  /// <param name="location"></param>
  /// <param name="force"></param>
  public void Splash(Vector3 location, int force)
  {
    // Find the touched part
    for (int i = 0; i < (size - 1); i++)
    {
      if (location.x >= parts[i].boundsMin.x
        && location.x < parts[i].boundsMax.x)
      {
        if (location.y <= parts[i].boundsMin.y
       && location.y > parts[i].boundsMax.y)
        {
          Splash(i, force);
        }
      }
    }

  }

  private void Splash(int i, int heightModifier)
  {
    parts[i].gameObject.transform.localPosition = new Vector3(
      parts[i].gameObject.transform.localPosition.x,
      parts[i].gameObject.transform.localPosition.y + heightModifier,
      parts[i].gameObject.transform.localPosition.z
      );
  }

  #endregion
}

#endregion