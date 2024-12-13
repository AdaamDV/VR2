Shader "Unlit/HeatMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DisableHeatmap ("Disable Heatmap", Float) = 1
        _pIntensity ("Point Intensity", Float) = 0.5
        _Color0("Color 0", Color) = (0,0,0,1)
        _Color1("Color 1", Color) = (0,.9,.2,1)
        _Color2("Color 2", Color) = (.9,1,.3,1)
        _Color3("Color 3", Color) = (.9,.7,.1,1)
        _Color4("Color 4", Color) = (1,0,0,1)
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
            float _DisableHeatmap = 1; // Declare the property as a float

            float _Hits[2350]; //intensiteit van de 2350, op de juiste volgorde
            int _HitCount = 0;
            float _aoe_size = .05;//1.0f is 25% van een 4x4
            float _pIntensity = 0.5;

            float4 _Color0;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;


            void init()
            {
                colors[0] = _Color0;
                colors[1] = _Color1;
                colors[2] = _Color2;
                colors[3] = _Color3;
                colors[4] = _Color4;

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

            float3 getHeatForPixel(float weight)
            {
                if(weight <= pointranges[0])
                {
                    return colors[0];
                }
                if(weight >= pointranges[4])
                {
                    return colors[4];
                }

                for(int i=0; i < 5; i++)
                {
                    if(weight < pointranges[i])
                    {
                        float dist_from_lower_point = weight - pointranges[i - 1];
                        float size_of_point_range = pointranges[i] - pointranges[i - 1];

                        float ratio_over_lower_point = dist_from_lower_point/size_of_point_range;

                        float3 color_range = colors[i] - colors[i-1];
                        float3 color_contribution = color_range*ratio_over_lower_point;

                        float3 new_color = colors[i-1] + color_contribution;
                        return new_color;
                    }
                }
                return colors[0];
            }

            float2 getXY(int i)
            {
                //first we get the original system's XY coordinates
                float originX = i % 50 + 0.5f; //midden van het hokje
                float originY = floor(i/50) + 0.5f; //weer midden van het hokje
                //now we transform into the (-2,-2)-(2,2) coordinate system:
                return float2(-2.0f+originY/11.75f,2.0f-originX/12.25f);
            }

            float4 heatMapAddition(float2 uv){
                uv = uv*4 - float2(2.0,2.0); //changes uv coordinate range to -2 to 2

                float totalWeight = 0;
                float scale = _pIntensity;
                for(float i = 0; i < _HitCount; i++)
                {
                    float2 work_pt = getXY(i); //the x and y coordinate based on the structure
                    float pt_intensity = _Hits[i];

                    totalWeight += scale * distsq(uv,work_pt)*100 * pt_intensity;
                }// for i
                float3 heat = getHeatForPixel(totalWeight);
                return float4(heat,0.1);
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
                return col + heatMapAddition(i.uv);
            }
            ENDCG

        }
    }
}
