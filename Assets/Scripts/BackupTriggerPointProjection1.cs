using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Mathematics;

public class BackupTriggerPointProjection : MonoBehaviour
{
    //Public variables:
    public int maxNumberOfPoints = 10000; //maximum amount of points we can read from a file
    public float aoeSize = 0.1f; //heatmap area of effect: 1.0f is 25% van een 4x4
    public float pIntensity = 0.5f; //intensity of each point's light contribution
    public string filePath2 = "Data/testData.csv"; // File path relative to the Assets folder
    public Texture originalTexture; //is assigned in the editor to be the texture
    
    //Private variables:
    private Material objectMaterial; // The material instance specific to this object
    private Vector2[] readPoints;// Points that are read in
    private List<float> gridSums = new List<float>(); //empty,will be expanded
    private int currentPointCount = 0; // Tracks the number of valid points
    private int boxesAmount = 2350; //Amount of point we read
    private int textureWidth = 855; //width of the texture of the court
    private int textureHeight = 905; //height of the texture of the court

    void Start()
    {
        //initialise the amount of points we will import
        //kan beter: we kunnen misschien deze informatie al scannen uit de csv?
        readPoints = new Vector2[maxNumberOfPoints];

        // Get the material instance applied to this object's renderer
        objectMaterial = GetComponent<Renderer>().material;

        // Just a check: We don't want calculations for a heatmap when we start
        objectMaterial.SetFloat("_DisableHeatmap", 1); // Initialize as "heatmap disabled"

        // Another check: we want the original texture to be on the field first.
        // This is assigned in the inspector, but we can double check here:
        if (originalTexture != null)
        {
            objectMaterial.SetTexture("_MainTex", originalTexture);
            Debug.Log("Original texture assigned to material at start.");
        }
        else
        {
            Debug.LogWarning("Original texture is not assigned in the Inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //we need to trigger the shader update when 'H' is pressed
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.LogWarning("H is pressed!");
            //Get the points from the file into the shader:
            ReadCSV(); //function that reads and assigns the points into the points vector
            transformPointToGrid();
            SendPointsToShader();//send the points to the shader

            //Render the shader by exporting and importing:
            renderShader();
        }
    }
    void ReadCSV()
    {
        // Get the path to the CSV file in your Unity project (relative to Assets folder)
        string path = Path.Combine(Application.dataPath, filePath2);

        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path); // Read all lines from the CSV file
            int pointIndex = 0; // Index to keep track of where we are in the points array
            currentPointCount = 0;
            // Iterate through each line in the file
            foreach (string line in lines)
            {
                // Skip lines that are empty or start with "//" (to ignore comments)
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;
                // Split the line by comma
                string[] values = line.Split(',');
                // Ensure there are exactly two values in the CSV line
                if (values.Length == 2)
                {
                    // Parse the X and Y values from the CSV, convert them to floats
                    if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
                    {
                        // Check if we haven't exceeded the maxPoints
                        if (pointIndex < maxNumberOfPoints)
                        {
                            // Create the Vector3 with the transformed values (add 0.0f as the Z value, we set this later)
                            readPoints[pointIndex] = new Vector2(x, y);
                            if(readPoints[pointIndex] == new Vector2(0,0)){
                                Debug.Log("eerste failure");
                            }
                            currentPointCount++;
                            // Move to the next point
                            pointIndex++;
                        }
                        else{Debug.LogWarning("Exceeded the maximum number of points."); break;}
                    }
                    else{Debug.LogError("Failed to parse line: " + line);}
                }
                else{Debug.LogError("Invalid line format: " + line);}
            }
        }
        else{Debug.LogError("File not found: " + path);}
    }

    //will convert the readPoints vector to the gridAmounts float
    //after, it converts this system to the points by normalising the intensities for each grid point
    Vector2 Transform(float x, float y){
        return new Vector2(-2.0f+y/11.75f,2.0f-x/12.25f);
    }
    void transformPointToGrid()
    {
        //We assume the grid to be 50 wide, and 47 high. The counting will be done as follows:
        //2300 2301 2302 ... 2348 2349
        //:    :    :     /  :    : 
        //50   51   53   ... 98   99
        //0    1    2    ... 48   49
        //where 0-49 is the baseline of a basketbal field. We obtain each point and calculate where the point is in the grid
        //we then up the counter of this by one, and add tot the total sum
        
        //first we need to make all the sums 0:
        for(int i = 0; i < boxesAmount; i++){
            gridSums.Add(0.0f);
        }
        //now intensities
        for (int i=0; i< currentPointCount; i++)
        {
            float floorX = Mathf.Floor(readPoints[i].x);
            float floorY = Mathf.Floor(readPoints[i].y);
            gridSums[(int)(floorY*50+floorX)] += 1.0f;
        }
        //we have yet to add a normalising constant, for now we divide all the points by the sum
        for(int i = 0; i < boxesAmount; i++){
            gridSums[i] = gridSums[i]*100/currentPointCount;
        }
        Debug.Log(currentPointCount);
    }

    private void SendPointsToShader()
    {
        // Convert the points array into a flattened float array
        float[] hitData = new float[boxesAmount]; // bottom half of points, each with 3 values (x, y, intensity)
        //float[] hitData2 = new float[2350*3]; // top half of point, each with 3 values (x, y, intensity)
        for (int k = 0; k < boxesAmount; k++)
        {
            hitData[k] = gridSums[k];//just the intensity
        }
        // Send the hit data and hit count to the shader
        objectMaterial.SetFloatArray("_Hits", hitData); //send the intensities
        objectMaterial.SetInt("_HitCount", boxesAmount);//we always get 2350 hits
        objectMaterial.SetFloat("_aoe_size", aoeSize); //send the desired aoe
        objectMaterial.SetFloat("_pIntensity", pIntensity);
    }

    void renderShader()
        {
            // We turn on the feature of the shader to do calculations:
            objectMaterial.SetFloat("_DisableHeatmap", 0); // Enable heatmap calc in shader
            Debug.Log("Heatmap calculations enabled in shader.");

            // Now we save the heatmap to PNG and apply the newly created texture
            SaveHeatmapToPNG(); // Get a PNG of the texture, and save the texture

            // //one option: apply from the png
            // ApplyPNGToMaterial();  // Apply the texture (in PNG form) to the material

            //another option: apply from the texture
            LoadAndPutTextureFromFile();
            objectMaterial.SetFloat("_DisableHeatmap", 1); // Disable heatmap in shader
            Debug.Log("Heatmap calculations disabled in shader."); // Disable further calculations
        }
    void SaveHeatmapToPNG()
    {
        // Create a temporary RenderTexture
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        // Set up the material's main texture and render it to the RenderTexture
        Graphics.Blit(null, renderTexture, objectMaterial);

        // Create a Texture2D to read pixels from the RenderTexture
        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Copy the RenderTexture to the Texture2D
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        // Convert to gamma space if needed
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color linearColor = texture.GetPixel(x, y);
                    texture.SetPixel(x, y, linearColor.gamma); // Convert to gamma
                }
            }
            texture.Apply();
        }

        // // Save as PNG to a runtime-safe location as we can load it in later
        // string filePath = Path.Combine(Application.persistentDataPath, "Heatmap.png");
        // byte[] pngData = texture.EncodeToPNG();
        // File.WriteAllBytes(filePath, pngData);
        // Debug.Log("Saved heatmap texture to: " + filePath);

        //another possibility: Save the entire texture to the runtime-safe location:
        // Get the texture's raw data
        byte[] textureData = texture.GetRawTextureData();
        
        // Save the raw data to a file
        string filePath2 = Path.Combine(Application.persistentDataPath, "Texture.raw");
        File.WriteAllBytes(filePath2, textureData);
        Debug.Log("Saved texture data to: " + filePath2);
        // Optionally, save additional metadata (e.g., texture format, width, height)
        string metaFilePath = Path.Combine(Application.persistentDataPath, "Texture.raw" + ".meta");
        File.WriteAllText(metaFilePath, $"{texture.width},{texture.height},{(int)texture.format}");
        Debug.Log("Saved texture metadata to: " + metaFilePath);

        // Clean up
        renderTexture.Release();
        Destroy(renderTexture);
        Destroy(texture);
    }

    void ApplyPNGToMaterial()
    {
        //filePath should be build-safe, and be the same as before
        string filePath = Path.Combine(Application.persistentDataPath, "Heatmap.png");
        if (!File.Exists(filePath))
        {
            Debug.LogError("Heatmap texture file not found: " + filePath);
            return;
        }

        //We load the png and convert it to a texture:
        byte[] pngData = File.ReadAllBytes(filePath);
        Texture2D loadedTexture = new Texture2D(2, 2); // Initialize a small dummy texture
        loadedTexture.LoadImage(pngData); // Load PNG data into the texture

        //We load the texture onto the material
        objectMaterial.SetTexture("_MainTex", loadedTexture);
        Debug.Log("Applied PNG texture to material from: " + filePath);
    }

    void LoadAndPutTextureFromFile()
    {
        // Construct file paths
        string filePath = Path.Combine(Application.persistentDataPath, "Texture.raw");
        string metaFilePath = Path.Combine(Application.persistentDataPath, "Texture.raw" + ".meta");

        if (!File.Exists(filePath) || !File.Exists(metaFilePath))
        {
            Debug.LogError("Texture file or metadata not found: " + filePath);
            return;
        }

        // Read metadata
        string metaData = File.ReadAllText(metaFilePath);
        string[] metaParts = metaData.Split(',');
        int width = int.Parse(metaParts[0]);
        int height = int.Parse(metaParts[1]);
        TextureFormat format = (TextureFormat)int.Parse(metaParts[2]);

        // Read raw texture data
        byte[] textureData = File.ReadAllBytes(filePath);

        // Create and load the texture
        Texture2D loadedTexture = new Texture2D(width, height, format, false);
        loadedTexture.LoadRawTextureData(textureData);
        loadedTexture.Apply();

        Debug.Log("Loaded texture data from: " + filePath);
        if (loadedTexture != null)
        {
            objectMaterial.SetTexture("_MainTex", loadedTexture);
        }
        else
        {
            Debug.Log("the loadedTexture was null!");
        }
    }
}
