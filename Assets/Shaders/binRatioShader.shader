Shader "Unlit/binRatioShader
"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DisableHeatmap ("Disable Heatmap", Float) = 1
        _pIntensity ("Point Intensity", Float) = 1.0 
        _Color0("Color 0", Color) = (0,0,0,1)
        _Color1("Color 1", Color) = (0,.9,.2,1)
        _Color2("Color 2", Color) = (.9,1,.3,1)
        _Color3("Color 3", Color) = (.9,.7,.1,1)
        _Color4("Color 4", Color) = (1,0,0,1)
        _GridWidth ("Grid Width", Float) = 50
        _GridHeight ("Grid Height", Float) = 47
        _TextureWidth ("Texture Width", Float) = 908
        _TextureHeight ("Texture Height", Float) = 855
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            //unity stuff hierboven, hieronder eigen code
            //-----------------------------------------------------
            float4 colors[5];
            float pointranges[5];
            float _DisableHeatmap; // Declare the property as a float

            float _Hits[2350]; //intensiteit van de 2350, op de juiste volgorde
            int _HitCount = 0;
            float _aoe_size = .05;//1.0f is 25% van een 4x4
            float _pIntensity = 1.0;
            float _GridWidth;          // Number of bins in X
            float _GridHeight;         // Number of bins in Y
            float _TextureWidth;       // Texture width in pixels
            float _TextureHeight;      // Texture height in pixels
            //we temporarily don't need these colors anymore
            // float4 _Color0;
            // float4 _Color1;
            // float4 _Color2;
            // float4 _Color3;
            // float4 _Color4;


            void init()
            {
                colors[0] = (0,.9,.2,1);
                //we temporarily don't need these colors anymore
                // colors[0] = _Color0;
                // colors[1] = _Color1;
                // colors[2] = _Color2;
                // colors[3] = _Color3;
                // colors[4] = _Color4;

                pointranges[0] = 0;
                pointranges[1] = 0.25;
                pointranges[2] = 0.5;
                pointranges[3] = 0.75;
                pointranges[4] = 1.0;
                // //We add our own points just for testing
                // _HitCount = 2;
                // _Hits[0*3] = 0; //x
                // _Hits[0*3+1] = 0; //y
                // _Hits[0*3+2] = 1; //intensity

                // _Hits[1*3] = 0.5;
                // _Hits[1*3+1] = 0.5;
                // _Hits[1*3+2] = 1;
            }

            float distsq(float2 a, float2 b) //a is uv, b is workpoint
            {
                float d = pow(max(0.0,1.0 - distance(a,b)/_aoe_size), 2);

                return d;
            }

            float3 getHeatForPixel(float weight, float ratio) //uses both hue for ratio and weight for whiteness and intensity on court
            {
                // Normalize the weight (ensure it's between 0 and 1)
                float normalizedIntensity = weight; // Clamp to [0, 1]
                float normRatio = ratio;

                //Rainbow:
                //We need to obtain the color of the pixel based on the ratio: we go from red to blue.
                //normRatio is a value between 0 and 0, when divided in 4 quarters:
                //first quarter: green up to 1.0 -> 0.0 is pure red
                //second quarter: red down to 0 -> 0.5 is green
                //third quarter: blue up to 1.0
                //forth quarter: green down to 0 -> 1.0 is blue
                // float redValue = -4*max(0.25f,normRatio) + 4*max(0.5f,normRatio);
                // float greenValue = 4*min(0.25f,normRatio) -4*max(0.75f,normRatio)+3;
                // float blueValue = 4*max(0.5f,normRatio) - 4*max(0.75f,normRatio)+1;

                //red to green:

                float redValue = -2*max(0.5,normRatio)+2;
                float greenValue = 2*min(0.5,normRatio);
                float blueValue = 0.0;
                float3 baseColor = float3(redValue,greenValue, blueValue); // Red and blue
                // Scale the base color's intensity
                float3 colorContribution = baseColor * max(normalizedIntensity*100.0, 1.0); // Modulate green intensity

                // // Add whiteness based on intensity
                float3 whiteness = float3(normalizedIntensity, normalizedIntensity, normalizedIntensity);

                return colorContribution + 0.25*whiteness; // Combine red,blue with whiteness
            }

            float2 getXY(int i)
            {
                //first we get the original system's XY coordinates
                float originX = i % 50 + 0.5f; //midden van het hokje
                float originY = floor(i/50) + 0.5f; //weer midden van het hokje
                //now we transform into the (-2,-2)-(2,2) coordinate system:
                return float2(-2.0f+originY/11.75f,2.0f-originX/12.25f);
            }
            float findBinWeight(float2 uv){
                float uvx = (1-uv.y);
                float uvy = uv.x;
                int k = (floor(uvy*47.0)*50.0 + floor(uvx*50.0));
                return floor(_Hits[k])/100.0;

                // float uvx = (1-uv.y);
                // float uvy = uv.x;
                // float kf = floor(uvx*47.0)*50.0 + floor(uvy*50.0);
                // return kf/2350.0;
                // int k = floor(uvx*47.0)*50 + floor(uvy*50.0);
                // //return the weight
                // return floor(_Hits[k])/100.0;
            }
            float findBinIntensity(float2 uv){
                float uvx = (1-uv.y);
                float uvy = uv.x;
                int k = (floor(uvy*47.0)*50.0 + floor(uvx*50.0));
                return _Hits[k] - floor(_Hits[k]);
            }

            float4 heatMapAddition(float2 uv){//we will have a block/bin approach
                //uv = uv*4 - float2(2.0,2.0); //changes uv coordinate range to -2 to 2
                float weightVal = findBinWeight(uv);
                float intensVal = findBinIntensity(uv);
                // float3 heat = getHeatForPixel(1.0,weightVal);
                // return float4(heat,1);
                //return float4((1-weightVal),weightVal,0,1);
                float3 heat2 = getHeatForPixel(intensVal, weightVal);
                return float4(heat2,1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Check if heatmap calculations are disabled
                if (_DisableHeatmap > 0)
                {
                    return col;
                }
                //We have to run the calculations for the heatmap
                init();
                float2 uv = i.uv;
                float uvx = (1-uv.y);
                float uvy = uv.x;
                int k = (floor(uvy*47.0)*50.0 + floor(uvx*50.0));
                if(_Hits[k] > 0){
                    return col + heatMapAddition(uv);
                    //return col + float4((1-_Hits[k]/100.0),_Hits[k]/100.0,0,1);
                }
                return col;
                // float2 uv = i.uv;
                // float uvx = (1-uv.y);
                // float uvy = uv.x;
                // // if(uvx < 0.2 && uvy < 0.3){ <- werkt!
                // //     return col + float4(floor(_Hits[54])/100.0, 1-(floor(_Hits[54])/100.0), 0, 1.0);
                // // }
                // // if( uvx > 0.8 && uvy > 0.9){
                // //     return col + float4(floor(_Hits[56])/100.0, 1-(floor(_Hits[56])/100.0), 0, 1.0);
                // // }
                // int kf = (floor(uvy*47.0)*50.0 + floor(uvx*50.0));
                // return col + float4(floor(_Hits[kf])/100.0, 1-(floor(_Hits[kf])/100.0),0,1.0);
                // return col + float4(kf,kf,kf,1);
                // int k = floor(uvx*47.0)*50 + floor(uvy*50.0);
                // //return the weight
                // return floor(_Hits[k])/100.0;
                // // if((floor((1-uv.x)*47)*50 + floor((1-uv.y)*50)) % 50 == 20){
                // //     return col + float4(1,0,0,1);
                // // }
                // // if(uvx > (20.0/50.0) && uvx < (29.0/50.0) && uvy <= (10.0/47.0))
                // // {
                // //     return col + float4(1,0,0,1);
                // // }
                // // if(uvx > 50.0/100.0)
                // // {
                // //     return col + float4(1,0,0,1);
                // // }
                return col + heatMapAddition(i.uv);
            }
            ENDCG

        }
    }
}

