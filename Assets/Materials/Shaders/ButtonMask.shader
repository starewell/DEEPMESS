// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Cg/ButtonMask"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
 
    SubShader
    {
        Tags {"Queue"="Transparent+1" "IgnoreProjector"="True"}
        ZWrite Off
        AlphaTest Greater 0.5
        ColorMask 0
        ZTest Always
 
 
        Stencil {
            Ref 1
            Comp always
            Pass replace
        }
    
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                    
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
                        
            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                //if(col.a<0.1)discard;
                return col * _Color;
            }
            ENDCG
        }
    }
 
}
