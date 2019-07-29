using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    Scene newScene;
    GameObject gameManager;
    GameObject mainCamera;

    GameObject sunLight;

    GameObject eventHandler;

    GameObject player;
    float horizontalSpeed = 0.1f;
    float jumpSpeed = 0.35f;
    bool isInAir = true;
    Vector3 velocity;

    Material superMaterial;

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
        mainCamera = new GameObject();
        mainCamera.transform.name = "MainCamera";
        mainCamera.AddComponent<Camera>();
        mainCamera.transform.position = new Vector3(0.0f, 4.0f, -10.0f);
        mainCamera.transform.Rotate(15.0f, 0.0f, 0.0f);

        sunLight = new GameObject();
        sunLight.transform.name = "SunLight";
        sunLight.AddComponent<Light>().type = LightType.Directional;

        CreateShader();

        CreateMaterial();

        //player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player = new GameObject();
        player.transform.name = "Player";
        CreateMesh(player);

        gameManager = new GameObject();
        gameManager.transform.name = "OneScript";
        gameManager.AddComponent<OneScript>();
    }

    void Update()
    {
        PlayerMovement();
    }

    void CreateMesh(GameObject obj)
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
        obj.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
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

    void PlayerMovement()
    {
        float x = Input.GetAxis("Horizontal");

        velocity.x = x * horizontalSpeed;
        
        velocity.y -= 0.01f;
        if(player.transform.position.y <= -3.0f)
        {
            player.transform.position = new Vector3(player.transform.position.x, -3.0f, player.transform.position.z);
            velocity.y = 0;
            isInAir = false;
        }

        if(Input.GetButtonDown("Jump") && !isInAir)
        {
            velocity.y += jumpSpeed;
            isInAir = true;
        }

        velocity.z = 0;
        player.transform.Translate(velocity); 
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