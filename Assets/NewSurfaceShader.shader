// Parameters:
// - Wind Direction;
// - Wind Wave Size;
// - Tree Sway Speed;
// - Tree Sway Displacement;
// - Tree Sway Stutter;
// - Foliage Wiggle Amount;
// - Foliage Wiggle Speed;
// - Branches Up/Down;
// - Red Vertex Influence;
// - Blue Vertex Influence.

Shader "Tree Wind" {

	Properties{
		_MainTex("Main Texture", 2D) = "white" {}
		_UV("UV map", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_Offset("Offset", Float) = 1

	}

		SubShader{
			Tags  { "Queue" = "Background" }
			LOD 100
			Cull off
			CGPROGRAM

			#pragma target 3.0
			#pragma surface surf Lambert vertex:vert addshadow 


			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _UV;
			float _Offset;


			struct Input {
				float2 uv_MainTex;
				float2 uv : TEXCOORD0;
				float3 viewDir;
			};
			struct appdata {
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;	//Texture
				float4 uv2 : TEXCOORD1;	//Physics
				float4 uv3 : TEXCOORD2;	//Growth
			};

			// Vertex Manipulation Function
			void vert(inout appdata_full i) {
				appdata o;
				o.uv = float4(i.texcoord.xy, 0, 0);
				o.uv2 = float4(i.texcoord1.xy, 0, 0);
				//float uvMap = tex2D(_UV, o.uv2.xy).r;
				float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
				//i.vertex.x *= clamp(_Time.z*(1.01-o.uv.y), 0, 1);
				//i.vertex.y *= clamp(_Time.z*(1.01-o.uv.y), 0, 1);

				i.vertex.x += cos(_Time.z*1.2)*o.uv2.y*0.1f;
				i.vertex.y += sin(_Time.z*1.9)*o.uv2.y*0.05f;
				// i.normal = o.uv;
				if (o.uv2.y > .95f) {
				   i.vertex.x += cos(_Time.z * 10 + worldPos.x + worldPos.y)*o.uv2.y*0.01f;
				   i.vertex.y += cos(_Time.z * 6 + worldPos.x + worldPos.y)*o.uv2.y*0.005f;
				}
			}
			 void surf(Input IN, inout SurfaceOutput o) {
				 fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				 o.Albedo = _Color.rgb  * _Color.a;
				 o.Emission = c.rgb.rgb;
			 }
	 ENDCG
		}

			Fallback "Diffuse"
}