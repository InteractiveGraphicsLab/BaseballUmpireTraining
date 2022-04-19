Shader "UICustom/CircleStencil"
{
   Properties {
       _MainTex ("Texture", 2D) = "white" {}
       _EdgeColor ("EdgeColor", Color) = (1,1,1,1)
       _EdgeWidth ("EdgeWidth", float) = 0.45
       _AlphaMask("Mask texture", 2D) = "white" {}

       _StencilComp ("Stencil Comparison", Float) = 8
       _Stencil ("Stencil ID", Float) = 0
       _StencilOp ("Stencil Operation", Float) = 0
       _StencilWriteMask ("Stencil Write Mask", Float) = 255
       _StencilReadMask ("Stencil Read Mask", Float) = 255
       _ColorMask ("Color Mask", Float) = 15
   }
   SubShader {
       Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
       LOD 200
       Lighting Off
       ZWrite Off
       ZTest Off
       Blend SrcAlpha OneMinusSrcAlpha

       Pass {
            Stencil {
                Ref 1
                Comp equal
                Pass keep
            }

           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag

           #include "UnityCG.cginc"

           struct appdata {
               float4 vertex : POSITION;
               float2 uv : TEXCOORD0;
           };

           struct v2f {
               float2 uv : TEXCOORD0;
               float4 vertex : SV_POSITION;
           };

           sampler2D _MainTex;
           sampler2D _AlphaMask;
           float4 _MainTex_ST;
           fixed4 _EdgeColor;
           float _EdgeWidth;

           v2f vert (appdata v) {
               v2f o;
               o.vertex = UnityObjectToClipPos(v.vertex);
               o.uv = TRANSFORM_TEX(v.uv, _MainTex);
               return o;
           }

           fixed4 frag (v2f i) : SV_Target {
               fixed4 col = tex2D(_MainTex, i.uv);
               col.a *= tex2D(_AlphaMask, i.uv).a;
               clip (col.a - 0.001);
               fixed radius = 0.5;
               fixed center = distance(i.uv, fixed2(0.5, 0.5));
               return (1 - step(radius, center)) * (step(_EdgeWidth, center) ? _EdgeColor : col);
           }
           ENDCG
       }
   }
}