// float3 getHeatForPixel(float weight)//sets red as standard color and applies some whiteness if there are lots of points
            // {
            //     // Normalize the weight (ensure it's between 0 and 1)
            //     float normalizedIntensity = saturate(weight); // Clamp to [0, 1]

            //     // Base color is green
            //     float3 baseColor = float3(1.0, 0.0, 0.0); // Pure red

            //     // Scale the base color's intensity
            //     float3 colorContribution = baseColor * normalizedIntensity; // Modulate green intensity

            //     // Add whiteness based on intensity
            //     float3 whiteness = float3(normalizedIntensity, normalizedIntensity, normalizedIntensity);

            //     return colorContribution + 0.5*whiteness; // Combine green with whiteness
            // }
            // float3 getHeatForPixel(float weight)
            // {
            //     if(weight <= pointranges[0])
            //     {
            //         return colors[0];
            //     }
            //     if(weight >= pointranges[4])
            //     {
            //         return colors[4];
            //     }

            //     for(int i=0; i < 5; i++)
            //     {
            //         if(weight < pointranges[i])
            //         {
            //             float dist_from_lower_point = weight - pointranges[i - 1];
            //             float size_of_point_range = pointranges[i] - pointranges[i - 1];

            //             float ratio_over_lower_point = dist_from_lower_point/size_of_point_range;

            //             float3 color_range = colors[i] - colors[i-1];
            //             float3 color_contribution = color_range*ratio_over_lower_point;

            //             float3 new_color = colors[i-1] + color_contribution;
            //             return new_color;
            //         }
            //     }
            //     return colors[0];
            // }